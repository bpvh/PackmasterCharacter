// Ported from src/main/java/thePackmaster/cards/aggressionpack/AbstractAggressionCard.java
namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

public abstract class AbstractAggressionCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : PackmasterCard(cost, type, rarity, target);
