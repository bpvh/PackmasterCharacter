// Ported from src/main/java/thePackmaster/cards/gemspack/EnergyGem.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>Citrine. Gem: gain 1 energy next turn.</summary>
public sealed class EnergyGem : AbstractGemsCard
{
    public EnergyGem() : base(CardRarity.Rare)
    {
        WithTip(typeof(EnergyNextTurnPower));
    }

    public override GemModifier CreateModifier(bool mutable) => BaseLib.Extensions.ModelDbExtensions.CardModifier<EnergyGemModifier>(mutable);
}
