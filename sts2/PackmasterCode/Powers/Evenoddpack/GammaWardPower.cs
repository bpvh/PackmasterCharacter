// Ported from src/main/java/thePackmaster/powers/evenoddpack/GammaWardPower.java
using MegaCrit.Sts2.Core.Combat;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Evenoddpack;

/// <summary>Your next Skill this turn has "Even: gain [Amount] Block". Removed when a Skill is played or at end of turn.</summary>
public sealed class GammaWardPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private CardModel? _sourceCard;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        _sourceCard = cardSource;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!IsOwnersCard(card) || card.Type != CardType.Skill) return;
        if (card == _sourceCard) { _sourceCard = null; return; }
        if (Wiz.PlayPosition(Owner.Player!, cardPlay) % 2 == 0)
        {
            Flash();
            // StS1 ran the Block through the card's modifiers (Dexterity etc.), so this is powered Block.
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, cardPlay);
        }
        await PowerCmd.Remove(this);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side) await PowerCmd.Remove(this);
    }
}
