// Ported from src/main/java/thePackmaster/actions/thieverypack/StealPowerAction.java and StealAllBlockAction.java
using System.Reflection;
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Thieverypack;

/// <summary>Stealing (Thievery pack). With Thievery Mastery the thief gains twice what was stolen.</summary>
public static class Steal
{
    /// <summary>
    /// Powers the player can take over (StS1 getDuplicatedPower). Matched by type name so powers missing from some
    /// game builds (Metallicize) still work when present. Others can't be stolen.
    /// </summary>
    public static readonly HashSet<string> Stealable =
        ["StrengthPower", "ArtifactPower", "PlatingPower", "ThornsPower", "MetallicizePower", "IntangiblePower", "RitualPower"];

    /// <summary>Magnet's list: Artifact, Plated Armor (StS2 Plating), Thorns, Metallicize.</summary>
    public static readonly HashSet<string> MagnetPowers = ["ArtifactPower", "PlatingPower", "ThornsPower", "MetallicizePower"];

    public static bool CanSteal(PowerModel p) => Stealable.Contains(p.GetType().Name) && p.Amount > 0;

    public static int Multiplier(Creature thief)
    {
        if (thief.GetPower<ThieveryMasteryPower>() is not { } mastery) return 1;
        mastery.Notify();
        return 2;
    }

    private static readonly MethodInfo ApplyGeneric = typeof(Wiz).GetMethod(nameof(Wiz.ApplyPower))!;

    /// <summary>Take up to <paramref name="max"/> (0 = all) of <paramref name="power"/> from its owner.</summary>
    public static async Task Power(PlayerChoiceContext ctx, Creature thief, PowerModel power, decimal max = 0)
    {
        if (!CanSteal(power)) return;
        var amount = max <= 0 ? power.Amount : Math.Min(power.Amount, max);
        if (amount >= power.Amount) await PowerCmd.Remove(power);
        else await Wiz.ModifyPowerAmount(power, -amount);
        var gain = amount * Multiplier(thief);
        var apply = ApplyGeneric.MakeGenericMethod(power.GetType());
        await (Task)apply.Invoke(null, [ctx, thief, gain, thief, null])!;
    }

    /// <summary>Steal every power in <paramref name="types"/> (by type name) from the creatures.</summary>
    public static async Task Powers(PlayerChoiceContext ctx, Creature thief, IEnumerable<Creature> from, ISet<string> types)
    {
        foreach (var c in from.ToList())
        foreach (var p in c.Powers.Where(p => types.Contains(p.GetType().Name)).ToList())
            await Power(ctx, thief, p);
    }

    /// <summary>StS1 StealAllBlockAction: the target loses all Block and the thief gains it.</summary>
    public static async Task AllBlock(Creature thief, Creature target)
    {
        var block = target.Block;
        if (block <= 0 || target.IsDead) return;
        await CreatureCmd.LoseBlock(target, block);
        await CreatureCmd.GainBlock(thief, block * Multiplier(thief), ValueProp.Unpowered, null);
    }
}
