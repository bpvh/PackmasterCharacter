// Ported from src/main/java/thePackmaster/cards/madsciencepack/Expand.java
using Packmaster.PackmasterCode.CardModifiers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

/// <summary>Modify a card in hand: "Costs 1 more" and "Play again". Exhaust. Upgrade: costs 0.</summary>
public sealed class Expand : AbstractMadScienceCard
{
    public Expand() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithKeyword(CardKeyword.Exhaust);
        WithCostUpgradeBy(-1);
        WithTip(PmTips.Modify);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MadScience.Modify(ctx, this, Wiz.Hand(Owner), 1, random: false, _ => true, MadScience.New<ExpandModifier>(), fromHand: true);
    }
}
