// Ported from src/main/java/thePackmaster/cards/upgradespack/QuickFix.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Upgradespack;

/// <summary>0 cost. Gain 3 (+3) Block. Upgrade a random card in your hand.</summary>
public sealed class QuickFix : AbstractBlacksmithCard
{
    public QuickFix() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(3, 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        Wiz.UpgradeRandom(Owner, Wiz.Hand(Owner), 1);
    }
}
