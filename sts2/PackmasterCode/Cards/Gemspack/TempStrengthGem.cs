// Ported from src/main/java/thePackmaster/cards/gemspack/TempStrengthGem.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>Ruby. Gem: gain 2 temporary Strength.</summary>
public sealed class TempStrengthGem : AbstractGemsCard
{
    public TempStrengthGem() : base(CardRarity.Common)
    {
        WithTip(typeof(StrengthPower));
    }

    public override GemModifier CreateModifier(bool mutable) => BaseLib.Extensions.ModelDbExtensions.CardModifier<TempStrengthGemModifier>(mutable);
}
