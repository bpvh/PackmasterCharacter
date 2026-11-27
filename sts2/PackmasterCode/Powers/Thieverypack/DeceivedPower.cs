// Ported from src/main/java/thePackmaster/powers/thieverypack/DeceivedPower.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Powers.Thieverypack;

/// <summary>If this creature dies while Poisoned, the thief gains [Amount] Gold.</summary>
public sealed class DeceivedPower : PackmasterPower
{
    public override PowerType Type => PowerType.None; // StS1 NEUTRAL
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>The player who gets the Gold (set by Cunning Poison).</summary>
    public Player? Thief { get; set; }

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Owner || Thief == null || !Owner.HasPower<PoisonPower>()) return;
        Flash();
        await PlayerCmd.GainGold(Amount * Steal.Multiplier(Thief.Creature), Thief);
    }
}
