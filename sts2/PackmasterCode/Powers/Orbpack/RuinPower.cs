// Ported from src/main/java/thePackmaster/powers/orbpack/RuinPower.java
namespace Packmaster.PackmasterCode.Powers.Orbpack;

/// <summary>The next time the owner takes unblocked attack damage, it loses [Amount] HP for each buff and debuff it has.</summary>
public sealed class RuinPower : PackmasterPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || result.UnblockedDamage <= 0 || !props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered)) return;
        var loss = Amount * Owner.Powers.Count;
        Flash();
        await PowerCmd.Remove(this);
        if (Owner.IsAlive && loss > 0)
            await CreatureCmd.Damage(choiceContext, Owner, loss, ValueProp.Unblockable | ValueProp.Unpowered, dealer ?? Owner);
    }
}
