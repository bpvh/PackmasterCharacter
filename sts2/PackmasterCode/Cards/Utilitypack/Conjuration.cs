// Ported from src/main/java/thePackmaster/cards/utilitypack/Conjuration.java and actions/utilitypack/ConjurationAction.java
using MegaCrit.Sts2.Core.Localization;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>Choose a card from your draw pile. Play it, and Exhaust it (upgraded: don't Exhaust it).</summary>
public sealed class Conjuration : AbstractUtilityCard
{
    public Conjuration() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override bool IsPlayable => Owner != null && Wiz.DrawPile(Owner).Count > 0;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var pile = Wiz.DrawPile(Owner).ToList();
        if (pile.Count == 0) return;
        var prefs = new CardSelectorPrefs(new LocString("packmaster_ui", "PACKMASTER-CONJURATION_ACTION.text0"), 1);
        var card = (await CardSelectCmd.FromSimpleGrid(ctx, pile, Owner, prefs)).FirstOrDefault();
        if (card == null) return;
        if (!IsUpgraded && !card.Keywords.Contains(CardKeyword.Exhaust)) CardCmd.ApplyKeyword(card, CardKeyword.Exhaust);
        // VERIFY: auto-playing a card straight from the draw pile (StS1 NewQueueCardAction after removing it).
        await CardCmd.AutoPlay(ctx, card, null);
    }
}
