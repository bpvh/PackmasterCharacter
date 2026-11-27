// Ported from src/main/java/thePackmaster/cards/thieverypack/FairTrade.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Thieverypack;

/// <summary>Deal 6 (+2) damage twice. You and the enemy gain 1 Strength each.</summary>
public sealed class FairTrade : AbstractThieveryCard
{
    public FairTrade() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(6, 2);
        WithVar("Magic", 1);
        WithTip(typeof(StrengthPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, hitCount: 2, vfx: "vfx/vfx_attack_blunt").Execute(ctx);
        await CommonActions.ApplySelf<StrengthPower>(ctx, this, DynamicVars["Magic"].BaseValue);
        if (play.Target is { IsAlive: true })
            await CommonActions.Apply<StrengthPower>(ctx, play.Target, this, DynamicVars["Magic"].BaseValue);
    }
}
