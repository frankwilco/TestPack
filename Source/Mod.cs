using System;
using Verse;

namespace FrankWilco.RimWorld
{
    public class TestPackMod : Mod
    {
        public TestPackMod(ModContentPack content) : base(content)
        {
            ModUtils.Initialize(content.PackageId);

            var targets = new Type[]
            {
                // Ascetic Expanded
                typeof(AsceticExpandedPatch),
                // Bill Stack Limit remover
                typeof(BillStackLimitPatch),
                // Less Visible Overlays
                typeof(CustomOverlayPatch),
                // Logical Behaviors
                typeof(AlertSilencerPatch),
                typeof(JobSilencerPatch),
                typeof(LifeThreateningAlertPatch),
                DisturbedSleepPatch.IsApplicable ? typeof(DisturbedSleepPatch) : null,
                typeof(NoAlcoholicRescuePatch),
                typeof(NoFoodThoughtsForAnimals)
            };
            ModUtils.PatchMultiple(targets);
        }
    }
}
