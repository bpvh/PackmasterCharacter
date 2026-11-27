// Ported from src/main/java/thePackmaster/cards/arcanapack/Death.java (+ actions/arcanapack/KillEnemyAction.java)
namespace Packmaster.PackmasterCode.Cards.Arcanapack;

/// <summary>0 cost. Deal 6 (+3) damage. If the enemy is left at or below 10% of its max HP, it dies.</summary>
public sealed class Death : AbstractAstrologerCard
{
    public Death() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var target = play.Target;
        if (target == null) return;
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        if (target.IsAlive && target.CurrentHp <= target.MaxHp / 10)
            await CreatureCmd.Kill(target);
    }
}
