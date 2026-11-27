// Ported from src/main/java/thePackmaster/cards/boardgamepack/AbstractBoardCard.java
namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

public abstract class AbstractBoardCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : PackmasterCard(cost, type, rarity, target)
{
    /// <summary>StS1 isChance: played automatically when drawn (Exhaust comes from the card's own keyword).</summary>
    protected virtual bool IsChance => false;

    // VERIFY: AutoPlay of an Unplayable card from hand (StS1 queued it with the "auto-play" flag).
    public override async Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        if (card == this && IsChance) await CardCmd.AutoPlay(ctx, this, null);
    }
}
