// Ported from src/main/java/thePackmaster/cards/ringofpainpack/AbstractEvolveCard.java
using Packmaster.PackmasterCode.Extensions;

namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>
/// Ring of Pain "Evolve" cards: they can be upgraded up to 3 times (MaxUpgradeLevel), and Evolve (upgrade once)
/// when their trigger happens while they're in your hand. At level 3 they are Evolved: the card text switches to
/// the evolved effect (var "Evolved" = 1 drives {Evolved:cond:...} in the loc) and the art to {ArtId}Evolved.
/// VERIFY: StS2's multi-level upgrade flow (CurrentUpgradeLevel is already incremented when OnUpgrade runs).
/// </summary>
public abstract class AbstractEvolveCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : AbstractRingOfPainCard(cost, type, rarity, target)
{
    public const int MaxUpgrades = 3;
    public const string EvolvedVar = "Evolved";

    public override int MaxUpgradeLevel => MaxUpgrades;

    protected bool IsEvolved => CurrentUpgradeLevel >= MaxUpgrades;

    protected bool InHand => !IsCanonical && Pile?.Type == PileType.Hand;

    public override string PortraitPath => IsEvolved ? $"{ArtId}Evolved.png".CardImagePathOrDefault(base.PortraitPath) : base.PortraitPath;
    public override string CustomPortraitPath => IsEvolved ? $"{ArtId}Evolved.png".BigCardImagePathOrDefault(base.CustomPortraitPath!) : base.CustomPortraitPath!;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        if (IsEvolved) DynamicVars[EvolvedVar].BaseValue = 1;
    }

    /// <summary>StS1 evolve(): upgrade once, if not fully evolved.</summary>
    protected void Evolve()
    {
        if (IsUpgradable) CardCmd.Upgrade(this);
    }
}
