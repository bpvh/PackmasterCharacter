using MegaCrit.Sts2.Core.Localization;

namespace Packmaster.PackmasterCode.Packs;

/// <summary>
/// A Booster Pack (StS1 AbstractCardPack). One subclass per pack; PackRegistry finds every subclass in every
/// loaded assembly, so other mods (an Expansion Packs port) can add packs just by defining them.
///
/// Text lives in the packmaster_ui table, converted from StS1 UIstrings:
///   PACKMASTER-{SLUG}.text0 = name, .text1 = description, .text2 = author (and .text3 = credits for some packs).
/// The draft shows the pack's preview card (PreviewCardType), whose art is the StS1 pack art (cards/{PackId}.png).
/// </summary>
public abstract class PackmasterPack
{
    /// <summary>StS1 pack ID without prefix, e.g. "StrikesPack". Stored in saves; never rename.</summary>
    public abstract string PackId { get; }

    /// <summary>Card classes in this pack (C# types of CardModel subclasses).</summary>
    public abstract IReadOnlyList<Type> CardTypes { get; }

    /// <summary>The PackPreviewCard subclass shown for this pack in the draft and pack lists.</summary>
    public abstract Type PreviewCardType { get; }

    public abstract PackSummary Summary { get; }

    /// <summary>Potions that only appear while this pack is in the run (StS1 getPackPotions).</summary>
    public virtual IReadOnlyList<Type> PotionTypes => [];

    /// <summary>False for packs that exist but are not yet ported; they never appear in drafts.</summary>
    public virtual bool Enabled => true;

    public string LocEntry => Porting.Entry(PackId);

    public LocString Name => new("packmaster_ui", $"{LocEntry}.text0");
    public LocString Description => new("packmaster_ui", $"{LocEntry}.text1");
    public LocString Author => new("packmaster_ui", $"{LocEntry}.text2");

    private IReadOnlyList<CardModel>? _cards;

    /// <summary>Canonical card models of this pack.</summary>
    public IReadOnlyList<CardModel> Cards => _cards ??= CardTypes.Select(PackRegistry.CardFor).ToList();

    public CardModel PreviewCard => PackRegistry.CardFor(PreviewCardType);

    public override string ToString() => PackId;
}

/// <summary>Mirrors tools/porting/ids.py so C# and the Python tools derive the same loc keys.</summary>
public static class Porting
{
    public const string Prefix = "PACKMASTER-";

    public static string Slug(string name)
    {
        var s = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1_$2");
        s = System.Text.RegularExpressions.Regex.Replace(s, "([A-Z]+)([A-Z][a-z])", "$1_$2");
        s = System.Text.RegularExpressions.Regex.Replace(s, "([A-Za-z])([0-9])", "$1_$2");
        s = System.Text.RegularExpressions.Regex.Replace(s, "([0-9])([A-Za-z])", "$1_$2");
        s = System.Text.RegularExpressions.Regex.Replace(s, "_+", "_").Trim('_');
        return s.ToUpperInvariant();
    }

    public static string Entry(string name) => Prefix + Slug(name);
}
