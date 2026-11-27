// Ported from src/main/java/thePackmaster/cards/bellordpack/AbstractBellordCard.java
using MegaCrit.Sts2.Core.Models.Cards;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>
/// Bellord pack base card. Several Bellord cards add Curse of the Bell to your deck when you obtain them (StS1
/// OnObtainCard); set CursesOnObtain. Bellord cards also care about Curses and Statuses in hand.
/// </summary>
public abstract class AbstractBellordCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : PackmasterCard(cost, type, rarity, target)
{
    protected virtual int CursesOnObtain => 0;

    /// <summary>StS1 "q.type == CURSE || q.type == STATUS || q.color == CURSE".</summary>
    public static bool IsCurseOrStatus(CardModel c) => c.Type is CardType.Curse or CardType.Status;

    protected bool CurseOrStatusInHand => !IsCanonical && Wiz.Hand(Owner).Any(IsCurseOrStatus);

    // VERIFY: AfterCardChangedPiles is a run-level hook (Hook.AfterCardChangedPiles takes IRunState), so a card in
    // the deck should receive it when it is added to the deck (old pile None).
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card != this || CursesOnObtain <= 0 || oldPileType != PileType.None || Pile?.Type != PileType.Deck) return;
        for (int i = 0; i < CursesOnObtain; i++)
            await CardPileCmd.AddCurseToDeck<CurseOfTheBell>(Owner);
    }
}
