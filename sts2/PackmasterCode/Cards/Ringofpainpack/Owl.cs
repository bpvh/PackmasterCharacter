// Ported from src/main/java/thePackmaster/cards/ringofpainpack/Owl.java (+ actions/ringofpainpack/OwlAction.java)
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Powers.Ringofpainpack;

namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>3 cost. Deal 24 (+4 per upgrade) damage. Apply 2 Vulnerable. Evolves when you Exhaust a card. Evolved: also apply Rend equal to half the unblocked damage.</summary>
public sealed class Owl : AbstractEvolveCard
{
    public Owl() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(24, 4);
        WithVar("Magic", 2);
        WithVar(EvolvedVar, 0);
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(RendPower));
        WithTip(Util.PmTips.Evolve);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var target = play.Target;
        if (target == null) return;
        var hpBefore = target.CurrentHp;
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        if (IsEvolved && target.IsAlive)
        {
            var rend = (hpBefore - target.CurrentHp) / 2;
            if (rend > 0) await CommonActions.Apply<RendPower>(ctx, target, this, rend);
        }
        if (target.IsAlive) await CommonActions.Apply<VulnerablePower>(ctx, target, this, DynamicVars["Magic"].BaseValue);
    }

    public override Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (InHand && card != this && card.Owner == Owner) Evolve();
        return Task.CompletedTask;
    }
}
