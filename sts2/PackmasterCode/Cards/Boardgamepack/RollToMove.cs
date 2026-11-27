// Ported from src/main/java/thePackmaster/cards/boardgamepack/RollToMove.java
using Packmaster.PackmasterCode.Powers.Boardgamepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

/// <summary>Gain 10 (+4) Block. Roll 2 D6.</summary>
public sealed class RollToMove : AbstractBoardCard
{
    public const int Sides = 6;

    public RollToMove() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(10, 4);
        WithTip(PmTips.Dice);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await Dice.Roll(ctx, Owner, Sides, 2);
    }
}
