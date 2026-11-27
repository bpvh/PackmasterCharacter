// Ported from src/main/java/thePackmaster/cards/upgradespack/MastersStrike.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Upgradespack;

/// <summary>2 cost. Deal 10 (+5) damage, +1 for each upgrade on cards in your hand, draw and discard piles.</summary>
public sealed class MastersStrike : AbstractBlacksmithCard
{
    public MastersStrike() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithCalculatedDamage(10, 1, (card, _) => card.IsCanonical ? 0 : Wiz.UpgradesInPiles(card.Owner), upgrade: 5);
        WithTags(CardTag.Strike);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
    }
}
