// Ported from src/main/java/thePackmaster/cards/evenoddpack/WitzBolt.java
using MegaCrit.Sts2.Core.Models.Powers;

using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>Deal 8 (+3) damage. Odd: apply 1 Weak and Vulnerable.</summary>
public sealed class WitzBolt : AbstractEvenOddCard
{
    public WitzBolt() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(8, 3);
        WithVar("Magic", 1);
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
        WithTip(PmTips.Odd);
    }

    protected override bool ShouldGlowGoldInternal => IsOdd();

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var odd = IsOdd(play);
        await CommonActions.CardAttack(this, play).Execute(ctx);
        if (!odd || play.Target is not { IsAlive: true }) return;
        await CommonActions.Apply<VulnerablePower>(ctx, play.Target, this, DynamicVars["Magic"].BaseValue);
        await CommonActions.Apply<WeakPower>(ctx, play.Target, this, DynamicVars["Magic"].BaseValue);
    }
}
