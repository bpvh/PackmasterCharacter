// Ported from src/main/java/thePackmaster/cards/quietpack/Crunch.java
namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>Unplayable. Retain. When drawn, draw 2 (+1) cards.</summary>
public sealed class Crunch : AbstractQuietCard
{
    public Crunch() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVar("Magic", 2, 1);
        WithKeyword(CardKeyword.Unplayable);
        WithKeyword(CardKeyword.Retain);
    }

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;

    public override async Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        if (card == this) await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
    }
}
