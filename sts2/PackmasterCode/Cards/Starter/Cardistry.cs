using Packmaster.PackmasterCode.Powers;

namespace Packmaster.PackmasterCode.Cards.Starter;

/// <summary>
/// StS1: 0 cost Skill. Gain 2 (+2) Block. The next card you play this turn is returned to the top of your draw pile.
/// StS1 used the vanilla ReboundPower; StS2 has no equivalent we can rely on, so CardistryPower reimplements it.
/// </summary>
public sealed class Cardistry : PackmasterCard
{
    public Cardistry() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        WithBlock(2, 2);
        WithTip(typeof(CardistryPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await CommonActions.ApplySelf<CardistryPower>(ctx, this, 1);
    }
}
