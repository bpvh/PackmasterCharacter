// Ported from src/main/java/thePackmaster/cards/prismaticpack/ShimmeringStrike.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Prismaticpack;

/// <summary>Deal 6 damage, +1 (+1) for each card of another color played this combat.</summary>
public sealed class ShimmeringStrike : AbstractPrismaticCard
{
    public ShimmeringStrike() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithCalculatedDamage(6, 1, (card, _) => card.IsCanonical ? 0 : Wiz.OtherColorPlayedThisCombat(card.Owner), bonusUpgrade: 1);
        WithTags(CardTag.Strike);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
    }
}
