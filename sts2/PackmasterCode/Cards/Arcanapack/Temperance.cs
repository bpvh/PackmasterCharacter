// Ported from src/main/java/thePackmaster/cards/arcanapack/Temperance.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Arcanapack;

/// <summary>Draw a card for each card in your exhaust pile. Exhaust (not when upgraded).</summary>
public sealed class Temperance : AbstractAstrologerCard
{
    public Temperance() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, UpgradeType.Remove);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var count = Wiz.ExhaustPile(Owner).Count;
        if (count > 0) await CardPileCmd.Draw(ctx, count, Owner);
    }
}
