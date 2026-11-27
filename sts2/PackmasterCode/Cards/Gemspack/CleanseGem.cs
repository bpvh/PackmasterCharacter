// Ported from src/main/java/thePackmaster/cards/gemspack/CleanseGem.java
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>Onyx. Gem: reduce your debuffs by 1.</summary>
public sealed class CleanseGem : AbstractGemsCard
{
    public CleanseGem() : base(CardRarity.Uncommon)
    {
    }

    public override GemModifier CreateModifier(bool mutable) => BaseLib.Extensions.ModelDbExtensions.CardModifier<CleanseGemModifier>(mutable);
}
