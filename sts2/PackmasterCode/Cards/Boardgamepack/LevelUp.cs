// Ported from src/main/java/thePackmaster/cards/boardgamepack/LevelUp.java
using Packmaster.PackmasterCode.Powers.Boardgamepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

/// <summary>Gain 1 (+1) Advantage. Roll 2 D6.</summary>
public sealed class LevelUp : AbstractBoardCard
{
    public const int Sides = 6;
    public const int NumDice = 2;

    public LevelUp() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 1, 1);
        WithTip(typeof(AdvantagePower));
        WithTip(PmTips.Dice);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<AdvantagePower>(ctx, this, DynamicVars["Magic"].BaseValue);
        await Dice.Roll(ctx, Owner, Sides, NumDice);
    }
}
