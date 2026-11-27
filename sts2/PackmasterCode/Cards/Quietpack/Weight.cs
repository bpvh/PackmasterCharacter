// Ported from src/main/java/thePackmaster/cards/quietpack/Weight.java
namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>Status. Unplayable. Retain. When you discard this, Exhaust it.</summary>
public sealed class Weight : AbstractQuietCard
{
    public Weight() : base(0, CardType.Status, CardRarity.Token, TargetType.None)
    {
        WithKeyword(CardKeyword.Unplayable);
        WithKeyword(CardKeyword.Retain);
    }

    public override int MaxUpgradeLevel => 0;

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;

    public override async Task AfterCardDiscarded(PlayerChoiceContext ctx, CardModel card)
    {
        if (card == this && Pile?.Type == PileType.Discard) await CardCmd.Exhaust(ctx, this);
    }
}
