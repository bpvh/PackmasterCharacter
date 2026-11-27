// Ported from src/main/java/thePackmaster/powers/upgradespack/BlacksmithsShieldPower.java
namespace Packmaster.PackmasterCode.Powers.Upgradespack;

/// <summary>Whenever you play an upgraded card, gain [Amount] Block for each time it was upgraded.</summary>
public sealed class BlacksmithsShieldPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!IsOwnersCard(card) || card.CurrentUpgradeLevel <= 0) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, card.CurrentUpgradeLevel * Amount, ValueProp.Unpowered, null);
    }
}
