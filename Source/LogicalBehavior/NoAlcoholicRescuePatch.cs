using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch(typeof(HealthAIUtility),
        nameof(HealthAIUtility.WantsToBeRescued))]
    [HarmonyPatchCategory(TestPackConstants.kLogicalBehaviorCategory)]
    public static class NoAlcoholicRescuePatch
    {
        private static bool IsDrunk(Pawn pawn)
        {
            bool alcoholHediff = pawn.health.hediffSet.HasHediff(
                HediffDefOf.AlcoholHigh);

            if (alcoholHediff
                && !pawn.health.HasHediffsNeedingTendByPlayer()
                && pawn.health.hediffSet.BleedRateTotal <= 0.01f)
            {
                return true;
            }

            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(
            ILGenerator generator,
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var alwaysDownedSection = new ILSection(
                codes,
                -1,
                4,
                OpCodes.Call,
                (object operand) => ((MethodInfo)operand).Name == "AlwaysDowned",
                OpCodes.Ldarg_0,
                OpCodes.Ret);
            ModUtils.Log($"Was NAR patch applied: {alwaysDownedSection.IsFound}");
            if (alwaysDownedSection.IsFound)
            {
                Label alwaysDownedCheckArgPushLabel = generator.DefineLabel();
                var isDrunkCheck = new Collection<CodeInstruction>()
                {
                    // We reuse the load argument instruction from the
                    // insertion point.
                    CodeInstruction.Call(
                        typeof(NoAlcoholicRescuePatch),
                        nameof(IsDrunk)),
                    // if (IsDrunk(pawn)) {}
                    new CodeInstruction(OpCodes.Brfalse_S,
                        alwaysDownedCheckArgPushLabel),
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    // return false;
                    new CodeInstruction(OpCodes.Ret),
                    // Create a new load argument instruction for the always
                    // downed conditional check.
                    new CodeInstruction(OpCodes.Ldarg_0)
                    {
                        labels = { alwaysDownedCheckArgPushLabel }
                    },
                };
                codes.InsertRange(
                    alwaysDownedSection.StartIndex + 1,
                    isDrunkCheck);
            }
            return codes;
        }
    }
}
