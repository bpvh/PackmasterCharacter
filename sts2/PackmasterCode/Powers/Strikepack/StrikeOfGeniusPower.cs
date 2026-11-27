// Ported from src/main/java/thePackmaster/powers/strikepack/StrikeOfGeniusPower.java
namespace Packmaster.PackmasterCode.Powers.Strikepack;

/// <summary>
/// Your next [Amount] Attack(s) gain "Gain Block equal to unblocked damage dealt". The card that applied this
/// doesn't count (StS1 justApplied).
/// </summary>
public sealed class StrikeOfGeniusPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private CardModel? _sourceCard;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        _sourceCard = cardSource;
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || cardSource == null || cardSource == _sourceCard || cardSource.Type != CardType.Attack) return;
        if (result.UnblockedDamage <= 0) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, result.UnblockedDamage, ValueProp.Unpowered, null);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!IsOwnersCard(cardPlay.Card)) return;
        if (cardPlay.Card == _sourceCard)
        {
            _sourceCard = null;
            return;
        }
        if (cardPlay.Card.Type == CardType.Attack) await PowerCmd.Decrement(this);
    }
}
