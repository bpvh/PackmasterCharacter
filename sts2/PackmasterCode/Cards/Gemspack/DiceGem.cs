// Ported from src/main/java/thePackmaster/cards/gemspack/DiceGem.java
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>Tourmaline. Gem: roll a D6.</summary>
public sealed class DiceGem : AbstractGemsCard
{
    public DiceGem() : base(CardRarity.Common)
    {
        WithTip(PmTips.Dice);
    }

    public override GemModifier CreateModifier(bool mutable) => BaseLib.Extensions.ModelDbExtensions.CardModifier<DiceGemModifier>(mutable);
}
