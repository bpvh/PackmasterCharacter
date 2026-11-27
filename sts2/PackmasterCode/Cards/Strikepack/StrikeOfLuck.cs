// Ported from src/main/java/thePackmaster/cards/strikepack/StrikeOfLuck.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Strikepack;

/// <summary>Deal 6 (+3) damage to a random enemy 1 to 3 times.</summary>
public sealed class StrikeOfLuck : AbstractStrikePackCard
{
    public StrikeOfLuck() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
    {
        WithDamage(6, 3);
        WithTags(CardTag.Strike);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var hits = Wiz.CardRng(Owner).NextInt(1, 4); // 1..3
        await CommonActions.CardAttack(this, play, hitCount: hits, vfx: "vfx/vfx_attack_slash").Execute(ctx);
    }
}
