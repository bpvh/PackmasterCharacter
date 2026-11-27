// Ported from src/main/java/thePackmaster/cards/madsciencepack/Discombobulate.java
using Packmaster.PackmasterCode.CardModifiers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

/// <summary>Deal 7 (+3) damage to ALL enemies. If you have a Modified card in hand, deal it twice.</summary>
public sealed class Discombobulate : AbstractMadScienceCard
{
    public Discombobulate() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithDamage(7, 3);
        WithTip(PmTips.Modify);
    }

    private bool ModifiedInHand => Owner != null && Wiz.Hand(Owner).Any(c => c != this && !MadScience.Unmodified(c));

    protected override bool ShouldGlowGoldInternal => ModifiedInHand;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var hits = ModifiedInHand ? 2 : 1;
        await CommonActions.CardAttack(this, play, hitCount: hits, vfx: "vfx/vfx_attack_blunt").Execute(ctx);
    }
}
