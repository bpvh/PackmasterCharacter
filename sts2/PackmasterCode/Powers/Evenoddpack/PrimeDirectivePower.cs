// Ported from src/main/java/thePackmaster/powers/evenoddpack/PrimeDirectivePower.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Evenoddpack;

/// <summary>Your Attacks have "Odd: deal [Amount] more damage" (odd = 1st, 3rd, ... card played this turn).</summary>
public sealed class PrimeDirectivePower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // VERIFY signature (see StrikeDummyJrPower). The card being played hasn't finished, so PlayPosition(null)
    // is its position; for cards in hand it's the position they would have.
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner || cardSource is not { Type: CardType.Attack } || !IsOwnersCard(cardSource)) return amount;
        return Wiz.PlayPosition(Owner.Player!, null) % 2 == 1 ? amount + Amount : amount;
    }
}
