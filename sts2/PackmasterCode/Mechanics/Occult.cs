// Ported from src/main/java/thePackmaster/patches/psychicpack/occult/ (OccultFields, OccultPatch, UseOccultCardCost)
using System.Runtime.CompilerServices;
using HarmonyLib;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Mechanics;

/// <summary>A card class that is always Occult (StS1 set OccultFields.isOccult in the constructor).</summary>
public interface IOccultCard;

/// <summary>A card modifier that makes its card Occult (Lapis Lazuli).</summary>
public interface IGrantsOccult;

/// <summary>
/// Occult cards can be played regardless of anything that makes them unplayable (the Unplayable keyword, hooks,
/// card logic, too little energy). A card played that way Exhausts, and spends at most the energy you have.
/// </summary>
public static class Occult
{
    private static readonly ConditionalWeakTable<CardModel, object> Marked = new();
    // Set by the CanPlay postfix: the card is playable only because it is Occult.
    private static readonly ConditionalWeakTable<CardModel, object> PlayableOnlyByOccult = new();

    public static bool Is(CardModel card) =>
        card is IOccultCard || Marked.TryGetValue(card, out _) ||
        Wiz.Modifiers(card).Any(m => m is IGrantsOccult);

    /// <summary>Make a card instance Occult (StS1 OccultFields.isOccult.set(card, true)). Not copied or saved.</summary>
    public static void Mark(CardModel card) => Marked.AddOrUpdate(card, true);

    private const UnplayableReason Overridable = UnplayableReason.HasUnplayableKeyword | UnplayableReason.BlockedByHook |
                                                 UnplayableReason.BlockedByCardLogic | UnplayableReason.EnergyCostTooHigh |
                                                 UnplayableReason.StarCostTooHigh;

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.CanPlay), [typeof(UnplayableReason), typeof(AbstractModel)],
        [ArgumentType.Out, ArgumentType.Out])]
    private static class CanPlayPatch
    {
        private static void Postfix(CardModel __instance, ref bool __result, ref UnplayableReason reason)
        {
            if (__result)
            {
                PlayableOnlyByOccult.Remove(__instance);
                return;
            }
            if (reason == UnplayableReason.None || (reason & ~Overridable) != 0) return; // no combat, no allies, ...
            if (!Is(__instance)) return;
            reason = UnplayableReason.None;
            __result = true;
            PlayableOnlyByOccult.AddOrUpdate(__instance, true);
        }
    }

    // VERIFY: patching the private async CardModel.SpendEnergy(int); the prefix runs before its state machine starts.
    [HarmonyPatch(typeof(CardModel), "SpendEnergy")]
    private static class SpendEnergyPatch
    {
        private static void Prefix(CardModel __instance, ref int amount)
        {
            if (!PlayableOnlyByOccult.TryGetValue(__instance, out _)) return;
            PlayableOnlyByOccult.Remove(__instance);
            var energy = __instance.Owner?.PlayerCombatState?.Energy ?? 0;
            if (amount > energy) amount = energy;
            if (!__instance.Keywords.Contains(CardKeyword.Exhaust)) CardCmd.ApplyKeyword(__instance, CardKeyword.Exhaust);
        }
    }
}
