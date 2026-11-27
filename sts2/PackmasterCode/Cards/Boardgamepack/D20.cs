// Ported from src/main/java/thePackmaster/cards/boardgamepack/D20.java
using Packmaster.PackmasterCode.Powers.Boardgamepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

/// <summary>Roll a D20. Exhaust (upgrade removes Exhaust).</summary>
public sealed class D20 : AbstractBoardCard
{
    public const int Sides = 20;

    public D20() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithKeyword(CardKeyword.Exhaust, UpgradeType.Remove);
        WithTip(PmTips.Dice);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Dice.Roll(ctx, Owner, Sides);
    }
}
