// Ported from src/main/java/thePackmaster/cards/orbpack/Allocate.java
namespace Packmaster.PackmasterCode.Cards.Orbpack;

/// <summary>Gain an orb slot for each enemy. Upgrade: Innate.</summary>
public sealed class Allocate : AbstractOrbCard
{
    public Allocate() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithKeyword(CardKeyword.Innate, UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var enemies = CombatState?.HittableEnemies.Count ?? 0;
        if (enemies > 0) await OrbCmd.AddSlots(Owner, enemies);
    }
}
