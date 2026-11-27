// Ported from src/main/java/thePackmaster/cards/strikepack/LightningStrike.java
using MegaCrit.Sts2.Core.Models.Orbs;

namespace Packmaster.PackmasterCode.Cards.Strikepack;

/// <summary>2 cost. Deal 6 (+3) damage twice. Channel 2 Lightning.</summary>
public sealed class LightningStrike : AbstractStrikePackCard
{
    public LightningStrike() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithTags(CardTag.Strike);
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(LightningOrb));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, hitCount: 2).Execute(ctx);
        await OrbCmd.Channel<LightningOrb>(ctx, Owner);
        await OrbCmd.Channel<LightningOrb>(ctx, Owner);
    }
}
