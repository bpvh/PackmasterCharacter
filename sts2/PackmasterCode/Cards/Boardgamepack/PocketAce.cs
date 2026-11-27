// Ported from src/main/java/thePackmaster/cards/boardgamepack/PocketAce.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

/// <summary>Draw 3 (+1) cards. Discard 2 (+1) cards. Exhaust.</summary>
public sealed class PocketAce : AbstractBoardCard
{
    public PocketAce() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVar("Magic", 3, 1);
        WithVar("Magic2", 2, 1);
        WithKeyword(CardKeyword.Exhaust);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
        await Wiz.ChooseAndDiscard(ctx, this, (int)DynamicVars["Magic2"].BaseValue);
    }
}
