// Ported from src/main/java/thePackmaster/powers/energyandechopack/SlowingPower.java
namespace Packmaster.PackmasterCode.Powers.Energyandechopack;

/// <summary>At the start of your turn, gain [Amount] energy.</summary>
public sealed class SlowingPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        await PlayerCmd.GainEnergy(Amount, player);
    }
}
