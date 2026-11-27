// Ported from src/main/java/thePackmaster/powers/boardgamepack/PerceptionCheckPower.java
namespace Packmaster.PackmasterCode.Powers.Boardgamepack;

/// <summary>At the start of your turn, if you have no Dice, roll [Amount] D8.</summary>
public sealed class PerceptionCheckPower : PackmasterPower
{
    public const int Sides = 8;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || Owner.HasPower<DicePower>()) return;
        Flash();
        await Dice.Roll(choiceContext, player, Sides, Amount);
    }
}
