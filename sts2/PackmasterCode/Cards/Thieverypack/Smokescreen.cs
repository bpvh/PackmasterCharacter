// Ported from src/main/java/thePackmaster/cards/thieverypack/Smokescreen.java
namespace Packmaster.PackmasterCode.Cards.Thieverypack;

/// <summary>EVERYONE gains 5 (+2) Block. ALL enemies lose 7 (+2) HP.</summary>
public sealed class Smokescreen : AbstractThieveryCard
{
    public Smokescreen() : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
    {
        WithBlock(5, 2);
        WithVar("Magic", 7, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        var enemies = CombatState!.HittableEnemies.ToList();
        foreach (var e in enemies) await CreatureCmd.GainBlock(e, DynamicVars.Block.BaseValue, ValueProp.Unpowered, null);
        // HP loss goes through Block (StS1 LoseHPAction).
        await CreatureCmd.Damage(ctx, enemies, DynamicVars["Magic"].BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
    }
}
