// Ported from src/main/java/thePackmaster/powers/boardgamepack/OneTimeAdvantagePower.java
namespace Packmaster.PackmasterCode.Powers.Boardgamepack;

/// <summary>Roll the next Dice [Amount] additional time(s) and take the highest value. Used up by <see cref="Dice.Roll"/>.</summary>
public sealed class OneTimeAdvantagePower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
