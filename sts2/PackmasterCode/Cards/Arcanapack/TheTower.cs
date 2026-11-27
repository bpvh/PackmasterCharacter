// Ported from src/main/java/thePackmaster/cards/arcanapack/TheTower.java (+ actions/arcanapack/TheTowerAction.java)
namespace Packmaster.PackmasterCode.Cards.Arcanapack;

/// <summary>2 cost. Deal 18 (+5) damage. If this kills the enemy, play a copy of this card (it then disappears).</summary>
public sealed class TheTower : AbstractAstrologerCard
{
    public TheTower() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(18, 5);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var target = play.Target;
        await CommonActions.CardAttack(this, play).Execute(ctx);
        if (target == null || target.IsAlive || CombatState?.HittableEnemies.Any() != true) return;

        // StS1 queued a purge-on-use copy at a random enemy. VERIFY: AutoPlay with a null target picks one, and
        // RemoveFromCombat afterwards keeps the copy out of the discard pile.
        var copy = CreateClone();
        await CardCmd.AutoPlay(ctx, copy, null);
        await CardPileCmd.RemoveFromCombat(copy);
    }
}
