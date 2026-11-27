// Ported from src/main/java/thePackmaster/cards/thieverypack/Magnet.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Powers.Thieverypack;

namespace Packmaster.PackmasterCode.Cards.Thieverypack;

/// <summary>
/// Steal all Artifact, Plated Armor (Plating), Thorns and Metallicize from the enemy (upgraded: from ALL enemies).
/// Exhaust. StS1 changed the upgraded card's target to all enemies; here you still pick one, and all are robbed.
/// </summary>
public sealed class Magnet : AbstractThieveryCard
{
    public Magnet() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(ArtifactPower));
        WithTip(typeof(PlatingPower));
        WithTip(typeof(ThornsPower));
    }

    protected override bool ShouldGlowGoldInternal =>
        CombatState?.HittableEnemies.Any(e => e.Powers.Any(p => Steal.MagnetPowers.Contains(p.GetType().Name))) == true;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        IEnumerable<Creature> from = IsUpgraded ? CombatState!.HittableEnemies : play.Target != null ? [play.Target] : [];
        await Steal.Powers(ctx, Owner.Creature, from, Steal.MagnetPowers);
    }
}
