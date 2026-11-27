// Ported from src/main/java/thePackmaster/powers/aggressionpack/InnerFuryPower.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Cards.Aggressionpack;
using Packmaster.PackmasterCode.Stances;
using Packmaster.PackmasterCode.Stances.Aggressionpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Aggressionpack;

/// <summary>When you enter or exit Aggression, gain [Amount] temporary Strength and [Amount] Block.</summary>
public sealed class InnerFuryPower : PackmasterPower, IStanceListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task AfterStanceChanged(PlayerChoiceContext ctx, Type? oldStance, Type? newStance)
    {
        if (oldStance != typeof(Aggression) && newStance != typeof(Aggression)) return;
        Flash();
        await Wiz.ApplyPower<InnerFuryStrengthPower>(ctx, Owner, Amount, Owner);
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}

/// <summary>StS1 StrengthPower + LoseStrengthPower: Strength until end of turn.</summary>
public sealed class InnerFuryStrengthPower : CustomTemporaryPowerModelWrapper<InnerFury, StrengthPower>;
