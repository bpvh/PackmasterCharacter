// Ported from src/main/java/thePackmaster/cards/alignmentpack/Alignment.java
namespace Packmaster.PackmasterCode.Cards.Alignmentpack;

/// <summary>X cost. Draw X cards. They cost X less this turn. Exhaust (not when upgraded).</summary>
public sealed class Alignment : global::Packmaster.PackmasterCode.Cards.Arcanapack.AbstractAstrologerCard
{
    public Alignment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, UpgradeType.Remove);
    }

    protected override bool HasEnergyCostX => true;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var x = EnergyCost.CapturedXValue;
        if (x <= 0) return;
        foreach (var card in await CardPileCmd.Draw(ctx, x, Owner))
        {
            if (card.EnergyCost.CostsX) continue;
            var cost = card.EnergyCost.GetResolved();
            if (cost > 0) card.EnergyCost.SetThisTurn(Math.Max(0, cost - x), reduceOnly: true);
        }
    }
}
