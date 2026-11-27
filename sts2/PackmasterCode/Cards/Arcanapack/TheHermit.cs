// Ported from src/main/java/thePackmaster/cards/arcanapack/TheHermit.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Arcanapack;

/// <summary>0 cost. Gain 3 Block. Discard a card, then draw cards equal to its cost (X costs: your energy). Exhaust (not when upgraded).</summary>
public sealed class TheHermit : AbstractAstrologerCard
{
    public TheHermit() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithBlock(3);
        WithKeyword(CardKeyword.Exhaust, UpgradeType.Remove);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        var chosen = await CommonActions.SelectSingleCard(this, CardSelectorPrefs.DiscardSelectionPrompt, ctx, PileType.Hand);
        if (chosen == null) return;
        var draw = chosen.EnergyCost.CostsX ? Owner.PlayerCombatState?.Energy ?? 0 : chosen.EnergyCost.GetResolved();
        await CardCmd.Discard(ctx, chosen);
        if (draw > 0) await CardPileCmd.Draw(ctx, draw, Owner);
    }
}
