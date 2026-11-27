// Ported from src/main/java/thePackmaster/powers/strikepack/StrikeDummyJrPower.java
namespace Packmaster.PackmasterCode.Powers.Strikepack;

/// <summary>Your Strikes deal [Amount] more damage this combat.</summary>
public sealed class StrikeDummyJrPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // ModifyDamageAdditive returns the delta (the game adds it). Game 0.111 (HeartMod) adds a CardPlay?
    // parameter that the older reference DLL lacks.
#if OLD_STS2_API
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
#else
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
#endif
    {
        if (dealer == Owner && cardSource != null && cardSource.Tags.Contains(CardTag.Strike))
            return Amount;
        return 0m;
    }
}
