// Ported from src/main/java/thePackmaster/cards/utilitypack/LayeredDefenses.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>Gain 4 (+1) Block for each card type among the other cards in your hand.</summary>
public sealed class LayeredDefenses : AbstractUtilityCard
{
    public LayeredDefenses() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(4, 1);
    }

    private int CardTypes => Owner == null ? 0 : Wiz.Hand(Owner).Where(c => c != this).Select(c => c.Type).Distinct().Count();

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var n = CardTypes;
        for (int i = 0; i < n; i++) await CommonActions.CardBlock(this, play);
    }
}
