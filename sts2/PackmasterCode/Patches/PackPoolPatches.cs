using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using Packmaster.PackmasterCode.Character;
using Packmaster.PackmasterCode.Packs;

namespace Packmaster.PackmasterCode.Patches;

/// <summary>
/// The run's card pool = the cards of the selected packs (StS1 rebuilt the dungeon card pools instead).
///
/// Every "random class card" source (card rewards, shops, events, potions, random-card effects, BaseLib's
/// CommonActions.GenerateCards) reads the character pool through CardPoolModel.GetUnlockedCards, so that's
/// where pack cards outside the run are removed. Non-pack cards in our pool (starters, system cards, pack
/// preview cards) are left alone; rarity rules already keep Basic/Token cards out of rewards.
///
/// GetUnlockedCards has no player argument. When a caller does (CardCreationOptions.GetPossibleCards(player),
/// used for rewards and shops), we remember that player for the duration of the call; otherwise we use every
/// Packmaster in the current run (in single player that's just you).
/// </summary>
[HarmonyPatch]
public static class PackPoolPatches
{
    [ThreadStatic] private static Player? _contextPlayer;

    // ---- player context ------------------------------------------------------------------------------

    [HarmonyPatch]
    private static class PossibleCardsContext
    {
        // VERIFY against sts2.dll: CardCreationOptions.GetPossibleCards(Player). Patch is skipped if not found.
        private static MethodBase? TargetMethod()
        {
            var type = AccessTools.TypeByName("CardCreationOptions");
            return type == null ? null : AccessTools.Method(type, "GetPossibleCards");
        }

        private static bool Prepare(MethodBase? original)
        {
            if (original != null) return true;
            var found = TargetMethod() != null;
            if (!found) MainFile.Logger.Info("CardCreationOptions.GetPossibleCards not found; pack pool uses all Packmasters in the run.");
            return found;
        }

        private static void Prefix(object[] __args, out Player? __state)
        {
            __state = _contextPlayer;
            if (__args.FirstOrDefault(a => a is Player) is Player p) _contextPlayer = p;
        }

        private static void Finalizer(Player? __state) => _contextPlayer = __state;
    }

    // ---- filtering -----------------------------------------------------------------------------------

    [HarmonyPatch(typeof(CardPoolModel), nameof(CardPoolModel.GetUnlockedCards))]
    [HarmonyPostfix]
    private static void FilterToRunPacks(CardPoolModel __instance, ref IEnumerable<CardModel> __result)
    {
        if (__instance is not PackmasterCardPool) return;
        try
        {
            var players = PlayersForContext();
            if (players.Count == 0) return; // not in a run (compendium, menus): show everything

            var allowed = players.SelectMany(PackRunState.Selected).ToHashSet();
            var cards = __result.ToList();
            var filtered = cards.Where(c =>
            {
                var packs = PackRegistry.PacksOf(c);
                return packs.Count == 0 || packs.Any(allowed.Contains);
            }).ToList();

            // Reward generation picks a rarity first; an empty rarity could break it. If the selected packs have
            // no card of a rarity the full pool has (only possible with tiny custom drafts), fall back to all
            // cards of that rarity.
            foreach (var rarity in new[] { CardRarity.Common, CardRarity.Uncommon, CardRarity.Rare })
            {
                if (filtered.Any(c => c.Rarity == rarity)) continue;
                filtered.AddRange(cards.Where(c => c.Rarity == rarity));
            }

            __result = filtered;
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"Pack pool filter failed, leaving the pool unfiltered: {e}");
        }
    }

    private static List<Player> PlayersForContext()
    {
        if (_contextPlayer != null)
            return PackRunState.IsPackmaster(_contextPlayer) ? [_contextPlayer] : [];
        var run = CurrentRun();
        return run?.Players.Where(PackRunState.IsPackmaster).ToList() ?? [];
    }

    // RunManager.Instance's run state; the getter isn't public in every build, so use reflection.
    private static readonly MethodInfo? RunStateGetter =
        AccessTools.PropertyGetter(typeof(RunManager), "State") ?? AccessTools.PropertyGetter(typeof(RunManager), "RunState");

    private static RunState? CurrentRun()
    {
        var manager = RunManager.Instance;
        return manager == null ? null : RunStateGetter?.Invoke(manager, null) as RunState;
    }
}
