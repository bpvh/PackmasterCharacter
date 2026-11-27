// Ported from src/main/java/thePackmaster/powers/thieverypack/ThieveryMasteryPower.java
namespace Packmaster.PackmasterCode.Powers.Thieverypack;

/// <summary>Whenever you steal something, gain twice as much of it. Read by <see cref="Steal"/>.</summary>
public sealed class ThieveryMasteryPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public void Notify() => Flash();
}
