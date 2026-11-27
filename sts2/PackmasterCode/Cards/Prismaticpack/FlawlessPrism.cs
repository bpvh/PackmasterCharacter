// Ported from src/main/java/thePackmaster/cards/prismaticpack/FlawlessPrism.java
using Packmaster.PackmasterCode.Powers.Prismaticpack;

namespace Packmaster.PackmasterCode.Cards.Prismaticpack;

/// <summary>Power, 2 cost. At the start of your turn, add 1 (+1) random Power of another color to your hand.</summary>
public sealed class FlawlessPrism : AbstractPrismaticCard
{
    public FlawlessPrism() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 1, 1);
        WithTip(typeof(FlawlessPrismPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<FlawlessPrismPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
