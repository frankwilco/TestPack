using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch]
    [HarmonyPatchCategory(TestPackConstants.kLogicalBehaviorCategory)]
    public static class AlertSilencerPatch
    {
        private static Collection<MethodBase> targetMethods;
        public static IEnumerable<MethodBase> TargetMethods()
        {
            if (targetMethods == null)
            {
                targetMethods = new Collection<MethodBase>()
                {
                    // Silence expensive and sometimes annoying alerts.
                    AccessTools.Method(
                        typeof(Alert_NoBabyFeeders),
                        nameof(Alert_NoBabyFeeders.GetReport)),
                    AccessTools.Method(
                        typeof(Alert_LowBabyFood),
                        nameof(Alert_LowBabyFood.GetReport)),
                    AccessTools.Method(
                        typeof(Alert_NeedWarmClothes),
                        nameof(Alert_NeedWarmClothes.GetReport)),
                    AccessTools.Method(
                        typeof(Alert_NeedResearchProject),
                        nameof(Alert_NeedResearchProject.GetReport))
                };
            }
            return targetMethods;
        }

        public static bool Prefix(ref AlertReport __result)
        {
            __result = false;
            return false;
        }
    }
}
