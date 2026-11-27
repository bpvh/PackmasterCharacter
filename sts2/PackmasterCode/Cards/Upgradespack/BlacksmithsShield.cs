// Ported from src/main/java/thePackmaster/cards/upgradespack/BlacksmithsShield.java
using Packmaster.PackmasterCode.Powers.Upgradespack;

namespace Packmaster.PackmasterCode.Cards.Upgradespack;

/// <summary>Power, 1 (0) cost. Whenever you play an upgraded card, gain 1 Block for each time it was upgraded.</summary>
public sealed class BlacksmithsShield : AbstractBlacksmithCard
{
    public BlacksmithsShield() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithCostUpgradeBy(-1);
        WithTip(typeof(BlacksmithsShieldPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<BlacksmithsShieldPower>(ctx, this, 1);
    }
}
