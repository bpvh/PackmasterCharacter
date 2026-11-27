// Ported from src/main/java/thePackmaster/cards/strikepack/StrikeOfGenius.java
using Packmaster.PackmasterCode.Powers.Strikepack;

namespace Packmaster.PackmasterCode.Cards.Strikepack;

/// <summary>Deal 6 (+3) damage. The next Attack you play gains "Gain Block equal to unblocked damage dealt".</summary>
public sealed class StrikeOfGenius : AbstractStrikePackCard
{
    public StrikeOfGenius() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithTags(CardTag.Strike);
        WithTip(typeof(StrikeOfGeniusPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play).Execute(ctx);
        await CommonActions.ApplySelf<StrikeOfGeniusPower>(ctx, this, 1);
    }
}
