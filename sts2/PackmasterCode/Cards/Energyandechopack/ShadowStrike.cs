// Ported from src/main/java/thePackmaster/cards/energyandechopack/ShadowStrike.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

/// <summary>Deal 8 (+2) damage. Echo.</summary>
public sealed class ShadowStrike : AbstractEchoCard
{
    public ShadowStrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(8, 2);
        WithTip(PmTips.Echo);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play).Execute(ctx);
        await Echo();
    }
}
