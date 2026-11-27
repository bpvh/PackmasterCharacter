// Ported from src/main/java/thePackmaster/cards/orbpack/Charge.java
namespace Packmaster.PackmasterCode.Cards.Orbpack;

/// <summary>Deal 8 (+4) damage. Trigger the passive of all your orbs.</summary>
public sealed class Charge : AbstractOrbCard
{
    public Charge() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(8, 4);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_fire").Execute(ctx);
        // StS1 ran start- and end-of-turn orb effects for vanilla orbs; StS2 orbs have one Passive.
        foreach (var orb in Owner.PlayerCombatState!.OrbQueue.Orbs.ToList())
            await OrbCmd.Passive(ctx, orb, null);
    }
}
