// Ported from src/main/java/thePackmaster/cards/strikepack/PenaltyStrike.java
using Packmaster.PackmasterCode.Powers.Strikepack;

namespace Packmaster.PackmasterCode.Cards.Strikepack;

/// <summary>2 cost. Deal 6 (+3) damage twice. The enemy loses 4 (+1) Strength this turn.</summary>
public sealed class PenaltyStrike : AbstractStrikePackCard
{
    public PenaltyStrike() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithVar("Magic", 4, 1);
        WithTags(CardTag.Strike);
        WithTip(typeof(PenaltyStrikePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, hitCount: 2, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        if (play.Target is { IsAlive: true })
            await CommonActions.Apply<PenaltyStrikePower>(ctx, play.Target, this, DynamicVars["Magic"].BaseValue);
    }
}
