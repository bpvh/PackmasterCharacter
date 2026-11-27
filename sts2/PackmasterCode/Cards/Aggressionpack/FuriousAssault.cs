// Ported from src/main/java/thePackmaster/cards/aggressionpack/FuriousAssault.java
using Packmaster.PackmasterCode.Stances;
using Packmaster.PackmasterCode.Stances.Aggressionpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

/// <summary>Deal 2 damage 3 (+1) times. Enter Aggression.</summary>
public sealed class FuriousAssault : AbstractAggressionCard
{
    public FuriousAssault() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(2);
        WithVar("Magic", 3, 1);
        WithTip(PmTips.Aggression);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, hitCount: DynamicVars["Magic"].IntValue, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        await Stance.Enter<Aggression>(ctx, Owner.Creature, this);
    }
}
