// Ported from src/main/java/thePackmaster/cards/boardgamepack/PerceptionCheck.java
using Packmaster.PackmasterCode.Powers.Boardgamepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

/// <summary>At the start of your turn, if you have no Dice, roll a D8. Upgrade: costs 0.</summary>
public sealed class PerceptionCheck : AbstractBoardCard
{
    public PerceptionCheck() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithCostUpgradeBy(-1);
        WithTip(PmTips.Dice);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<PerceptionCheckPower>(ctx, this, 1);
    }
}
