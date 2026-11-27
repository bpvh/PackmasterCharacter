using MegaCrit.Sts2.Core.Runs;

namespace Packmaster.PackmasterCode.Packs;

/// <summary>
/// Per-player pack state for the current run, saved with the run through BaseLib SavedSpireFields.
/// Replaces StS1 SpireAnniversary5Mod.currentPoolPacks (a global) so co-op runs can have several Packmasters.
///
/// Lifecycle (StS1 startOfGamePackSetup + PackSetupScreen):
///  1. Resolve (lazily, the first time anything asks): fixed and random slots go straight into Selected;
///     every "choice" slot becomes a draft round whose offer is computed up front. Randomness comes from a
///     private SplitMix64 seeded by (run seed, player NetId), so it doesn't touch game RNG streams and gives
///     the same result on every co-op client regardless of when it runs.
///  2. Draft (PackDraft): the player picks one pack per round; picks are appended to Selected.
///  3. Mutations (Banishing Decree etc.): Ban / AddPack / RemovePack.
///
/// StS1 rule kept: random slots and draft offers are drawn without replacement from one shuffled pool, so a
/// pack that was offered and passed over is not offered again.
/// </summary>
public static class PackRunState
{
    private const char Sep = ',';
    private const char RoundSep = ';';

    private static readonly SavedSpireField<Player, string> SelectedField = new(() => "", "packmaster_selected_packs");
    private static readonly SavedSpireField<Player, string> OffersField = new(() => "", "packmaster_draft_offers");
    private static readonly SavedSpireField<Player, string> BannedField = new(() => "", "packmaster_banned_packs");
    private static readonly SavedSpireField<Player, bool> ResolvedField = new(() => false, "packmaster_packs_resolved");

    public static bool IsPackmaster(Player? player) => player?.Character is Character.ThePackmaster;

    /// <summary>Packs in this player's run so far, in selection order.</summary>
    public static IReadOnlyList<PackmasterPack> Selected(Player player)
    {
        EnsureResolved(player);
        return Parse(SelectedField[player]);
    }

    public static bool HasPack(Player player, PackmasterPack pack) => Selected(player).Contains(pack);

    /// <summary>Remaining draft rounds, each a list of offered packs.</summary>
    public static IReadOnlyList<IReadOnlyList<PackmasterPack>> PendingOffers(Player player)
    {
        EnsureResolved(player);
        var raw = OffersField[player] ?? "";
        return raw.Split(RoundSep, StringSplitOptions.RemoveEmptyEntries).Select(r => (IReadOnlyList<PackmasterPack>)Parse(r)).ToList();
    }

    public static bool DraftPending(Player player) => PendingOffers(player).Count > 0;

    /// <summary>Record the pick for the first pending round.</summary>
    public static void CompleteRound(Player player, PackmasterPack? pick)
    {
        var rounds = PendingOffers(player).ToList();
        if (rounds.Count == 0) return;
        rounds.RemoveAt(0);
        OffersField[player] = string.Join(RoundSep, rounds.Select(r => Join(r)));
        if (pick != null) AddPack(player, pick);
    }

    public static void AddPack(Player player, PackmasterPack pack)
    {
        var selected = Selected(player).ToList();
        if (selected.Contains(pack)) return;
        selected.Add(pack);
        SelectedField[player] = Join(selected);
    }

    public static void RemovePack(Player player, PackmasterPack pack)
    {
        SelectedField[player] = Join(Selected(player).Where(p => p != pack));
    }

    /// <summary>Banishing Decree: remove a pack and keep it out of future offers for this run.</summary>
    public static void Ban(Player player, PackmasterPack pack)
    {
        RemovePack(player, pack);
        var banned = Parse(BannedField[player]).ToList();
        if (!banned.Contains(pack)) banned.Add(pack);
        BannedField[player] = Join(banned);
    }

    public static IReadOnlyList<PackmasterPack> Banned(Player player) => Parse(BannedField[player]);

    /// <summary>Enabled packs not in the run and not banned; for effects that offer new packs mid-run.</summary>
    public static IReadOnlyList<PackmasterPack> Unselected(Player player)
    {
        var have = Selected(player).Concat(Banned(player)).ToHashSet();
        return PackRegistry.Enabled.Where(p => !have.Contains(p)).ToList();
    }

    // ------------------------------------------------------------------------------------------------------

