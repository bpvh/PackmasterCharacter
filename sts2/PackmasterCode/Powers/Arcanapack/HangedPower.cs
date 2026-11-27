// Ported from src/main/java/thePackmaster/powers/arcanapack/HangedPower.java
using MegaCrit.Sts2.Core.Combat;

namespace Packmaster.PackmasterCode.Powers.Arcanapack;

/// <summary>This turn, whenever you draw a card, ALL enemies lose [Amount] HP. Removed at end of turn.</summary>
public sealed class HangedPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (!IsOwnersCard(card) || CombatState == null) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies.ToList(), Amount, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side) await PowerCmd.Remove(this);
    }
}
