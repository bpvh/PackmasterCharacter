// StS1 Lie applied -Strength plus vanilla GainStrengthPower.
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Cards.Quietpack;

namespace Packmaster.PackmasterCode.Powers.Quietpack;

/// <summary>Loses [Amount] Strength this turn.</summary>
public sealed class LiePower : CustomTemporaryPowerModelWrapper<Lie, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
