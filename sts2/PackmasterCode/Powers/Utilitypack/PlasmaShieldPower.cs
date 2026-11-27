// Ported from src/main/java/thePackmaster/powers/utilitypack/PlasmaShieldPower.java
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace Packmaster.PackmasterCode.Powers.Utilitypack;

/// <summary>At the end of your turn, gain [Amount] Block if you have Plasma.</summary>
public sealed class PlasmaShieldPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || Owner.Player?.PlayerCombatState is not { } state) return;
        if (!state.OrbQueue.Orbs.Any(o => o is PlasmaOrb)) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
