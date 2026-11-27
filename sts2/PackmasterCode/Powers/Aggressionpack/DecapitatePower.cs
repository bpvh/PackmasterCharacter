// StS1 Decapitate applied -Strength plus vanilla GainStrengthPower.
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Cards.Aggressionpack;

namespace Packmaster.PackmasterCode.Powers.Aggressionpack;

/// <summary>Loses [Amount] Strength this turn.</summary>
public sealed class DecapitatePower : CustomTemporaryPowerModelWrapper<Decapitate, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
