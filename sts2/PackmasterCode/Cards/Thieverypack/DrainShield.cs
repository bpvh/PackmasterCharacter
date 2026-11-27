// Ported from src/main/java/thePackmaster/cards/thieverypack/DrainShield.java
using Packmaster.PackmasterCode.Powers.Thieverypack;

namespace Packmaster.PackmasterCode.Cards.Thieverypack;

/// <summary>Steal all Block from the enemy. Deal 9 (+3) damage.</summary>
public sealed class DrainShield : AbstractThieveryCard
{
    public DrainShield() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(9, 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (play.Target != null) await Steal.AllBlock(Owner.Creature, play.Target);
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
    }
}
