using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Verse.AI;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch]
    public static class JobSilencerPatch
    {
        private static Collection<MethodBase> targetMethods;
        public static IEnumerable<MethodBase> TargetMethods()
        {
            if (targetMethods == null)
            {
                targetMethods = new Collection<MethodBase>()
                {
                    // I can handle this on my own. No need to lecture me on
                    // how to keep the the little ones safe.
                    AccessTools.Method(
                        typeof(WorkGiver_BringBabyToSafety),
                        nameof(WorkGiver_BringBabyToSafety.NonScanJob)),
                    AccessTools.Method(
                        typeof(JobGiver_BringBabyToSafety),
                        "TryGiveJob")
                };
            }
            return targetMethods;
        }

        public static bool Prefix(ref Job __result)
        {
            __result = null;
            return false;
        }
    }
}
