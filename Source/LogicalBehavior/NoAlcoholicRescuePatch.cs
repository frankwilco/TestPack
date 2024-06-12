using HarmonyLib;
using RimWorld;
using Verse;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch(typeof(HealthAIUtility),
        nameof(HealthAIUtility.WantsToBeRescuedIfDowned))]
    public static class NoAlcoholicRescuePatch
    {
        public static bool Prefix(Pawn pawn, ref bool __result)
        {
            var alcoholHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.AlcoholHigh);

            if (alcoholHediff != null
                && !pawn.health.HasHediffsNeedingTendByPlayer()
                && pawn.health.hediffSet.BleedRateTotal <= 0.01f)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
