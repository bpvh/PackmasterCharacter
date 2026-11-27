// StS1 vanilla DuplicationPower (used by several packs, e.g. Even/Odd's Element of Surprise).
using MegaCrit.Sts2.Core.Combat;

namespace Packmaster.PackmasterCode.Powers;

/// <summary>Your next [Amount] card(s) this turn are played twice. Removed at end of turn.</summary>
public sealed class DuplicationPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        IsOwnersCard(card) && Amount > 0 ? playCount + 1 : playCount;

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
