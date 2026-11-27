// Ported from src/main/java/thePackmaster/powers/rimworldpack/SpikeTrapPower.java
namespace Packmaster.PackmasterCode.Powers.Rimworldpack;

/// <summary>The next time an enemy attacks you, deal [Amount] damage to it. Then this is removed.</summary>
public sealed class SpikeTrapPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || dealer == null || dealer == Owner || !props.HasFlag(ValueProp.Move)) return;
        Flash();
        var amount = Amount;
        await PowerCmd.Remove(this);
        if (dealer.IsAlive) await CreatureCmd.Damage(choiceContext, dealer, amount, ValueProp.Unpowered, Owner);
    }
}
