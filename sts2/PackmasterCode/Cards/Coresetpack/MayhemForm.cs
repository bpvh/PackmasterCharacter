// Ported from src/main/java/thePackmaster/cards/coresetpack/MayhemForm.java
using Packmaster.PackmasterCode.Powers.Coresetpack;

namespace Packmaster.PackmasterCode.Cards.Coresetpack;

/// <summary>Power, 3 cost. Once per turn, when you have 2 or fewer cards in hand, draw 2 cards and gain 1 (+1) energy.</summary>
public sealed class MayhemForm : PackmasterCard
{
    public MayhemForm() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 1, 1);
        WithTip(typeof(MayhemFormPower));
        WithEnergyTip();
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<MayhemFormPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
