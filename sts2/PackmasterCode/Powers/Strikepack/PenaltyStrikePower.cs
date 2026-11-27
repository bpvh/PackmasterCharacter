// StS1 PenaltyStrike applied -Strength plus vanilla GainStrengthPower (Piercing Wail style).
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Cards.Strikepack;

namespace Packmaster.PackmasterCode.Powers.Strikepack;

/// <summary>The target loses [Amount] Strength this turn (BaseLib temporary power around StrengthPower).</summary>
public sealed class PenaltyStrikePower : CustomTemporaryPowerModelWrapper<PenaltyStrike, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
