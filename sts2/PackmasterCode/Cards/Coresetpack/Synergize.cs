// Ported from src/main/java/thePackmaster/cards/coresetpack/Synergize.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Coresetpack;

/// <summary>Deal 7 (+2) damage to ALL enemies. Synergy: gain 7 (+2) Block.</summary>
public sealed class Synergize : PackmasterCard
{
    public Synergize() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        WithDamage(7, 2);
        WithBlock(7, 2);
    }

    protected override bool ShouldGlowGoldInternal => Wiz.HasSynergy(this);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var synergy = Wiz.HasSynergy(this);
        await CommonActions.CardAttack(this, play).Execute(ctx);
        if (synergy) await CommonActions.CardBlock(this, play);
    }
}
