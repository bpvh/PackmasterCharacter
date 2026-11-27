// Ported from src/main/java/thePackmaster/cards/madsciencepack/Fortify.java
using Packmaster.PackmasterCode.CardModifiers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

/// <summary>Gain 7 (+3) Block. Modify a random Block card in hand: "Add 4 Block".</summary>
public sealed class Fortify : AbstractMadScienceCard
{
    public Fortify() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(7, 3);
        WithVar("Magic", 4);
        WithTip(PmTips.Modify);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await MadScience.Modify(ctx, this, Wiz.Hand(Owner), 1, random: true, c => c.DynamicVars.Keys.Contains("Block"),
            MadScience.New<AddBlockModifier>(DynamicVars["Magic"].IntValue), fromHand: true);
    }
}
