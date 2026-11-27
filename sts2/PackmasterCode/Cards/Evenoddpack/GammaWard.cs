// Ported from src/main/java/thePackmaster/cards/evenoddpack/GammaWard.java
using Packmaster.PackmasterCode.Powers.Evenoddpack;

using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>Gain 4 (+2) Block. Even: your next Skill this turn gains "Even: gain 12 (+4) Block".</summary>
public sealed class GammaWard : AbstractEvenOddCard
{
    public GammaWard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithBlock(4, 2);
        WithVar("Magic", 12, 4);
        WithTip(typeof(GammaWardPower));
        WithTip(Keywords.Even);
    }

    protected override bool ShouldGlowGoldInternal => IsEven();

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        if (IsEven(play)) await CommonActions.ApplySelf<GammaWardPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
