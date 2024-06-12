using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse.AI;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch]
    public static class AsceticExpandedPatch
    {
        // This prevents the "undignified throneroom" alert from showing up
        // for pawns with the ascetic trait.
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Pawn_RoyaltyTracker),
            nameof(Pawn_RoyaltyTracker.GetUnmetThroneroomRequirements))]
        public static IEnumerable<string> GetUnmetThroneroomRequirements_Postfix(
            IEnumerable<string> __values,
            Pawn_RoyaltyTracker __instance)
        {
            // Ignore throneroom requirements if our pawn has the
            // ascetic trait.
            if (__instance.pawn.story.traits.HasTrait(TraitDefOf.Ascetic))
            {
                yield break;
            }
            foreach (var value in __values)
            {
                yield return value;
            }
        }

        // This prevents pawns with the ascetic trait from complaining or
        // having an opinion about their throneroom.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(JobDriver_Reign), "MakeNewToils")]
        public static IEnumerable<Toil> JobDriver_Reign_MakeNewToils_Postfix(
            IEnumerable<Toil> __values,
            JobDriver_Reign __instance)
        {
            foreach (var value in __values)
            {
                // It's a PITA to modify this using a transpiler. Since it's
                // unlikely that this method will change anytime soon, we'll
                // just try to override the toil's tick action to ignore the
                // memory checks. If the meditation tick method does not exist
                // or changes in a future game version, this will safely
                // fallback to the original tick action.
                var oldAction = value.tickAction;
                value.tickAction = delegate {
                    bool isAscetic = __instance.pawn.story.traits.HasTrait(
                        TraitDefOf.Ascetic);
                    if (isAscetic)
                    {
                        var traverse = new Traverse(__instance);
                        var tickMethod = traverse.Method("MeditationTick");
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
