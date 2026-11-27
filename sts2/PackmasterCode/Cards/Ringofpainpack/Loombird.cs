// Ported from src/main/java/thePackmaster/cards/ringofpainpack/Loombird.java
using Packmaster.PackmasterCode.Powers.Ringofpainpack;

namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>Deal 8 (+1) damage. Apply 3 (+1) Rend.</summary>
public sealed class Loombird : AbstractRingOfPainCard
{
    public Loombird() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(8, 1);
        WithVar("Magic", 3, 1);
        WithTip(typeof(RendPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        if (play.Target is { IsAlive: true }) await CommonActions.Apply<RendPower>(ctx, play.Target, this, DynamicVars["Magic"].BaseValue);
    }
}
