// Ported from src/main/java/thePackmaster/cards/evenoddpack/ElementOfSurprise.java
using Packmaster.PackmasterCode.Powers;

using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>1 (0) cost. First: your next card this turn is played twice.</summary>
public sealed class ElementOfSurprise : AbstractEvenOddCard
{
    public ElementOfSurprise() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithTip(typeof(DuplicationPower));
        WithTip(Keywords.First);
    }

    protected override bool ShouldGlowGoldInternal => IsFirst();

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (IsFirst(play)) await CommonActions.ApplySelf<DuplicationPower>(ctx, this, 1);
    }
}
