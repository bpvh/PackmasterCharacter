// Ported from src/main/java/thePackmaster/cards/thieverypack/Witchcraft.java and actions/thieverypack/WitchcraftAction.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Potions;
using Packmaster.PackmasterCode.Powers.Thieverypack;

namespace Packmaster.PackmasterCode.Cards.Thieverypack;

/// <summary>Steal 7 (+2) HP from the enemy. If this kills it (not a minion), obtain a Divine Potion. Exhaust.</summary>
public sealed class Witchcraft : AbstractThieveryCard
{
    public Witchcraft() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithVar("Magic", 7, 2);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(DivinePotion));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (play.Target is not { IsAlive: true } target) return;
        var minion = target.HasPower<MinionPower>();
        var results = (await CreatureCmd.Damage(ctx, target, DynamicVars["Magic"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered, this)).ToList();
        var lost = results.Sum(r => r.UnblockedDamage);
        if (lost > 0) await CreatureCmd.Heal(Owner.Creature, lost * Steal.Multiplier(Owner.Creature));
        if (!minion && results.Any(r => r.WasTargetKilled)) await PotionCmd.TryToProcure<DivinePotion>(Owner);
    }
}
