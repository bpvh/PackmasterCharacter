// Ported from src/main/java/thePackmaster/cards/strikepack/Backstrike.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Strikepack;

/// <summary>0 cost. Innate. Deal 6 (+3) damage. Attacks in hand count as Strikes this combat. Exhaust.</summary>
public sealed class Backstrike : AbstractStrikePackCard
{
    public Backstrike() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithKeyword(CardKeyword.Innate);
        WithKeyword(CardKeyword.Exhaust);
        WithTags(CardTag.Strike);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
        foreach (var card in Wiz.Hand(Owner).Where(c => c.Type == CardType.Attack))
            ExtraTags.Add(card, CardTag.Strike);
    }
}
