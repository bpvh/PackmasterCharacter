// Ported from src/main/java/thePackmaster/cards/arcanapack/TheSun.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Arcanapack;

/// <summary>3 cost. Deal 22 (+8) damage to ALL enemies. Apply 99 Vulnerable to the enemy with the most HP. Exhaust.</summary>
public sealed class TheSun : AbstractAstrologerCard
{
    public TheSun() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
        WithDamage(22, 8);
        WithVar("Magic", 99);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(VulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play).Execute(ctx);
        var strongest = CombatState?.HittableEnemies.Where(e => e.IsAlive).OrderByDescending(e => e.CurrentHp).FirstOrDefault();
        if (strongest != null)
            await CommonActions.Apply<VulnerablePower>(ctx, strongest, this, DynamicVars["Magic"].BaseValue);
    }
}
