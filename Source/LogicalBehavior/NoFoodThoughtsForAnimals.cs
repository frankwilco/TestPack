using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using static RimWorld.FoodUtility;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch(typeof(FoodUtility), nameof(ThoughtsFromIngesting))]
    public class NoFoodThoughtsForAnimals
    {
        // Colonists shouldn't give a damn about the food their animals eat.
        public static bool Prefix(
            ref List<ThoughtFromIngesting> __result,
            Pawn ingester)
        {
            if (ingester != null && ingester.RaceProps.Animal)
            {
                var ingestThoughts =
                    AccessTools.Field(
                        typeof(FoodUtility),
                        "ingestThoughts").GetValue(null) as List<ThoughtFromIngesting>;
                ingestThoughts.Clear();
                __result = ingestThoughts;
                return false;
            }
            return true;
        }
    }
}
