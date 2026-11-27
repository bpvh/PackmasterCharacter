// Ported from src/main/java/thePackmaster/cards/madsciencepack/AbstractMadScienceCard.java
namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

public abstract class AbstractMadScienceCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : PackmasterCard(cost, type, rarity, target);
