// Ported from src/main/java/thePackmaster/cards/alignmentpack/Conjunction.java

namespace Packmaster.PackmasterCode.Cards.Alignmentpack;

/// <summary>Ethereal. When drawn, gain 8 (+3) Block.</summary>
public sealed class Conjunction : global::Packmaster.PackmasterCode.Cards.Arcanapack.AbstractAstrologerCard
{
    public Conjunction() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(8, 3);
        WithKeyword(CardKeyword.Ethereal);
    }

    /// <summary>Playing it does nothing; the effect happens when drawn (StS1 triggerWhenDrawn).</summary>
    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;

    public override async Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;
        await CommonActions.CardBlock(this, null);
    }
}
