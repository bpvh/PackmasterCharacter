// Ported from src/main/java/thePackmaster/cards/upgradespack/Masterwork.java (+ actions/upgradespack/SuperUpgradeAction.java)
using BaseLib.Cards.Variables;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Upgradespack;

/// <summary>Choose a card in your hand. Upgrade it 2 (+1) times, even past its usual limit. Exhaustive 3.</summary>
public sealed class Masterwork : AbstractBlacksmithCard
{
    public Masterwork() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 2, 1);
        WithVar(new ExhaustiveVar(3));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var card = await CommonActions.SelectSingleCard(this, CardSelectorPrefs.UpgradeSelectionPrompt, ctx, PileType.Hand);
        if (card == null) return;
        for (int i = 0; i < DynamicVars["Magic"].IntValue; i++) Wiz.ForceUpgrade(card);
    }
}
