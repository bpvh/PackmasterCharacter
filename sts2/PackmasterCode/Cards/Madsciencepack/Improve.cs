// Ported from src/main/java/thePackmaster/cards/madsciencepack/Improve.java
using Packmaster.PackmasterCode.CardModifiers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

/// <summary>Draw 2 cards. Modify a random (upgraded: chosen) card in hand: "Draw 1 card". Exhaust.</summary>
public sealed class Improve : AbstractMadScienceCard
{
    public Improve() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 2);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(PmTips.Modify);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
        await MadScience.Modify(ctx, this, Wiz.Hand(Owner), 1, random: !IsUpgraded, MadScience.Playable,
            MadScience.New<DrawCardModifier>(1), fromHand: true);
    }
}
