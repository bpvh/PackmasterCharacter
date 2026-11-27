// Ported from src/main/java/thePackmaster/cards/prismaticpack/PremiumSelection.java (+ actions/prismaticpack/PremiumSelectionAction.java)
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Prismaticpack;

/// <summary>Choose 1 of 3 (+1) different random Rare cards of other colors to add to your hand. It costs 0 this turn. Exhaust.</summary>
public sealed class PremiumSelection : AbstractPrismaticCard
{
    public PremiumSelection() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 3, 1);
        WithKeyword(CardKeyword.Exhaust);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var count = DynamicVars["Magic"].IntValue;
        var options = new List<CardModel>();
        // Distinct cards; bounded so a tiny pool can't loop forever.
        for (int tries = 0; options.Count < count && tries < count * 20; tries++)
        {
            var card = Wiz.RandomOtherColorCards(Owner, null, CardRarity.Rare, 1).FirstOrDefault();
            if (card != null && options.All(o => o.Id != card.Id)) options.Add(card);
        }
        await Wiz.Discover(ctx, Owner, options, costsZeroThisTurn: true, canSkip: true);
    }
}
