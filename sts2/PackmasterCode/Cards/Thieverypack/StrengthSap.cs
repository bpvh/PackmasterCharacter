// Ported from src/main/java/thePackmaster/cards/thieverypack/StrengthSap.java
using BaseLib.Cards.Variables;
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Powers.Thieverypack;

namespace Packmaster.PackmasterCode.Cards.Thieverypack;

/// <summary>Apply 1 (+1) Weak. If the enemy has positive Strength, steal 1 Strength. Exhaustive 2 (+1).</summary>
public sealed class StrengthSap : AbstractThieveryCard
{
    public StrengthSap() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithVar("Magic", 1, 1);
        WithVar("Magic2", 1);
        WithVar(new ExhaustiveVar(2));
        WithTip(typeof(WeakPower));
        WithTip(typeof(StrengthPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars[ExhaustiveVar.Key].UpgradeValueBy(1);
    }

    protected override bool ShouldGlowGoldInternal =>
        CombatState?.HittableEnemies.Any(e => e.GetPower<StrengthPower>() is { Amount: > 0 }) == true;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (play.Target == null) return;
        await CommonActions.Apply<WeakPower>(ctx, play.Target, this, DynamicVars["Magic"].BaseValue);
        if (play.Target.GetPower<StrengthPower>() is { Amount: > 0 } str)
            await Steal.Power(ctx, Owner.Creature, str, DynamicVars["Magic2"].BaseValue);
    }
}
