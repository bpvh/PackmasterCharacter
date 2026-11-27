// Ported from src/main/java/thePackmaster/cards/evenoddpack/Flourish.java
using MegaCrit.Sts2.Core.Models.Powers;

using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>Power. Gain 3 (+1) Plating. Even: heal 3 (+1) HP. Odd: gain [E].</summary>
public sealed class Flourish : AbstractEvenOddCard
{
    public Flourish() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 3, 1);
        WithTip(typeof(PlatingPower));
        WithEnergyTip();
        WithTip(PmTips.Even);
        WithTip(PmTips.Odd);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<PlatingPower>(ctx, this, DynamicVars["Magic"].BaseValue);
        if (IsEven(play)) await CreatureCmd.Heal(Owner.Creature, DynamicVars["Magic"].BaseValue);
        else await PlayerCmd.GainEnergy(1, Owner);
    }
}
