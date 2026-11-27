// Ported from src/main/java/thePackmaster/powers/rimworldpack/TradeBeaconPower.java
using MegaCrit.Sts2.Core.Combat;

namespace Packmaster.PackmasterCode.Powers.Rimworldpack;

/// <summary>Once per turn, the first time you draw or discard a card after your turn's draw, draw [Amount] cards.</summary>
public sealed class TradeBeaconPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool _armed;   // after the start-of-turn draw
    private bool _traded;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player) { _armed = true; _traded = false; }
        return Task.CompletedTask;
    }

    public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side) _armed = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (IsOwnersCard(card) && !fromHandDraw) await Trade(choiceContext);
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (IsOwnersCard(card)) await Trade(choiceContext);
    }

    private async Task Trade(PlayerChoiceContext ctx)
    {
        if (!_armed || _traded || Owner.Player == null) return;
        _traded = true;
        Flash();
        await CardPileCmd.Draw(ctx, Amount, Owner.Player);
    }
}
