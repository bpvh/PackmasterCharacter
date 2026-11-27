// Ported from src/main/java/thePackmaster/cards/strikepack/StrikeABargain.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Strikepack;

/// <summary>2 cost. Deal 6 (+3) damage. Attacks in your hand cost 0 until played. Exhaust (not when upgraded).</summary>
public sealed class StrikeABargain : AbstractStrikePackCard
{
    public StrikeABargain() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithKeyword(CardKeyword.Exhaust, UpgradeType.Remove);
        WithTags(CardTag.Strike);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        foreach (var card in Wiz.Hand(Owner).Where(c => c.Type == CardType.Attack && !c.EnergyCost.CostsX && c.EnergyCost.GetResolved() > 0))
            card.EnergyCost.SetUntilPlayed(0);
    }
}
