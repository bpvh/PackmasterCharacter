// Ported from src/main/java/thePackmaster/cards/utilitypack/AbstractUtilityCard.java
namespace Packmaster.PackmasterCode.Cards.Utilitypack;

public abstract class AbstractUtilityCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : PackmasterCard(cost, type, rarity, target);
