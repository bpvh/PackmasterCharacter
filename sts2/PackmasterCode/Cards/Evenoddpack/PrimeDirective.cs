// Ported from src/main/java/thePackmaster/cards/evenoddpack/PrimeDirective.java
using Packmaster.PackmasterCode.Powers.Evenoddpack;

using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>Power. Your Attacks have "Odd: deal 4 (+1) more damage".</summary>
public sealed class PrimeDirective : AbstractEvenOddCard
{
    public PrimeDirective() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 4, 1);
        WithTip(typeof(PrimeDirectivePower));
        WithTip(PmTips.Odd);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<PrimeDirectivePower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
