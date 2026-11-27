// Ported from src/main/java/thePackmaster/cards/bellordpack/Reverberation.java
namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>Gain 6 (+2) Block. At the start of your turn, if this is in your discard pile, return it to your hand.</summary>
public sealed class Reverberation : AbstractBellordCard
{
    public Reverberation() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(6, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner && Pile?.Type == PileType.Discard) await CardPileCmd.Add(this, PileType.Hand);
    }
}
