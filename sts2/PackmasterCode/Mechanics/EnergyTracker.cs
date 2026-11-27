// Ported from src/main/java/thePackmaster/patches/energyandechopack/ (GenerateEnergyPatch, UsedEnergyPatch)
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Mechanics;

/// <summary>Energy spent and energy generated (gained during your turn, not the turn-start refill) this turn.</summary>
public static class EnergyTracker
{
    private sealed class Mark { public object? Combat; public int Turn; public int Generated; }
    private static readonly ConditionalWeakTable<Player, Mark> Generated = new();

    public static int SpentThisTurn(Player p)
    {
        var state = p.Creature.CombatState;
        if (state == null) return 0;
        return CombatManager.Instance.History.Entries.OfType<EnergySpentEntry>()
            .Where(e => e.Actor == p.Creature && e.HappenedThisTurn(state)).Sum(e => e.Amount);
    }

    public static int GeneratedThisTurn(Player p) =>
        Generated.TryGetValue(p, out var m) && ReferenceEquals(m.Combat, p.Creature.CombatState) && m.Turn == Wiz.TurnNumber(p)
            ? m.Generated
            : 0;

    // StS1 counted AbstractPlayer.gainEnergy while the player's turn hadn't ended (including start-of-turn relics).
    // The turn-start refill goes through the energy reset, not PlayerCmd.GainEnergy. VERIFY in game.
    [HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.GainEnergy))]
    private static class GainEnergyPatch
    {
        private static void Prefix(decimal amount, Player player)
        {
            var state = player?.Creature.CombatState;
            if (state == null || amount <= 0 || state.CurrentSide != CombatSide.Player) return;
            var m = Generated.GetOrCreateValue(player!);
            var turn = Wiz.TurnNumber(player!);
            if (!ReferenceEquals(m.Combat, state) || m.Turn != turn)
            {
                m.Combat = state;
                m.Turn = turn;
                m.Generated = 0;
            }
            m.Generated += (int)amount;
        }
    }
}
