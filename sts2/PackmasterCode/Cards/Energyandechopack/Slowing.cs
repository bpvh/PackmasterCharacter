// Ported from src/main/java/thePackmaster/cards/energyandechopack/Slowing.java
using MegaCrit.Sts2.Core.Models.Cards;
using Packmaster.PackmasterCode.Powers.Energyandechopack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

/// <summary>At the start of your turn, gain 1 energy. Add 2 (upgraded: 1) Wounds to your hand.</summary>
public sealed class Slowing : AbstractEchoCard
{
    public Slowing() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 2, -1);
        WithTip(typeof(Wound));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<SlowingPower>(ctx, this, 1);
        for (int i = 0; i < DynamicVars["Magic"].IntValue; i++) await Wiz.MakeInHand(Wiz.CreateCard<Wound>(Owner), Owner);
    }
}
