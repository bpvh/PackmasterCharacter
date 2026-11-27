// Ported from src/main/java/thePackmaster/cards/rimworldpack/TradeBeacon.java
using Packmaster.PackmasterCode.Powers.Rimworldpack;

namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>Power, 1 (0) cost. Once per turn, the first time you draw or discard a card after your turn's draw, draw 2 cards.</summary>
public sealed class TradeBeacon : AbstractRimCard
{
    public TradeBeacon() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 2);
        WithCostUpgradeBy(-1);
        WithTip(typeof(TradeBeaconPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<TradeBeaconPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
