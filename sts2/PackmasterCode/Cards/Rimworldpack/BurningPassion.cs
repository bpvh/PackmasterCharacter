// Ported from src/main/java/thePackmaster/cards/rimworldpack/BurningPassion.java (+ actions/rimworldpack/BurningPassionAction.java)
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>2 cost. Exhaust your hand (upgraded: any number of cards in it). Draw that many cards and gain that much energy. Exhaust.</summary>
public sealed class BurningPassion : AbstractRimCard
{
    public BurningPassion() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithKeyword(CardKeyword.Exhaust);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var hand = Wiz.Hand(Owner).ToList();
        if (hand.Count == 0) return;
        List<CardModel> toExhaust;
        if (IsUpgraded)
        {
            var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 0, hand.Count);
            toExhaust = (await CardSelectCmd.FromHand(ctx, Owner, prefs, null, this)).ToList();
        }
        else toExhaust = hand;
        if (toExhaust.Count == 0) return;
        foreach (var card in toExhaust) await CardCmd.Exhaust(ctx, card);
        await PlayerCmd.GainEnergy(toExhaust.Count, Owner);
        await CardPileCmd.Draw(ctx, toExhaust.Count, Owner);
    }
}
