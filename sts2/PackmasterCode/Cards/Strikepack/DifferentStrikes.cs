// Ported from src/main/java/thePackmaster/cards/strikepack/DifferentStrikes.java
using Packmaster.PackmasterCode.Cards.Starter;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Strikepack;

/// <summary>
/// Deal 6 (+3) damage. Exhaust 1 card. Gain a random Strike (any character) that costs 1 less this combat and
/// Exhausts; upgraded if this is.
/// </summary>
public sealed class DifferentStrikes : AbstractStrikePackCard
{
    public DifferentStrikes() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithTags(CardTag.Strike);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);

        var toExhaust = await CommonActions.SelectSingleCard(this, CardSelectorPrefs.ExhaustSelectionPrompt, ctx, PileType.Hand);
        if (toExhaust != null) await CardCmd.Exhaust(ctx, toExhaust);

        var strikes = ModelDb.AllCards
            .Where(c => c.Tags.Contains(CardTag.Strike) && c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
            .ToList();
        var model = strikes.Count > 0 ? Wiz.CardRng(Owner).NextItem(strikes)! : ModelDb.Card<Strike>();
        var strike = Wiz.CreateCard(model, Owner);
        strike.EnergyCost.AddThisCombat(-1);
        if (IsUpgraded && strike.IsUpgradable) CardCmd.Upgrade(strike);
        if (!strike.Keywords.Contains(CardKeyword.Exhaust)) CardCmd.ApplyKeyword(strike, CardKeyword.Exhaust);
        await Wiz.MakeInHand(strike, Owner);
    }
}
