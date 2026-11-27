// Ported from src/main/java/thePackmaster/cards/gemspack/LightningGem.java
using MegaCrit.Sts2.Core.Models.Orbs;
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>Fragmented Gem. Gem: channel 1 Lightning.</summary>
public sealed class LightningGem : AbstractGemsCard
{
    public LightningGem() : base(CardRarity.Uncommon)
    {
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(LightningOrb));
    }

    public override GemModifier CreateModifier(bool mutable) => BaseLib.Extensions.ModelDbExtensions.CardModifier<LightningGemModifier>(mutable);
}
