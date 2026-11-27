// Ported from src/main/java/thePackmaster/cards/aggressionpack/Animosity.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Powers.Aggressionpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

/// <summary>When you enter, exit, or start your turn in Aggression, gain 1 (+1) Strength.</summary>
public sealed class Animosity : AbstractAggressionCard
{
    public Animosity() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 1, 1);
        WithTip(PmTips.Aggression);
        WithTip(typeof(StrengthPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<AnimosityPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
