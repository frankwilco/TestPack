using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace FrankWilco.RimWorld
{
    // Extreme blood loss should not be life threatening if
    // the bleeding was stopped (bleed rate total < 0).
    [HarmonyPatch(typeof(Alert_LifeThreateningHediff))]
    [HarmonyPatchCategory(TestPackConstants.kLogicalBehaviorCategory)]
    public static class LifeThreateningAlertPatch
    {
        private static bool IsBloodLossLifeThreatening(Pawn pawn, Hediff hediff)
        {
            if (hediff.def.defName == "BloodLoss")
            {
                float bleedRateTotal = pawn.health.hediffSet.BleedRateTotal;
                return bleedRateTotal > 0.01f;
            }
            return true;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("SickPawns", MethodType.Getter)]
        public static IEnumerable<CodeInstruction> GetSickPawns_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var immuneCheckSection = new ILSection(
                codes,
                -1,
                1,
                OpCodes.Call,
                (object operand) => ((MethodInfo)operand).Name == "FullyImmune",
                OpCodes.Ldloc_3,
                OpCodes.Brtrue_S);
            ModUtils.Log($"Was LTA patch applied: {immuneCheckSection.IsFound}");
            if (immuneCheckSection.IsFound &&
                immuneCheckSection.EndInstruction != null)
            {
                var bleedRateCheck = new Collection<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    CodeInstruction.Call(
                        typeof(LifeThreateningAlertPatch),
                        nameof(IsBloodLossLifeThreatening)),
                    new CodeInstruction(OpCodes.Brfalse_S,
                        immuneCheckSection.EndInstruction.operand)
                };
                codes.InsertRange(immuneCheckSection.EndIndex + 1, bleedRateCheck);
            }
            return codes.AsEnumerable();
        }
    }
}
