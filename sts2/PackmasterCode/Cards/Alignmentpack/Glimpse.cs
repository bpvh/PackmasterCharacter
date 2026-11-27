// Ported from src/main/java/thePackmaster/cards/alignmentpack/Glimpse.java
using Packmaster.PackmasterCode.Powers.Alignmentpack;

namespace Packmaster.PackmasterCode.Cards.Alignmentpack;

/// <summary>Power. Whenever you draw an Ethereal card, gain 3 (+2) Block.</summary>
public sealed class Glimpse : global::Packmaster.PackmasterCode.Cards.Arcanapack.AbstractAstrologerCard
{
    public Glimpse() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 3, 2);
        WithTip(typeof(GlimpsePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<GlimpsePower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
