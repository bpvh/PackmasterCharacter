// Ported from src/main/java/thePackmaster/cards/coresetpack/BackpackSmack.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Coresetpack;

/// <summary>X cost. Deal 8 (+3) damage X times. Synergy: first apply X Vulnerable and X Weak.</summary>
public sealed class BackpackSmack : PackmasterCard
{
    public BackpackSmack() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(8, 3);
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(WeakPower));
    }

    protected override bool HasEnergyCostX => true;

    protected override bool ShouldGlowGoldInternal => Wiz.HasSynergy(this);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var x = EnergyCost.CapturedXValue;
        if (x <= 0 || play.Target == null) return;
        if (Wiz.HasSynergy(this))
        {
            await CommonActions.Apply<VulnerablePower>(ctx, play.Target, this, x);
            await CommonActions.Apply<WeakPower>(ctx, play.Target, this, x);
        }
        await CommonActions.CardAttack(this, play, hitCount: x, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
    }
}
