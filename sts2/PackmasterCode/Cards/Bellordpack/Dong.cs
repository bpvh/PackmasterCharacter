// Ported from src/main/java/thePackmaster/cards/bellordpack/Dong.java
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>2 cost. Deal 20 (+4) damage. Apply 2 (+1) Weak and Vulnerable. When obtained, add a Curse of the Bell to your deck.</summary>
public sealed class Dong : AbstractBellordCard
{
    public Dong() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(20, 4);
        WithVar("Magic", 2, 1);
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(CurseOfTheBell));
    }

    protected override int CursesOnObtain => 1;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
        if (play.Target is not { IsAlive: true }) return;
        await CommonActions.Apply<WeakPower>(ctx, play.Target, this, DynamicVars["Magic"].BaseValue);
        await CommonActions.Apply<VulnerablePower>(ctx, play.Target, this, DynamicVars["Magic"].BaseValue);
    }
}
