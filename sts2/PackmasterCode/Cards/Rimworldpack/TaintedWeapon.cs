// Ported from src/main/java/thePackmaster/cards/rimworldpack/TaintedWeapon.java (+ actions/rimworldpack/PutCardInMonsterAction.java)
using Packmaster.PackmasterCode.Powers.Rimworldpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>0 cost. Deal 8 (+3) damage. If the enemy survives, this card stays in it and returns to your hand when it dies.</summary>
public sealed class TaintedWeapon : AbstractRimCard
{
    public TaintedWeapon() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(8, 3);
        WithTip(typeof(HoldCardPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var target = play.Target;
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        if (target is not { IsAlive: true }) return;
        var power = await Wiz.ApplyPower<HoldCardPower>(ctx, target, 1, Owner.Creature, this);
        power?.Hold(this);
        // VERIFY: removing the card mid-play keeps it out of the discard pile.
        await CardPileCmd.RemoveFromCombat(this);
    }
}
