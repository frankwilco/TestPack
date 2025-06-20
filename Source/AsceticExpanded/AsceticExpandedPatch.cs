using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse.AI;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch]
    [HarmonyPatchCategory(TestPackConstants.kAsceticExpandedCategory)]
    public static class AsceticExpandedPatch
    {
        // This prevents the "undignified throneroom" alert from showing up
        // for pawns with the ascetic trait.
        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Pawn_RoyaltyTracker),
            nameof(Pawn_RoyaltyTracker.AnyUnmetThroneroomRequirements))]
        public static bool AnyUnmetThroneroomRequirements_Prefix(
            ref bool __result,
            Pawn_RoyaltyTracker __instance)
        {
            // Ignore throneroom requirements if our pawn has the
            // ascetic trait.
            if (__instance.pawn.story.traits.HasTrait(TraitDefOf.Ascetic))
            {
                __result = false;
                return false;
            }
            return true;
        }

        // This prevents pawns with the ascetic trait from complaining or
        // having an opinion about their throneroom.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(JobDriver_Reign), "MakeNewToils")]
        public static IEnumerable<Toil> JobDriver_Reign_MakeNewToils_Postfix(
            IEnumerable<Toil> __values,
            JobDriver_Reign __instance)
        {
            foreach (Toil value in __values)
            {
                // Don't modify other toils.
                if (value.debugName != "MakeNewToils")
                {
                    yield return value;
                }

                // It's a PITA to modify this using a transpiler. Since it's
                // unlikely that this method will change anytime soon, we'll
                // just try to override the toil's tick action to ignore the
                // memory checks. If the meditation tick method does not exist
                // or changes in a future game version, this will safely
                // fallback to the original tick action.
                bool tickActionMissing = value.tickAction == null;
                ModUtils.Log($"Was AEP patch applied: {!tickActionMissing}");
                if (tickActionMissing)
                {
                    yield return value;
                }
                Action oldAction = value.tickAction;
                value.tickAction = delegate {
                    bool isAscetic = __instance.pawn.story.traits.HasTrait(
                        TraitDefOf.Ascetic);
                    if (isAscetic)
                    {
                        Traverse traverse = new Traverse(__instance);
                        Traverse tickMethod = traverse.Method("MeditationTick");
                        // Check first if the meditation tick method exists.
                        if (tickMethod.MethodExists())
                        {
                            __instance.rotateToFace = TargetIndex.B;
                            tickMethod.GetValue();
                            return;
                        }
                    }
                    oldAction();
                };
                yield return value;
            }
        }
    }
}
