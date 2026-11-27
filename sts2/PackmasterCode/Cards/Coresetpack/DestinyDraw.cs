// Ported from src/main/java/thePackmaster/cards/coresetpack/DestinyDraw.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Coresetpack;

/// <summary>Choose 1 of 3 random cards, each from a different Pack in your run. It costs 0 this turn. Exhaust. Upgrade: cost 0.</summary>
public sealed class DestinyDraw : PackmasterCard
{
    public DestinyDraw() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust);
        WithCostUpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var options = Wiz.RandomPacks(Owner, onlyCurrent: true, 3)
            .Select(pack => Wiz.RandomCardFromPack(pack, Owner))
            .Where(c => c != null)
            .Select(c => c!)
            .ToList();
        await Wiz.Discover(ctx, Owner, options, costsZeroThisTurn: true);
    }
}
