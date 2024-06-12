using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Verse;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch(typeof(BillStack))]
    [HarmonyPatchCategory(TestPackConstants.kBillStackLimitCategory)]
    public static class BillStackLimitPatch
    {
        // Remove bill stack limit (currently 15).
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BillStack.DoListing))]
        public static IEnumerable<CodeInstruction> DoListing_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var section = new ILSection(
                codes,
                -1,
                2,
                OpCodes.Call,
                (object operand) => ((MethodInfo)operand).Name == "get_Count",
                OpCodes.Ldarg_0,
                OpCodes.Bge_S);
            section.CheckAll();
            if (!section.IsFound)
            {
                Log.Warning("Failed to patch out the bill stack limit.");
            }
            section.Remove();
            return codes.AsEnumerable();
        }
    }
}
