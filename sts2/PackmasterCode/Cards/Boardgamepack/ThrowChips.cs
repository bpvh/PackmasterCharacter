// Ported from src/main/java/thePackmaster/cards/boardgamepack/ThrowChips.java
using Packmaster.PackmasterCode.Powers.Boardgamepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

/// <summary>Deal 8 (+3) damage to a random enemy X times. Reroll (doesn't use up Dice).</summary>
public sealed class ThrowChips : AbstractBoardCard, IRerollCard
{
    public ThrowChips() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        WithDamage(8, 3);
        WithTip(PmTips.Reroll);
    }

    protected override bool HasEnergyCostX => true;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var x = EnergyCost.CapturedXValue;
        for (int i = 0; i < x; i++)
            await Wiz.AttackRandomEnemy(ctx, this, play, "vfx/vfx_attack_slash");
    }
}
