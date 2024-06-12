using RimWorld;
using UnityEngine;
using Verse;

namespace FrankWilco.RimWorld
{
    [StaticConstructorOnStartup]
    public static class CustomOverlayResources
    {
        public static readonly Material RefuelForbiddenMat;

        static CustomOverlayResources()
        {
            // Force overlay drawer to reload its resources.
            typeof(OverlayDrawer).TypeInitializer.Invoke(null);
            // Initialize some of our custom resources.
            RefuelForbiddenMat = MaterialPool.MatFrom(
                "UI/Overlays/TP_ForbiddenOverlay_Refuel",
                ShaderDatabase.MetaOverlay);
        }
    }
}
