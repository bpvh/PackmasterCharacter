namespace Packmaster.PackmasterCode.Cards.Starter;

/// <summary>StS1: 1 cost Skill, gain 5 (+3) Block.</summary>
public sealed class Defend : PackmasterCard
{
    public Defend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        WithBlock(5, 3);
        WithTags(CardTag.Defend);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
    }
}
