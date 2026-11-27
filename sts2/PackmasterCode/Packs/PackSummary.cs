namespace Packmaster.PackmasterCode.Packs;

/// <summary>StS1 AbstractCardPack.PackSummary.Tags.</summary>
public enum PackTag
{
    None, Strength, Exhaust, Orbs, Stances, Discard, Debuffs, Attacks, Tokens, Powers
}

/// <summary>
/// Ratings shown on the pack draft screen (StS1 PackSummary): 0-5 stars each for offense, defense, support,
/// frontload and scaling, plus archetype tags.
/// </summary>
public sealed record PackSummary(int Offense, int Defense, int Support, int Frontload, int Scaling, params PackTag[] Tags)
{
    public IReadOnlyList<PackTag> EffectiveTags => Tags.Length == 0 ? [PackTag.None] : Tags;
}
