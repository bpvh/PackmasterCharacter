// Ported from src/main/java/thePackmaster/cards/utilitypack/GreaterHex.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>Token (from Lesser Hex). Apply 2 Weak, 2 Vulnerable and -2 Strength. Exhaust. Upgrade: costs 1.</summary>
public sealed class GreaterHex : PackmasterCard
{
    public GreaterHex() : base(2, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy)
    {
        WithVar("Magic", 2);
        WithKeyword(CardKeyword.Exhaust);
        WithCostUpgradeBy(-1);
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(StrengthPower));
    }

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => LesserHex.Hex(ctx, this, play, DynamicVars["Magic"].BaseValue);
}
