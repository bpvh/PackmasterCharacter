// Ported from src/main/java/thePackmaster/cards/upgradespack/SetUpShop.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Upgradespack;

/// <summary>Choose up to 2 (+1) cards from your draw and discard piles. Upgrade them and put them on top of your draw pile.</summary>
public sealed class SetUpShop : AbstractBlacksmithCard
{
    public SetUpShop() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 2, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var options = Wiz.DrawPile(Owner).Concat(Wiz.DiscardPile(Owner)).ToList();
        if (options.Count == 0) return;
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, Math.Min(options.Count, DynamicVars["Magic"].IntValue));
        foreach (var card in (await CardSelectCmd.FromSimpleGrid(ctx, options, Owner, prefs)).ToList())
        {
            if (card.IsUpgradable) CardCmd.Upgrade(card);
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
        }
    }
}
