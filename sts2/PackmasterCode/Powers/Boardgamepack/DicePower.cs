// Ported from src/main/java/thePackmaster/powers/boardgamepack/DicePower.java
namespace Packmaster.PackmasterCode.Powers.Boardgamepack;

/// <summary>
/// The next card you play that deals damage or gains Block deals/gains [Amount] more. Used up by that card unless it
/// has Reroll. Dice gained while the card resolves (Flick Dice's own roll) are kept, as in StS1.
/// </summary>
public sealed class DicePower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private CardModel? _consumer;
    private decimal _consumeAmount;

    private bool UsesDice(CardModel card) =>
        IsOwnersCard(card) && card is not IRerollCard &&
        (card.Type == CardType.Attack || card.DynamicVars.Keys.Contains("Block") || card.DynamicVars.Keys.Contains("CalculatedBlock"));

    private static bool Powered(ValueProp props) => props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);

#if OLD_STS2_API
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
#else
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
#endif
    {
        return dealer == Owner && cardSource != null && Powered(props) ? Amount : 0m;
    }

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        return target == Owner && cardSource != null && Powered(props) ? Amount : 0m;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (_consumer == null && UsesDice(cardPlay.Card))
        {
            _consumer = cardPlay.Card;
            _consumeAmount = Amount;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card != _consumer) return;
        _consumer = null;
        Flash();
        if (_consumeAmount >= Amount) await PowerCmd.Remove(this);
        else await Util.Wiz.ModifyPowerAmount(this, -_consumeAmount);
    }
}
