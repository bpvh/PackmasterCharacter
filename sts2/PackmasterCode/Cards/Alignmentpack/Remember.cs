// Ported from src/main/java/thePackmaster/cards/alignmentpack/Remember.java
using Packmaster.PackmasterCode.Powers.Alignmentpack;

namespace Packmaster.PackmasterCode.Cards.Alignmentpack;

/// <summary>Power. At the end of your turn, shuffle up to 1 card from your hand into your draw pile. Upgrade: cost 0.</summary>
public sealed class Remember : global::Packmaster.PackmasterCode.Cards.Arcanapack.AbstractAstrologerCard
{
    public Remember() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 1);
        WithCostUpgradeBy(-1);
        WithTip(typeof(RememberPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<RememberPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
