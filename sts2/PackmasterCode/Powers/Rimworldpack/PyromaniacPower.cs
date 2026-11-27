// Ported from src/main/java/thePackmaster/powers/rimworldpack/PyromaniacPower.java
using Packmaster.PackmasterCode.CardModifiers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Rimworldpack;

/// <summary>At the start of your turn, set [Amount] random card(s) in your hand On Fire.</summary>
public sealed class PyromaniacPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return Task.CompletedTask;
        var hand = Wiz.Hand(player).ToList();
        if (hand.Count == 0) return Task.CompletedTask;
        Flash();
        var rng = Wiz.CardRng(player);
        for (int i = 0; i < Amount; i++)
            CardModifier.AddModifier<OnFireModifier>(rng.NextItem(hand)!, 1);
        return Task.CompletedTask;
    }
}
