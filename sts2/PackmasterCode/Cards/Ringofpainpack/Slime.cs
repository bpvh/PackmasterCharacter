// Ported from src/main/java/thePackmaster/cards/ringofpainpack/Slime.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>Apply 4 (+1 per upgrade) Poison. Evolves when you apply a debuff. Evolved: the enemy then loses HP equal to its Poison.</summary>
public sealed class Slime : AbstractEvolveCard
{
    public Slime() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithVar("Magic", 4, 1);
        WithVar(EvolvedVar, 0);
        WithTip(typeof(PoisonPower));
        WithTip(Util.PmTips.Evolve);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var target = play.Target;
        if (target == null) return;
        await CommonActions.Apply<PoisonPower>(ctx, target, this, DynamicVars["Magic"].BaseValue);
        if (IsEvolved && target.IsAlive)
        {
            var poison = target.GetPowerAmount<PoisonPower>();
            if (poison > 0) await CreatureCmd.Damage(ctx, target, poison, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
        }
    }

#if OLD_STS2_API
    public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
#else
    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
#endif
    {
        if (InHand && amount > 0 && power.Type == PowerType.Debuff && applier == Owner.Creature && power.Owner != Owner.Creature) Evolve();
        return Task.CompletedTask;
    }
}
