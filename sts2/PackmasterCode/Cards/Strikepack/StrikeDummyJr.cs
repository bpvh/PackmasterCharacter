// Ported from src/main/java/thePackmaster/cards/strikepack/StrikeDummyJr.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Powers.Strikepack;

namespace Packmaster.PackmasterCode.Cards.Strikepack;

/// <summary>Deal 6 (+3) damage. Gain 1 (+1) Strength. Strikes deal 2 more damage this combat. Exhaust.</summary>
public sealed class StrikeDummyJr : AbstractStrikePackCard
{
    private const int StrikeBonus = 2;

    public StrikeDummyJr() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithVar("Magic", 1, 1);
        WithKeyword(CardKeyword.Exhaust);
        WithTags(CardTag.Strike);
        WithTip(typeof(StrengthPower));
        WithTip(typeof(StrikeDummyJrPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        await CommonActions.ApplySelf<StrengthPower>(ctx, this, DynamicVars["Magic"].BaseValue);
        await CommonActions.ApplySelf<StrikeDummyJrPower>(ctx, this, StrikeBonus);
    }
}
