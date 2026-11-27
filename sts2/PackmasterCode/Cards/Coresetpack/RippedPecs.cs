// Ported from src/main/java/thePackmaster/cards/coresetpack/RippedPecs.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Coresetpack;

/// <summary>Gain 1 Strength. Synergy: gain 3 (+1) Strength instead.</summary>
public sealed class RippedPecs : PackmasterCard
{
    public RippedPecs() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 3, 1);
        WithTip(typeof(StrengthPower));
    }

    protected override bool ShouldGlowGoldInternal => Wiz.HasSynergy(this);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var amount = Wiz.HasSynergy(this) ? DynamicVars["Magic"].BaseValue : 1m;
        await CommonActions.ApplySelf<StrengthPower>(ctx, this, amount);
    }
}
