// Ported from src/main/java/thePackmaster/cards/prismaticpack/ExoticKnowledge.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Prismaticpack;

/// <summary>Draw 2 cards. If you played a card of another color this turn, draw 2 (+1) more.</summary>
public sealed class ExoticKnowledge : AbstractPrismaticCard
{
    public ExoticKnowledge() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 2);
        WithVar("Magic2", 2, 1);
    }

    protected override bool ShouldGlowGoldInternal => !IsCanonical && Wiz.PlayedOtherColorThisTurn(Owner);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var draw = DynamicVars["Magic"].BaseValue;
        if (Wiz.PlayedOtherColorThisTurn(Owner)) draw += DynamicVars["Magic2"].BaseValue;
        await CardPileCmd.Draw(ctx, draw, Owner);
    }
}
