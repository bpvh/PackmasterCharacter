// Ported from src/main/java/thePackmaster/cards/ringofpainpack/Hound.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>0 cost. Deal 4 (+1 per upgrade) damage to ALL enemies. Evolves when you play an Attack. Evolved: also apply 1 Weak to ALL enemies.</summary>
public sealed class Hound : AbstractEvolveCard
{
    public Hound() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        WithDamage(4, 1);
        WithVar("Magic", 1);
        WithVar(EvolvedVar, 0);
        WithTip(typeof(WeakPower));
        WithTip(Util.PmTips.Evolve);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play).Execute(ctx);
        if (!IsEvolved || CombatState == null) return;
        foreach (var enemy in CombatState.HittableEnemies.ToList())
            await CommonActions.Apply<WeakPower>(ctx, enemy, this, DynamicVars["Magic"].BaseValue);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (InHand && cardPlay.Card != this && cardPlay.Card.Owner == Owner && cardPlay.Card.Type == CardType.Attack) Evolve();
        return Task.CompletedTask;
    }
}
