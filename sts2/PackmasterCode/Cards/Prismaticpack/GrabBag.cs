// Ported from src/main/java/thePackmaster/cards/prismaticpack/GrabBag.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Prismaticpack;

/// <summary>Add 4 random cards of other colors to your hand. Exhaust. Upgrade: cost 0.</summary>
public sealed class GrabBag : AbstractPrismaticCard
{
    public GrabBag() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithVar("Magic", 4);
        WithKeyword(CardKeyword.Exhaust);
        WithCostUpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        foreach (var model in Wiz.RandomOtherColorCards(Owner, null, null, DynamicVars["Magic"].IntValue))
            await Wiz.MakeInHand(Wiz.CreateCard(model, Owner), Owner);
    }
}
