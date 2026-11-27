// Ported from src/main/java/thePackmaster/powers/thieverypack/HammeredPower.java
using MegaCrit.Sts2.Core.Combat;

namespace Packmaster.PackmasterCode.Powers.Thieverypack;

/// <summary>Until the end of the round, buffs this enemy gains are stolen by the player who hammered it.</summary>
public sealed class HammeredPower : PackmasterPower
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    public Creature? Thief { get; set; }

#if OLD_STS2_API
    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        var ctx = new ThrowingPlayerChoiceContext();
#else
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext ctx, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
#endif
        if (Thief == null || power.Owner != Owner || power == this || power.Type != PowerType.Buff || amount <= 0) return;
        if (!Steal.CanSteal(power)) return;
        Flash();
        await Steal.Power(ctx, Thief, power, amount);
    }

    // StS1 atEndOfRound: after the enemies' turn.
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Enemy) await PowerCmd.Remove(this);
    }
}
