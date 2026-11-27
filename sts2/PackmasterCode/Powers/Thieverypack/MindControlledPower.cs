// Ported from src/main/java/thePackmaster/powers/thieverypack/MindControlledPower.java and patches/thieverypack/MindControlPatch.java
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Thieverypack;

/// <summary>
/// This enemy fights for you: its attacks hit a random other (not mind-controlled) enemy, it can't apply powers to
/// players, and when only mind-controlled enemies are left they die, ending the combat.
/// </summary>
public sealed class MindControlledPower : PackmasterPower
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>StS1 turned other debuffs into Vulnerable on the new target; here they are simply dropped.</summary>
    public override decimal ModifyPowerAmountGiven(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource) =>
        giver == Owner && target is { IsPlayer: true } ? 0m : amount;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource) => await EndCombatIfOnlyControlledLeft(Owner);

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength) =>
        await EndCombatIfOnlyControlledLeft(Owner);

    private static async Task EndCombatIfOnlyControlledLeft(Creature any)
    {
        var state = any.CombatState;
        if (state == null) return;
        var alive = state.Enemies.Where(e => e.IsAlive).ToList();
        if (alive.Count == 0 || alive.Any(e => !e.HasPower<MindControlledPower>())) return;
        await CreatureCmd.Kill(alive, force: true);
    }

    // VERIFY against the current build: monster attacks (FromMonster) target "all opponents", resolved by the
    // private AttackCommand.GetPossibleTargets. For a mind-controlled attacker, pick another enemy instead.
    [HarmonyPatch(typeof(AttackCommand), "GetPossibleTargets")]
    private static class RedirectPatch
    {
        private static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
        {
            var attacker = __instance.Attacker;
            if (attacker == null || !attacker.HasPower<MindControlledPower>() || attacker.CombatState is not { } state) return;
            var others = state.Enemies.Where(e => e.IsAlive && e != attacker && !e.HasPower<MindControlledPower>()).ToList();
            if (others.Count == 0)
            {
                __result = [attacker];
                return;
            }
            var rng = state.RunState.Rng.CombatTargets;
            __result = [rng.NextItem(others)!];
        }
    }
}
