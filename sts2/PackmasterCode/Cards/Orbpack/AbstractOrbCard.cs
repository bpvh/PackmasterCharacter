// Ported from src/main/java/thePackmaster/cards/orbpack/AbstractOrbCard.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Orbpack;

public abstract class AbstractOrbCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : PackmasterCard(cost, type, rarity, target)
{
    protected int EmptyOrbSlots => Owner?.PlayerCombatState?.OrbQueue is { } q ? Math.Max(0, q.Capacity - q.Orbs.Count) : 0;

    /// <summary>StS1 "choose cards in hand to discard" (manual discard).</summary>
    protected async Task<List<CardModel>> DiscardFromHand(PlayerChoiceContext ctx, int min, int max)
    {
        var hand = Wiz.Hand(Owner).Where(c => c != this).ToList();
        if (hand.Count == 0 || max <= 0) return [];
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, Math.Min(min, hand.Count), Math.Min(max, hand.Count));
        var chosen = (await CardSelectCmd.FromHand(ctx, Owner, prefs, c => c != this, this)).ToList();
        if (chosen.Count > 0) await CardCmd.Discard(ctx, chosen);
        return chosen;
    }
}
