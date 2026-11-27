using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using Packmaster.PackmasterCode.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Stances;

/// <summary>
/// StS1 stances. StS2 has none, so a stance is a single-instance buff power; a creature is in at most one
/// Packmaster stance at a time (no stance power = StS1 Neutral). Change stance only through <see cref="Stance"/>
/// so exit/enter effects and <see cref="IStanceListener"/> hooks run.
/// </summary>
public abstract class PackmasterStance : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>StS1 onEnterStance. Runs after the power is applied.</summary>
    public virtual Task OnEnter(PlayerChoiceContext ctx) => Task.CompletedTask;

    /// <summary>StS1 onExitStance. Runs before the power is removed.</summary>
    public virtual Task OnExit(PlayerChoiceContext ctx) => Task.CompletedTask;
}

/// <summary>StS1 AbstractPower.onChangeStance. Implement on powers; called for every power of the creature that changed.</summary>
public interface IStanceListener
{
    /// <param name="oldStance">Stance type left, null for Neutral.</param>
    /// <param name="newStance">Stance type entered, null for Neutral.</param>
    Task AfterStanceChanged(PlayerChoiceContext ctx, Type? oldStance, Type? newStance);
}

public static class Stance
{
    private sealed class Mark { public object? Combat; public int Turn; }
    private static readonly ConditionalWeakTable<Creature, Mark> LastChange = new();

    public static PackmasterStance? Current(Creature c) => c.Powers.OfType<PackmasterStance>().FirstOrDefault();

    public static bool IsIn<T>(Creature c) where T : PackmasterStance => Current(c) is T;

    public static bool IsNeutral(Creature c) => Current(c) == null;

    /// <summary>StS1 switchedStance this turn (Dark Lance). Changing to the same stance doesn't count.</summary>
    public static bool ChangedThisTurn(Creature c) =>
        LastChange.TryGetValue(c, out var m) && ReferenceEquals(m.Combat, c.CombatState) && c.Player != null &&
        m.Turn == Wiz.TurnNumber(c.Player);

    /// <summary>StS1 ChangeStanceAction(new T()).</summary>
    public static Task Enter<T>(PlayerChoiceContext ctx, Creature c, CardModel? source = null) where T : PackmasterStance =>
        Change(ctx, c, typeof(T), async () => await Wiz.ApplyPower<T>(ctx, c, 1, c, source));

    /// <summary>StS1 ChangeStanceAction(new NeutralStance()).</summary>
    public static Task Exit(PlayerChoiceContext ctx, Creature c) => Change(ctx, c, null, null);

    private static async Task Change(PlayerChoiceContext ctx, Creature c, Type? target, Func<Task<PackmasterStance?>>? apply)
    {
        var old = Current(c);
        if (old?.GetType() == target) return;
        if (c.IsDead) return;

        if (old != null)
        {
            await old.OnExit(ctx);
            await PowerCmd.Remove(old);
        }
        if (apply != null && await apply() is { } entered) await entered.OnEnter(ctx);

        var mark = LastChange.GetOrCreateValue(c);
        mark.Combat = c.CombatState;
        mark.Turn = c.Player != null ? Wiz.TurnNumber(c.Player) : 0;

        foreach (var listener in c.Powers.OfType<IStanceListener>().ToList())
            await listener.AfterStanceChanged(ctx, old?.GetType(), target);
    }
}
