// Ported from src/main/java/thePackmaster/cards/gemspack/DrawGem.java
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>Opal. Gem: draw 1 card.</summary>
public sealed class DrawGem : AbstractGemsCard
{
    public DrawGem() : base(CardRarity.Rare)
    {
    }

    public override GemModifier CreateModifier(bool mutable) => BaseLib.Extensions.ModelDbExtensions.CardModifier<DrawGemModifier>(mutable);
}
