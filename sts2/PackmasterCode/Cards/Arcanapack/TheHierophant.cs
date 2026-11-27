// Ported from src/main/java/thePackmaster/cards/arcanapack/TheHierophant.java
namespace Packmaster.PackmasterCode.Cards.Arcanapack;

/// <summary>Gain 6 (+4) Block. Remove all your debuffs. Exhaust.</summary>
public sealed class TheHierophant : AbstractAstrologerCard
{
    public TheHierophant() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithBlock(6, 4);
        WithKeyword(CardKeyword.Exhaust);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        foreach (var debuff in Owner.Creature.Powers.Where(p => p.Type == PowerType.Debuff).ToList())
            await PowerCmd.Remove(debuff);
    }
}
