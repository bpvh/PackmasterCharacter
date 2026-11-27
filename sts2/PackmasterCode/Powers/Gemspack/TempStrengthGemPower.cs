// StS1 Ruby applied StrengthPower plus LoseStrengthPower.
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Cards.Gemspack;

namespace Packmaster.PackmasterCode.Powers.Gemspack;

/// <summary>Gains [Amount] Strength this turn (Ruby).</summary>
public sealed class TempStrengthGemPower : CustomTemporaryPowerModelWrapper<TempStrengthGem, StrengthPower>;
