// Ported from src/main/java/thePackmaster/cards/madsciencepack/Winterize.java
using MegaCrit.Sts2.Core.Models.Orbs;
using Packmaster.PackmasterCode.CardModifiers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

/// <summary>Channel 3 Frost. Modify 2 (+1) random cards in your draw pile: "Channel 1 Frost". Exhaust.</summary>
public sealed class Winterize : AbstractMadScienceCard
{
    public const int Frost = 3;

    public Winterize() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithVar("Magic", 2, 1);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(FrostOrb));
        WithTip(PmTips.Modify);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        for (int i = 0; i < Frost; i++) await OrbCmd.Channel<FrostOrb>(ctx, Owner);
        await MadScience.Modify(ctx, this, Wiz.DrawPile(Owner), DynamicVars["Magic"].IntValue, random: true, _ => true,
            MadScience.New<FrostOrbModifier>(1), fromHand: false);
    }
}