    private static void EnsureResolved(Player player)
    {
        if (ResolvedField[player]) return;
        ResolvedField[player] = true;

        var result = Resolve(EffectiveSlots(player), PackRegistry.Enabled.ToList(), Seed(player),
            AllPacksMode(player), PackmasterConfig.AllowMultipleNone && !IsMultiplayer(player),
            Math.Max(1, IsMultiplayer(player) ? 3 : PackmasterConfig.PacksPerChoice),
            PackmasterConfig.SkipDraft && !IsMultiplayer(player));
        SelectedField[player] = Join(result.Selected);
        OffersField[player] = string.Join(RoundSep, result.Offers.Select(o => Join(o)));
        MainFile.Logger.Info($"Packs for player {player.NetId}: {Join(result.Selected)}; draft rounds: {result.Offers.Count}");
    }

    public sealed record Resolution(List<PackmasterPack> Selected, List<List<PackmasterPack>> Offers);

    /// <summary>Pure slot resolution (testable without a run).</summary>
    public static Resolution Resolve(IReadOnlyList<string> slots, List<PackmasterPack> enabled, ulong seed,
        bool allPacks, bool allowMultipleNone, int packsPerChoice, bool skipDraft)
    {
        if (allPacks) return new Resolution(enabled.ToList(), []);

        var selected = new List<PackmasterPack>();
        int randoms = 0, choices = 0;
        bool noneUsed = false;
        foreach (var raw in slots)
        {
            var slot = raw.Trim();
            if (slot.Equals(PackmasterConfig.Random, StringComparison.OrdinalIgnoreCase)) randoms++;
            else if (slot.Equals(PackmasterConfig.Choice, StringComparison.OrdinalIgnoreCase)) choices++;
            else if (slot.Equals(PackmasterConfig.None, StringComparison.OrdinalIgnoreCase))
            {
                if (noneUsed && !allowMultipleNone) randoms++;
                noneUsed = true;
            }
            else
            {
                var pack = enabled.FirstOrDefault(p => p.PackId.Equals(slot, StringComparison.OrdinalIgnoreCase));
                if (pack != null && !selected.Contains(pack)) selected.Add(pack);
                else randoms++; // unknown, disabled or duplicate fixed pack: StS1 skipped it; a random pack keeps the pool size.
            }
        }

        var pool = enabled.Where(p => !selected.Contains(p)).ToList();
        var rng = new SplitMix64(seed);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = (int)(rng.Next() % (ulong)(i + 1));
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int next = 0;
        for (int i = 0; i < randoms && next < pool.Count; i++) selected.Add(pool[next++]);

        var offers = new List<List<PackmasterPack>>();
        for (int round = 0; round < choices && next < pool.Count; round++)
        {
            var offer = new List<PackmasterPack>();
            for (int k = 0; k < packsPerChoice && next < pool.Count; k++) offer.Add(pool[next++]);
            if (skipDraft) selected.Add(offer[0]);
            else offers.Add(offer);
        }

        return new Resolution(selected, offers);
    }

    private static bool IsMultiplayer(Player player) => player.RunState.Players.Count > 1;

    private static bool AllPacksMode(Player player) => PackmasterConfig.AllPacksMode && !IsMultiplayer(player);

    private static IReadOnlyList<string> EffectiveSlots(Player player) =>
        PackmasterConfig.SplitList(IsMultiplayer(player) ? PackmasterConfig.DefaultSlots : PackmasterConfig.DraftSlots);

    private static ulong Seed(Player player)
    {
        // Run seed's string form is stable whatever its type; hash it together with the player's NetId.
        var text = $"{player.RunState.Rng.Seed}|{player.NetId}|packmaster";
        ulong h = 14695981039346656037UL; // FNV-1a
        foreach (var ch in text)
        {
            h ^= ch;
            h *= 1099511628211UL;
        }
        return h;
    }

    private static List<PackmasterPack> Parse(string? raw) =>
        (raw ?? "").Split(Sep, StringSplitOptions.RemoveEmptyEntries)
            .Select(PackRegistry.Get).Where(p => p != null).Select(p => p!).ToList();

    private static string Join(IEnumerable<PackmasterPack> packs) => string.Join(Sep, packs.Select(p => p.PackId));

    /// <summary>Small deterministic PRNG, identical on every platform and .NET version.</summary>
    private struct SplitMix64(ulong seed)
    {
        private ulong _state = seed;

        public ulong Next()
        {
            ulong z = _state += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }
}
