// Ported from src/main/java/thePackmaster/cards/bellordpack/RingTheBell.java
using MegaCrit.Sts2.Core.Models.Cards;

namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>ALL enemies lose 13 (+4) HP. Draw 1 card. When obtained, add a Curse of the Bell to your deck.</summary>
public sealed class RingTheBell : AbstractBellordCard
{
    public RingTheBell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithVar("Magic", 13, 4);
        WithTip(typeof(CurseOfTheBell));
    }

    protected override int CursesOnObtain => 1;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (CombatState != null)
            await CreatureCmd.Damage(ctx, CombatState.HittableEnemies.ToList(), DynamicVars["Magic"].BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
        await CardPileCmd.Draw(ctx, 1, Owner);
    }
}
