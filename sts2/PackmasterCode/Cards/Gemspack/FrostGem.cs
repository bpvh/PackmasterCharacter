// Ported from src/main/java/thePackmaster/cards/gemspack/FrostGem.java
using MegaCrit.Sts2.Core.Models.Orbs;
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>Aquamarine. Gem: channel 1 Frost.</summary>
public sealed class FrostGem : AbstractGemsCard
{
    public FrostGem() : base(CardRarity.Uncommon)
    {
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(FrostOrb));
    }

    public override GemModifier CreateModifier(bool mutable) => BaseLib.Extensions.ModelDbExtensions.CardModifier<FrostGemModifier>(mutable);
}
