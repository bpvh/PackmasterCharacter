// Ported from src/main/java/thePackmaster/cards/orbpack/Fear.java
using MegaCrit.Sts2.Core.Models.Orbs;

namespace Packmaster.PackmasterCode.Cards.Orbpack;

/// <summary>Gain 14 Block. Channel 1 (+1) Dark.</summary>
public sealed class Fear : AbstractOrbCard
{
    public Fear() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithBlock(14);
        WithVar("Magic", 1, 1);
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(DarkOrb));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        for (int i = 0; i < DynamicVars["Magic"].IntValue; i++) await OrbCmd.Channel<DarkOrb>(ctx, Owner);
    }
}
