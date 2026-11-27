// StS1 Quincunx applied -Strength plus vanilla GainStrengthPower.
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Cards.Alignmentpack;

namespace Packmaster.PackmasterCode.Powers.Alignmentpack;

/// <summary>Loses [Amount] Strength this turn.</summary>
public sealed class QuincunxPower : CustomTemporaryPowerModelWrapper<Quincunx, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
