// Ported from src/main/java/thePackmaster/powers/boardgamepack/SkipNextTurnPower.java
namespace Packmaster.PackmasterCode.Powers.Boardgamepack;

/// <summary>At the start of your next [Amount] turn(s), end your turn.</summary>
public sealed class SkipNextTurnPower : PackmasterPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // StS1 pressed the end-turn button at the start of turn. VERIFY: ending the turn from AfterPlayerTurnStart.
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        if (Amount <= 1) await PowerCmd.Remove(this);
        else await Util.Wiz.ModifyPowerAmount(this, -1);
        PlayerCmd.EndTurn(player, canBackOut: false);
    }
}
