// Ported from src/main/java/thePackmaster/cards/arcanapack/TheChariot.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Powers.Arcanapack;

namespace Packmaster.PackmasterCode.Cards.Arcanapack;

/// <summary>Power. Whenever you discard a card, gain 4 (+2) Vigor.</summary>
public sealed class TheChariot : AbstractAstrologerCard
{
    public TheChariot() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 4, 2);
        WithTip(typeof(TheChariotPower));
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<TheChariotPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
