// Ported from src/main/java/thePackmaster/cards/alignmentpack/Square.java

namespace Packmaster.PackmasterCode.Cards.Alignmentpack;

/// <summary>Ethereal. When drawn, draw 2 (+1) cards.</summary>
public sealed class Square : global::Packmaster.PackmasterCode.Cards.Arcanapack.AbstractAstrologerCard
{
    public Square() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVar("Magic", 2, 1);
        WithKeyword(CardKeyword.Ethereal);
    }

    /// <summary>Playing it does nothing; the effect happens when drawn (StS1 triggerWhenDrawn).</summary>
    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;

    public override async Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
    }
}
