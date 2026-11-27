// Ported from src/main/java/thePackmaster/cards/orbpack/Augment.java
namespace Packmaster.PackmasterCode.Cards.Orbpack;

/// <summary>Gain 7 Block. If you have no empty orb slots, gain an orb slot. Upgrade: Retain.</summary>
public sealed class Augment : AbstractOrbCard
{
    public Augment() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(7);
        WithKeyword(CardKeyword.Retain, UpgradeType.Add);
    }

    protected override bool ShouldGlowGoldInternal => Owner?.PlayerCombatState != null && EmptyOrbSlots == 0;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        if (EmptyOrbSlots == 0) await OrbCmd.AddSlots(Owner, 1);
    }
}
