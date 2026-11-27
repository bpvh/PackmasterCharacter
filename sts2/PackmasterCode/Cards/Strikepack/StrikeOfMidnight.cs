// Ported from src/main/java/thePackmaster/cards/strikepack/StrikeOfMidnight.java
using MegaCrit.Sts2.Core.Models.Orbs;

namespace Packmaster.PackmasterCode.Cards.Strikepack;

/// <summary>Deal 6 (+3) damage. Channel 1 Dark.</summary>
public sealed class StrikeOfMidnight : AbstractStrikePackCard
{
    public StrikeOfMidnight() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithTags(CardTag.Strike);
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(DarkOrb));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        await OrbCmd.Channel<DarkOrb>(ctx, Owner);
    }
}
