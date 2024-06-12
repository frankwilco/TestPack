using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch]
    public static class CustomOverlayPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OverlayDrawer), "RenderForbiddenRefuelOverlay")]
        public static bool RenderForbiddenRefuelOverlay_Prefix(OverlayDrawer __instance, Thing t)
        {
            var traverse = new Traverse(__instance);
            var BaseAlt = traverse.Field<float>("BaseAlt").Value;
            var drawBatch = traverse.Field<DrawBatch>("drawBatch").Value;

            Vector3 pos = t.TrueCenter();
            pos.y = BaseAlt + 15f / 74f;
            new Vector3(pos.x, pos.y + 3f / 74f, pos.z);
            drawBatch.DrawMesh(
                MeshPool.plane08,
                Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one),
                CustomOverlayResources.RefuelForbiddenMat,
                0,
                renderInstanced: true);

            return false;
        }
    }
}
