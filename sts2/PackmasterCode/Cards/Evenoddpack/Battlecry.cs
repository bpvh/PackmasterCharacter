// Ported from src/main/java/thePackmaster/cards/evenoddpack/Battlecry.java
using MegaCrit.Sts2.Core.Models.Powers;

using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>Draw 2 (+1) cards. First: gain 2 Strength.</summary>
public sealed class Battlecry : AbstractEvenOddCard
{
    public Battlecry() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 2, 1);
        WithTip(typeof(StrengthPower));
        WithTip(Keywords.First);
    }

    protected override bool ShouldGlowGoldInternal => IsFirst();

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (IsFirst(play)) await CommonActions.ApplySelf<StrengthPower>(ctx, this, 2);
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
    }
}
