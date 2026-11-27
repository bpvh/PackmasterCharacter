// Ported from src/main/java/thePackmaster/cards/alignmentpack/Divination.java
namespace Packmaster.PackmasterCode.Cards.Alignmentpack;

/// <summary>Put a card (upgraded: any number of cards) from your hand on the bottom of your draw pile. Draw 3 cards.</summary>
public sealed class Divination : global::Packmaster.PackmasterCode.Cards.Arcanapack.AbstractAstrologerCard
{
    public Divination() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVar("Magic", 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var hand = Owner.PlayerCombatState?.Hand.Cards.Count ?? 0;
        if (hand > 0)
        {
            var prefs = IsUpgraded ? new CardSelectorPrefs(SelectionScreenPrompt, 0, hand) : new CardSelectorPrefs(SelectionScreenPrompt, 1);
            foreach (var card in (await CommonActions.SelectCards(this, prefs, ctx, PileType.Hand)).ToList())
                await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Bottom);
        }
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
    }
}
