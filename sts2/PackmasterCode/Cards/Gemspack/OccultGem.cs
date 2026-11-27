// Ported from src/main/java/thePackmaster/cards/gemspack/OccultGem.java
using Packmaster.PackmasterCode.Mechanics;
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>Lapis Lazuli. Unplayable gem: the card gains Occult. The gem itself is Occult (StS1 too).</summary>
public sealed class OccultGem : AbstractGemsCard, IOccultCard
{
    public OccultGem() : base(CardRarity.Rare, playable: false)
    {
        WithTip(PmTips.Occult);
    }

    public override GemModifier CreateModifier(bool mutable) => BaseLib.Extensions.ModelDbExtensions.CardModifier<OccultGemModifier>(mutable);
}
