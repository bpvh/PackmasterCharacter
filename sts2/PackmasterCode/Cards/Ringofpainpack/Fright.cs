// Ported from src/main/java/thePackmaster/cards/ringofpainpack/Fright.java
namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>Deal 9 (+2) damage. If the enemy intends to attack, draw 2 (+1) cards.</summary>
public sealed class Fright : AbstractRingOfPainCard
{
    public Fright() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(9, 2);
        WithVar("Magic", 2, 1);
    }

    protected override bool ShouldGlowGoldInternal => CombatState?.HittableEnemies.Any(e => e.Monster?.IntendsToAttack == true) == true;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var attacking = play.Target?.Monster?.IntendsToAttack == true;
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_blunt").Execute(ctx);
        if (attacking) await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
    }
}
