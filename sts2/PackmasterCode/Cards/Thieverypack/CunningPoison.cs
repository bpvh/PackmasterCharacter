// Ported from src/main/java/thePackmaster/cards/thieverypack/CunningPoison.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Powers.Thieverypack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Thieverypack;

/// <summary>Apply 4 (+2) Poison to the enemy with the most HP. If it dies while Poisoned, steal 12 (+3) Gold. Exhaust.</summary>
public sealed class CunningPoison : AbstractThieveryCard
{
    public CunningPoison() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithVar("Magic", 4, 2);
        WithVar("Magic2", 12, 3);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(PoisonPower));
        WithTip(typeof(DeceivedPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var target = CombatState?.HittableEnemies.MaxBy(c => c.CurrentHp);
        if (target == null) return;
        await CommonActions.Apply<PoisonPower>(ctx, target, this, DynamicVars["Magic"].BaseValue);
        var deceived = await Wiz.ApplyPower<DeceivedPower>(ctx, target, DynamicVars["Magic2"].BaseValue, Owner.Creature, this);
        if (deceived != null) deceived.Thief = Owner;
    }
}
