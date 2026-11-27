// Ported from src/main/java/thePackmaster/cards/ringofpainpack/Hollower.java (+ actions/ringofpainpack/HollowerAction.java)
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>0 cost. Exhaust a random card in your hand (upgraded: a card of your choice). If you do, put a card from your discard pile into your hand; it gains Exhaust.</summary>
public sealed class Hollower : AbstractRingOfPainCard
{
    public Hollower() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var hand = Wiz.Hand(Owner).ToList();
        if (hand.Count == 0) return;
        var toExhaust = IsUpgraded
            ? await CommonActions.SelectSingleCard(this, CardSelectorPrefs.ExhaustSelectionPrompt, ctx, PileType.Hand)
            : Wiz.CardRng(Owner).NextItem(hand);
        if (toExhaust == null) return;
        await CardCmd.Exhaust(ctx, toExhaust);

        var fetched = await CommonActions.SelectSingleCard(this, SelectionScreenPrompt, ctx, PileType.Discard);
        if (fetched == null) return;
        await CardPileCmd.Add(fetched, PileType.Hand);
        if (!fetched.Keywords.Contains(CardKeyword.Exhaust)) CardCmd.ApplyKeyword(fetched, CardKeyword.Exhaust);
    }
}
