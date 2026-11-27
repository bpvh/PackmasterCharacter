using MegaCrit.Sts2.Core.Combat;

namespace Packmaster.PackmasterCode.Powers;

/// <summary>
/// StS1 vanilla ReboundPower, reimplemented: the next card you play this turn (other than the card that
/// applied this) is put on top of your draw pile instead of its normal destination. Removed at end of turn.
/// Amount = number of cards affected.
/// Only moves cards that ended in the discard pile, so Powers and Exhausted cards are left alone (StS1 behavior).
/// </summary>
public sealed class CardistryPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string ArtId => "Rebound"; // VERIFY: no StS1 mod art exists; sync_assets copies a placeholder.

    private CardModel? _sourceCard;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        _sourceCard = cardSource;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!IsOwnersCard(card)) return;
        if (card == _sourceCard)
        {
            // The card that applied this power doesn't count.
            _sourceCard = null;
            return;
        }

        if (card.Pile?.Type == PileType.Discard)
        {
            Flash();
            // VERIFY: CardPilePosition.Top (Bottom and Random are confirmed in other mods).
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
        }

        await PowerCmd.Decrement(this);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side) await PowerCmd.Remove(this);
    }
}
