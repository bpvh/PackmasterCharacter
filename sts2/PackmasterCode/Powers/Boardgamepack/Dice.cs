// Ported from src/main/java/thePackmaster/actions/boardgamepack/RollAction.java (and DelayedDiceRollAction)
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Boardgamepack;

/// <summary>Cards that don't use up Dice when played (StS1 AbstractBoardCard.reroll).</summary>
public interface IRerollCard;

/// <summary>Dice rolls (Board Game pack; also used by Gems' Tourmaline). The total becomes <see cref="DicePower"/>.</summary>
public static class Dice
{
    /// <summary>
    /// StS1 RollAction: roll <paramref name="count"/> dice with <paramref name="sides"/> sides, each rolled
    /// 1 + Advantage times keeping the highest (One-Time Advantage is used up), and gain Dice equal to the sum.
    /// </summary>
    public static async Task<int> Roll(PlayerChoiceContext ctx, Player player, int sides, int count = 1, int modifier = 0)
    {
        var creature = player.Creature;
        var advantage = 0;
        if (creature.GetPower<OneTimeAdvantagePower>() is { } once)
        {
            advantage += once.Amount;
            await PowerCmd.Remove(once);
        }
        if (creature.GetPower<AdvantagePower>() is { } adv) advantage += adv.Amount;

        var rng = Wiz.CardRng(player);
        var sum = modifier;
        for (int d = 0; d < count; d++)
        {
            var best = 0;
            for (int i = 0; i <= advantage; i++) best = Math.Max(best, rng.NextInt(1, sides + 1));
            sum += best;
        }
        if (sum > 0) await Wiz.ApplyPower<DicePower>(ctx, creature, sum, creature);
        return sum;
    }
}
