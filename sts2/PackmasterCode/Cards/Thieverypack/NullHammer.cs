// Ported from src/main/java/thePackmaster/cards/thieverypack/NullHammer.java
using Packmaster.PackmasterCode.Powers.Thieverypack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Thieverypack;

/// <summary>Deal 13 (+4) damage. Steal all buffs this enemy gains until your next turn.</summary>
public sealed class NullHammer : AbstractThieveryCard
{
    public NullHammer() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(13, 4);
        WithTip(typeof(HammeredPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
        if (play.Target is not { IsAlive: true } target) return;
        var hammered = await Wiz.ApplyPower<HammeredPower>(ctx, target, 1, Owner.Creature, this);
        if (hammered != null) hammered.Thief = Owner.Creature;
    }
}
