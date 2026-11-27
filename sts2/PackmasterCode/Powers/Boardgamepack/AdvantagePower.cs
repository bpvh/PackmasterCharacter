// Ported from src/main/java/thePackmaster/powers/boardgamepack/AdvantagePower.java
namespace Packmaster.PackmasterCode.Powers.Boardgamepack;

/// <summary>Roll Dice [Amount] additional time(s) and take the highest value. Read by <see cref="Dice.Roll"/>.</summary>
public sealed class AdvantagePower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
