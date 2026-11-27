// Ported from src/main/java/thePackmaster/cards/gemspack/RetainGem.java
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>Amber. Unplayable gem: the card gains Retain.</summary>
public sealed class RetainGem : AbstractGemsCard
{
    public RetainGem() : base(CardRarity.Uncommon, playable: false)
    {
    }

    public override GemModifier CreateModifier(bool mutable) => BaseLib.Extensions.ModelDbExtensions.CardModifier<RetainGemModifier>(mutable);
}
