// Ported from src/main/java/thePackmaster/cards/quietpack/HammerThrow.java
using Packmaster.PackmasterCode.Powers.Quietpack;

namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>Power. At the end of your turn, deal 3 (+2) damage to ALL enemies for each Retained card in your hand.</summary>
public sealed class HammerThrow : AbstractQuietCard
{
    public HammerThrow() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 3, 2);
        WithTip(typeof(HammerThrowPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<HammerThrowPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
