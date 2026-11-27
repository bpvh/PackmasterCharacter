// Ported from src/main/java/thePackmaster/cards/utilitypack/PreemptiveStrike.java
namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>Deal 6 (+2) damage. If the enemy intends to attack, deal it again.</summary>
public sealed class PreemptiveStrike : AbstractUtilityCard
{
    public PreemptiveStrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(6, 2);
        WithTags(CardTag.Strike);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // Intent is read before the first hit, as StS1 queued both hits at once.
        var hits = play.Target?.Monster?.IntendsToAttack == true ? 2 : 1;
        await CommonActions.CardAttack(this, play, hitCount: hits, vfx: "vfx/vfx_attack_slash").Execute(ctx);
    }
}
