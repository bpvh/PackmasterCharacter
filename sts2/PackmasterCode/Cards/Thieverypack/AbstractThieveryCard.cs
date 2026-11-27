// Ported from src/main/java/thePackmaster/cards/thieverypack/AbstractThieveryCard.java
namespace Packmaster.PackmasterCode.Cards.Thieverypack;

public abstract class AbstractThieveryCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : PackmasterCard(cost, type, rarity, target);
