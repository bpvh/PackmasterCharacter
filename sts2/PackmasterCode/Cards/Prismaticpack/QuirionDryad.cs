// Ported from src/main/java/thePackmaster/cards/prismaticpack/QuirionDryad.java
using Packmaster.PackmasterCode.Powers.Prismaticpack;

namespace Packmaster.PackmasterCode.Cards.Prismaticpack;

/// <summary>Power, 2 cost. The first time you play a card of each color, gain 1 Strength and 1 Dexterity. Upgrade: cost 1.</summary>
public sealed class QuirionDryad : AbstractPrismaticCard
{
    public QuirionDryad() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 1);
        WithCostUpgradeBy(-1);
        WithTip(typeof(QuirionDryadPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<QuirionDryadPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
