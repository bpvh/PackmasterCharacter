// Ported from src/main/java/thePackmaster/cards/evenoddpack/QuickReflex.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>Gain 6 (+2) Block. First: gain it again.</summary>
public sealed class QuickReflex : AbstractEvenOddCard
{
    public QuickReflex() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(6, 2);
        WithTip(Keywords.First);
    }

    protected override bool ShouldGlowGoldInternal => IsFirst();

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var first = IsFirst(play);
        await CommonActions.CardBlock(this, play);
        if (first) await CommonActions.CardBlock(this, play);
    }
}
