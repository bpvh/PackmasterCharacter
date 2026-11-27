// Ported from src/main/java/thePackmaster/powers/coresetpack/MayhemFormPower.java
using MegaCrit.Sts2.Core.Combat;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Coresetpack;

/// <summary>
/// Once per turn, when you have 2 or fewer cards in hand, draw 2 cards and gain [Amount] energy.
/// StS1 checked on every hand refresh (OnRefreshHandPower) while the action queue was empty. StS2 has no hand
/// refresh hook, so this checks at the points where the hand shrinks during your turn: after the turn's draw,
/// after a card is played, discarded or exhausted.
/// </summary>
public sealed class MayhemFormPower : PackmasterPower
{
    private const int HandThreshold = 2;
    private const int CardsDrawn = 2;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    private bool _triggeredThisTurn;
    private bool _ownersTurn;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        _triggeredThisTurn = false;
        _ownersTurn = true;
        await TryTrigger(choiceContext);
    }

    public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side) _ownersTurn = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsOwnersCard(cardPlay.Card)) await TryTrigger(choiceContext);
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (IsOwnersCard(card)) await TryTrigger(choiceContext);
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (IsOwnersCard(card)) await TryTrigger(choiceContext);
    }

    private async Task TryTrigger(PlayerChoiceContext ctx)
    {
        var player = Owner.Player;
        if (player == null || _triggeredThisTurn || !_ownersTurn) return;
        if (Wiz.Hand(player).Count > HandThreshold) return;
        _triggeredThisTurn = true;
        Flash();
        await CardPileCmd.Draw(ctx, CardsDrawn, player);
        await PlayerCmd.GainEnergy(Amount, player);
    }
}
