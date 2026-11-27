// Ported from src/main/java/thePackmaster/cards/coresetpack/Sideboard.java
namespace Packmaster.PackmasterCode.Cards.Coresetpack;

/// <summary>Gain 8 (+3) Block. Put a card from your discard pile on top of your draw pile.</summary>
public sealed class Sideboard : PackmasterCard
{
    public Sideboard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(8, 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        var chosen = await CommonActions.SelectSingleCard(this, SelectionScreenPrompt, ctx, PileType.Discard);
        if (chosen != null)
            await CardPileCmd.Add(chosen, PileType.Draw, CardPilePosition.Top);
    }
}
