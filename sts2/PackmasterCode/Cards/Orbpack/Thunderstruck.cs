// Ported from src/main/java/thePackmaster/cards/orbpack/Thunderstruck.java
using MegaCrit.Sts2.Core.Models.Orbs;
using Packmaster.PackmasterCode.Powers.Orbpack;

namespace Packmaster.PackmasterCode.Cards.Orbpack;

/// <summary>Whenever you play a Strike, Channel 1 (+1) Lightning.</summary>
public sealed class Thunderstruck : AbstractOrbCard
{
    public Thunderstruck() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 1, 1);
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(LightningOrb));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<ThunderstruckPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
