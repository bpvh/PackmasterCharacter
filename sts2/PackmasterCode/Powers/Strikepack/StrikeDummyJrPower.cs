// Ported from src/main/java/thePackmaster/powers/strikepack/StrikeDummyJrPower.java
namespace Packmaster.PackmasterCode.Powers.Strikepack;

/// <summary>Your Strikes deal [Amount] more damage this combat.</summary>
public sealed class StrikeDummyJrPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // VERIFY signature on the current game build: newer builds add a trailing CardPlay? parameter to the
    // ModifyDamage* hooks (HeartMod, game 0.111). The older reference DLL has the 5-parameter form used here.
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner && cardSource != null && cardSource.Tags.Contains(CardTag.Strike))
            return amount + Amount;
        return amount;
    }
}
