// Ported from src/main/java/thePackmaster/cards/gemspack/PersistGem.java
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>Garnet. Unplayable gem: the card gains Persist 2.</summary>
public sealed class PersistGem : AbstractGemsCard
{
    public PersistGem() : base(CardRarity.Rare, playable: false)
    {
        WithTip(BaseLibTip.Persist);
    }

    public override GemModifier CreateModifier(bool mutable) => BaseLib.Extensions.ModelDbExtensions.CardModifier<PersistGemModifier>(mutable);
}
