using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch(typeof(Pawn), "CheckForDisturbedSleep")]
    public class DisturbedSleepPatch
    {
        private const int kKnownHash = -0x55FB4D02;

        private static bool? _isApplicable;
        public static bool IsApplicable
        {
            get
            {
                if (_isApplicable == null)
                {
                    int hash = ModUtils.GetMethodHash(typeof(Pawn), "CheckForDisturbedSleep");
                    _isApplicable = hash == kKnownHash;
                    if (!_isApplicable.Value)
                    {
                        Log.Warning("[TP] Mismatched hash for disturbed sleep patch.");
                    }
                }
                return _isApplicable.Value;
            }
        }

        public static bool Prefix(Pawn __instance, Pawn source)
        {
            var traverse = new Traverse(__instance);

            // Pawn has no mood.
            if (__instance.needs.mood == null)
            {
                return false;
            }
            // Pawn is either awake or deathresting.
            if (__instance.Awake() || __instance.Deathresting)
            {
                return false;
            }
            // Pawn is not from player faction.
            if (__instance.Faction != Faction.OfPlayer)
            {
                return false;
            }
            // 300 ticks have not yet passed since the last time sleep was disturbed.
            var lastSleepDisturbedTick = traverse.Field<int>("lastSleepDisturbedTick");
            if (Find.TickManager.TicksGame < (lastSleepDisturbedTick.Value + 300))
            {
                return false;
            }

            // Process relationship if source pawn still exists.
            if (source != null)
            {
                // Pawn has a romantic relationship with the source.
                bool hasRelationByPartner =
                    LovePartnerRelationUtility.LovePartnerRelationExists(
                        __instance, source);
                if (hasRelationByPartner)
                {
                    return false;
                }
                // Check direct relationships if it exists.
                if (source.relations != null)
                {
                    // Pawn has a bond with the source (animal).
                    bool hasRelationByBond =
                        source.RaceProps.petness > 0f
                        && source.relations.GetDirectRelation(PawnRelationDefOf.Bond, __instance) != null;
                    if (hasRelationByBond)
                    {
                        return false;
                    }
                    // Pawn is related by blood with the source and does not
                    // have a low opinion.
                    bool hasRelationByBlood =
                        source.relations.FamilyByBlood.Contains(__instance) &&
                        source.relations.OpinionOf(__instance) >= 0;
                    if (hasRelationByBlood)
                    {
                        return false;
                    }
                }
            }

            // Give the pawn the disturbed sleep memory.
            lastSleepDisturbedTick.Value = Find.TickManager.TicksGame;
            __instance.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleepDisturbed);

            return false;
        }
    }
}
