// Ported from src/main/java/thePackmaster/cards/evenoddpack/AbstractEvenOddCard.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>
/// Even/Odd pack base card. Effects depend on this card's position among the cards you play this turn
/// (1st, 2nd, ...): "First", "Even" or "Odd". StS1 also grayed out the inactive half of the text; here the active
/// half glows the card gold instead.
/// </summary>
public abstract class AbstractEvenOddCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : PackmasterCard(cost, type, rarity, target)
{
    /// <summary>Position this play has (during OnPlay) or would have (in hand).</summary>
    protected int Position(CardPlay? play = null) => IsCanonical ? 1 : Wiz.PlayPosition(Owner, play);

    protected bool IsFirst(CardPlay? play = null) => Position(play) == 1;
    protected bool IsEven(CardPlay? play = null) => Position(play) % 2 == 0;
    protected bool IsOdd(CardPlay? play = null) => Position(play) % 2 == 1;
}
