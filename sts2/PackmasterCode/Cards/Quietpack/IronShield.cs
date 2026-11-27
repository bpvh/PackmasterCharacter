// Ported from src/main/java/thePackmaster/cards/quietpack/IronShield.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>Gain 11 (+4) Block. Add a Weight to your hand.</summary>
public sealed class IronShield : AbstractQuietCard
{
    public IronShield() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(11, 4);
        WithTip(typeof(Weight));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await Wiz.MakeInHand(Wiz.CreateCard<Weight>(Owner), Owner);
    }
}
