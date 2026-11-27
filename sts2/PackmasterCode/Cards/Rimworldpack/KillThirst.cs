// Ported from src/main/java/thePackmaster/cards/rimworldpack/KillThirst.java (+ actions/rimworldpack/KillThirstAction.java)
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>Deal 8 (+2) damage. If it doesn't kill, shuffle a Despair into your draw pile and double this card's damage this combat.</summary>
public sealed class KillThirst : AbstractRimCard
{
    public KillThirst() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(8, 2);
        WithTip(typeof(Despair));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var target = play.Target;
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
        if (target == null || !target.IsAlive) return;
        await Wiz.ShuffleIn(Wiz.CreateCard<Despair>(Owner), Owner);
        DynamicVars.Damage.BaseValue *= 2;
    }
}
