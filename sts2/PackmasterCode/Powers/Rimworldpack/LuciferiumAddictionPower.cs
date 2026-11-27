// Ported from src/main/java/thePackmaster/powers/rimworldpack/LuciferiumAddictionPower.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Cards.Rimworldpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Rimworldpack;

/// <summary>Counts down each turn. At 0: add 2 Despair to your hand and lose 5 Strength.</summary>
public sealed class LuciferiumAddictionPower : PackmasterPower
{
    private const int Despairs = 2;
    private const int StrengthLoss = 5;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }
        Flash();
        await PowerCmd.Remove(this);
        for (int i = 0; i < Despairs; i++) await Wiz.MakeInHand(Wiz.CreateCard<Despair>(player), player);
        await Wiz.ApplyPower<StrengthPower>(choiceContext, Owner, -StrengthLoss, Owner);
    }
}
