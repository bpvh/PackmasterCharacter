// Ported from src/main/java/thePackmaster/cards/utilitypack/LesserHex.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>Apply 1 Weak, 1 Vulnerable and -1 Strength. Add a Greater Hex (+) to your discard pile. Exhaust.</summary>
public sealed class LesserHex : AbstractUtilityCard
{
    public LesserHex() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithVar("Magic", 1);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(StrengthPower));
        WithTip(typeof(GreaterHex));
    }

    public static async Task Hex(PlayerChoiceContext ctx, CardModel card, CardPlay play, decimal amount)
    {
        if (play.Target == null) return;
        await CommonActions.Apply<WeakPower>(ctx, play.Target, card, amount);
        await CommonActions.Apply<VulnerablePower>(ctx, play.Target, card, amount);
        await CommonActions.Apply<StrengthPower>(ctx, play.Target, card, -amount);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hex(ctx, this, play, DynamicVars["Magic"].BaseValue);
        var greater = Wiz.CreateCard<GreaterHex>(Owner);
        if (IsUpgraded) Wiz.ForceUpgrade(greater);
        await Wiz.MakeInDiscard(greater, Owner);
    }
}
