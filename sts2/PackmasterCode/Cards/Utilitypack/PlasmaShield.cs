// Ported from src/main/java/thePackmaster/cards/utilitypack/PlasmaShield.java
using MegaCrit.Sts2.Core.Models.Orbs;
using Packmaster.PackmasterCode.Powers.Utilitypack;

namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>At the end of your turn, gain 2 (+2) Block if you have Plasma. Channel 1 Plasma.</summary>
public sealed class PlasmaShield : AbstractUtilityCard
{
    public PlasmaShield() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 2, 2);
        WithVar("Magic2", 1);
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(PlasmaOrb));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<PlasmaShieldPower>(ctx, this, DynamicVars["Magic"].BaseValue);
        for (int i = 0; i < DynamicVars["Magic2"].IntValue; i++) await OrbCmd.Channel<PlasmaOrb>(ctx, Owner);
    }
}
