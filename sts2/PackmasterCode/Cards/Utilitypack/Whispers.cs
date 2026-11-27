// Ported from src/main/java/thePackmaster/cards/utilitypack/Whispers.java
using MegaCrit.Sts2.Core.Models.Cards;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>Draw 3 (+1) cards. Add a Dazed to your hand.</summary>
public sealed class Whispers : AbstractUtilityCard
{
    public Whispers() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 3, 1);
        WithTip(typeof(Dazed));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
        await Wiz.MakeInHand(Wiz.CreateCard<Dazed>(Owner), Owner);
    }
}
