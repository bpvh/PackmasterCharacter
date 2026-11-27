// Ported from src/main/java/thePackmaster/powers/alignmentpack/TrinePower.java
using MegaCrit.Sts2.Core.Combat;

namespace Packmaster.PackmasterCode.Powers.Alignmentpack;

/// <summary>The next [Amount] cards you play this turn are played twice. Removed at end of turn (vanilla Burst pattern).</summary>
public sealed class TrinePower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        return IsOwnersCard(card) && Amount > 0 ? playCount + 1 : playCount;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        await PowerCmd.Decrement(this);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side) await PowerCmd.Remove(this);
    }
}
