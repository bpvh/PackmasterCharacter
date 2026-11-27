// Ported from src/main/java/thePackmaster/powers/ringofpainpack/ConnectionPower.java
namespace Packmaster.PackmasterCode.Powers.Ringofpainpack;

/// <summary>Whenever you play a card, Upgrade it and every copy of it in your piles.</summary>
public sealed class ConnectionPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!IsOwnersCard(card) || card.Owner.PlayerCombatState == null) return Task.CompletedTask;
        var copies = card.Owner.PlayerCombatState.AllCards.Where(c => c.Id == card.Id && c.IsUpgradable).ToList();
        if (card.IsUpgradable && !copies.Contains(card)) copies.Add(card);
        if (copies.Count == 0) return Task.CompletedTask;
        Flash();
        foreach (var c in copies) CardCmd.Upgrade(c);
        return Task.CompletedTask;
    }
}
