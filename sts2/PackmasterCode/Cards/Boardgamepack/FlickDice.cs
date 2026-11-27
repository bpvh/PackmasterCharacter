// Ported from src/main/java/thePackmaster/cards/boardgamepack/FlickDice.java
using Packmaster.PackmasterCode.Powers.Boardgamepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

/// <summary>Deal 8 (+3) damage. Roll a D6.</summary>
public sealed class FlickDice : AbstractBoardCard
{
    public const int Sides = 6;

    public FlickDice() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(8, 3);
        WithTip(PmTips.Dice);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_blunt").Execute(ctx);
        await Dice.Roll(ctx, Owner, Sides);
    }
}
