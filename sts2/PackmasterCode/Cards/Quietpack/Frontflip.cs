// Ported from src/main/java/thePackmaster/cards/quietpack/Frontflip.java
namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>2 cost. Gain 13 (+2) Block. Draw 3 (+1) cards. Discard the Unplayable cards drawn.</summary>
public sealed class Frontflip : AbstractQuietCard
{
    public Frontflip() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithBlock(13, 2);
        WithVar("Magic", 3, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        var drawn = (await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner)).ToList();
        var unplayable = drawn.Where(c => c.Keywords.Contains(CardKeyword.Unplayable) && c.Pile?.Type == PileType.Hand).ToList();
        if (unplayable.Count > 0) await CardCmd.Discard(ctx, unplayable);
    }
}
