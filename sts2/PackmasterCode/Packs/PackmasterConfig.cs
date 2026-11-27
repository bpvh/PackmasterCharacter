using BaseLib.Config;

namespace Packmaster.PackmasterCode.Packs;

/// <summary>
/// Mod settings (BaseLib generates the UI under Settings / Mod Configuration).
/// Mirrors the StS1 options: custom draft, All Packs mode, multiple NONE slots, pack filter, summaries.
/// Loc: settings_ui.json, keys PACKMASTER-{PROPERTY_NAME}.title.
///
/// Multiplayer: every player's run must resolve identically on all clients, and these settings are local,
/// so co-op runs always use the default slots and no filter (see PackRunState.EffectiveSlots).
/// </summary>
[ConfigHoverTipsByDefault]
internal sealed class PackmasterConfig : SimpleModConfig
{
    public const string Random = "random";
    public const string Choice = "choice";
    public const string None = "none";
    public const string DefaultSlots = "CoreSetPack,random,random,random,choice,choice,choice";

    /// <summary>Every enabled pack is in the run; no draft.</summary>
    public static bool AllPacksMode { get; set; } = false;

    /// <summary>
    /// Comma-separated draft slots, in order: a pack ID (e.g. CoreSetPack), "random", "choice" or "none".
    /// StS1 default: Core Set, 3 random packs, then 3 rounds of choosing 1 of 3.
    /// </summary>
    [ConfigTextInput(TextInputPreset.Anything, MaxLength = 400)]
    public static string DraftSlots { get; set; } = DefaultSlots;

    /// <summary>StS1 "Allow multiple NONE": otherwise only the first "none" slot counts and the rest roll random.</summary>
    public static bool AllowMultipleNone { get; set; } = false;

    /// <summary>Packs per draft round (StS1 PACKS_PER_CHOICE).</summary>
    [ConfigSlider(2, 5, 1)]
    public static int PacksPerChoice { get; set; } = 3;

    /// <summary>Comma-separated pack IDs that never appear in random slots or draft offers (StS1 pack filter).</summary>
    [ConfigTextInput(TextInputPreset.Anything, MaxLength = 2000)]
    public static string DisabledPacks { get; set; } = "";

    public static bool ShowPackSummaries { get; set; } = true;

    /// <summary>Skip the draft: choice slots are resolved randomly.</summary>
    [ConfigSection("Debug", CollapsedByDefault = true)]
    public static bool SkipDraft { get; set; } = false;

    public static bool IsPackAllowed(string packId) =>
        !SplitList(DisabledPacks).Contains(packId, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<string> SplitList(string s) =>
        s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
