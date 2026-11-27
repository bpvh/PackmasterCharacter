// Ported from src/main/java/thePackmaster/cards/evenoddpack/WindUpBomb.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>Even: increase this card's damage by 15 (+5) this combat. Odd: deal 8 damage.</summary>
public sealed class WindUpBomb : AbstractEvenOddCard
{
    public WindUpBomb() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(8);
        WithVar("Magic", 15, 5);
        WithTip(PmTips.Even);
        WithTip(PmTips.Odd);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (IsOdd(play)) await CommonActions.CardAttack(this, play).Execute(ctx);
        else DynamicVars.Damage.BaseValue += DynamicVars["Magic"].BaseValue; // StS1 ModifyDamageAction: this combat copy only
    }
}
