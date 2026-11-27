// Ported from src/main/java/thePackmaster/powers/prismaticpack/FlawlessPrismPower.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Prismaticpack;

/// <summary>At the start of your turn, add [Amount] random Power(s) of another color to your hand.</summary>
public sealed class FlawlessPrismPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string ArtId => "FlawlessPrism";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        foreach (var model in Wiz.RandomOtherColorCards(player, CardType.Power, null, (int)Amount))
            await Wiz.MakeInHand(Wiz.CreateCard(model, player), player);
    }
}
