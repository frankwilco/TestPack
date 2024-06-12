using Verse;

namespace FrankWilco.RimWorld
{
    public class TestPackMod : Mod
    {
        public TestPackMod(ModContentPack content) : base(content)
        {
            ModUtils.Initialize(content.PackageId);
            var targets = new string[]
            {
                // Ascetic Expanded
                TestPackConstants.kAsceticExpandedCategory,
                // Bill Stack Limit remover
                TestPackConstants.kBillStackLimitCategory,
                // Less Visible Overlays
                TestPackConstants.kLessVisibleOverlayCategory,
                // Logical Behaviors
                TestPackConstants.kLogicalBehaviorCategory,
                DisturbedSleepPatch.IsApplicable
                    ? TestPackConstants.kDisturbedSleepCategory
                    : null,
            };
            ModUtils.PatchMultiple(targets);
        }
    }
}
