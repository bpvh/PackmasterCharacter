// Ported from src/main/java/thePackmaster/cards/rimworldpack/Despair.java
namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>Status, 2 cost. Does nothing. Exhaust. (Rimworld's "mental break" filler.)</summary>
public sealed class Despair : AbstractRimCard
{
    public Despair() : base(2, CardType.Status, CardRarity.Token, TargetType.Self)
    {
        WithKeyword(CardKeyword.Exhaust);
    }

    public override int MaxUpgradeLevel => 0;

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;
}
