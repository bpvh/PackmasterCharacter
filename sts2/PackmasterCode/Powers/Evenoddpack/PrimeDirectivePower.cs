// Ported from src/main/java/thePackmaster/powers/evenoddpack/PrimeDirectivePower.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Evenoddpack;

/// <summary>Your Attacks have "Odd: deal [Amount] more damage" (odd = 1st, 3rd, ... card played this turn).</summary>
public sealed class PrimeDirectivePower : PackmasterPower
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
        if (dealer != Owner || cardSource is not { Type: CardType.Attack } || !IsOwnersCard(cardSource)) return 0m;
        return Wiz.PlayPosition(Owner.Player!, null) % 2 == 1 ? Amount : 0m;
    }
}
