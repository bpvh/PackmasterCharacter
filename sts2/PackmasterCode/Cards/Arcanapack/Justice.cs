// Ported from src/main/java/thePackmaster/cards/arcanapack/Justice.java
namespace Packmaster.PackmasterCode.Cards.Arcanapack;

/// <summary>Gain 7 (+2) Block for each enemy.</summary>
public sealed class Justice : AbstractAstrologerCard
{
    public Justice() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(7, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var enemies = CombatState?.HittableEnemies.Count() ?? 0;
        for (int i = 0; i < enemies; i++) await CommonActions.CardBlock(this, play);
    }
}
