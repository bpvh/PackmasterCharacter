// Ported from src/main/java/thePackmaster/cards/utilitypack/Replenish.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>Put 2 cards from your discard pile into your hand. Exhaust (upgrade removes Exhaust).</summary>
public sealed class Replenish : AbstractUtilityCard
{
    public Replenish() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 2);
        WithKeyword(CardKeyword.Exhaust, UpgradeType.Remove);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var discard = Wiz.DiscardPile(Owner).ToList();
        if (discard.Count == 0) return;
        var n = Math.Min(DynamicVars["Magic"].IntValue, discard.Count);
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, n);
        var chosen = (await CardSelectCmd.FromSimpleGrid(ctx, discard, Owner, prefs)).ToList();
        if (chosen.Count > 0) await CardPileCmd.Add(chosen, PileType.Hand, source: this);
    }
}
