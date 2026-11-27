// Ported from src/main/java/thePackmaster/cards/utilitypack/RareStrike.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>Deal 6 damage, plus 2 (+2) for each Rare card in your hand, draw pile and discard pile.</summary>
public sealed class RareStrike : AbstractUtilityCard
{
    public RareStrike() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithCalculatedDamage(6, 2, (card, _) => card.IsCanonical ? 0 : CountRares(card.Owner), bonusUpgrade: 2);
        WithTags(CardTag.Strike);
    }

    private static int CountRares(Player p) =>
        Wiz.Hand(p).Concat(Wiz.DrawPile(p)).Concat(Wiz.DiscardPile(p)).Count(c => c.Rarity == CardRarity.Rare);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
    }
}
