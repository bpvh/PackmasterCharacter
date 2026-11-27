// Ported from src/main/java/thePackmaster/powers/rimworldpack/CatharsisPower.java
using MegaCrit.Sts2.Core.Combat;

namespace Packmaster.PackmasterCode.Powers.Rimworldpack;

/// <summary>Your next [Amount] card(s) this turn cost 0 and are played twice. Removed at end of turn.</summary>
public sealed class CatharsisPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private CardModel? _sourceCard;
    private CardModel? _freeCard;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        _sourceCard = cardSource;
    }

    // Vanilla FreeAttackPower pattern: make cards free while this is active, consume on play.
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!IsOwnersCard(card) || card == _sourceCard || Amount <= 0) return false;
        modifiedCost = 0;
        return true;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (IsOwnersCard(cardPlay.Card) && cardPlay.Card != _sourceCard) _freeCard = cardPlay.Card;
        return Task.CompletedTask;
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        card == _freeCard ? playCount + 1 : playCount;

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (card != _freeCard) return;
        _freeCard = null;
        Flash();
        await PowerCmd.Decrement(this);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side) await PowerCmd.Remove(this);
    }
}
