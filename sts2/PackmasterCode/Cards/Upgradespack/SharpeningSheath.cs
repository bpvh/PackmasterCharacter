// Ported from src/main/java/thePackmaster/cards/upgradespack/SharpeningSheath.java (+ actions/upgradespack/SharpeningFollowUpAction.java)
namespace Packmaster.PackmasterCode.Cards.Upgradespack;

/// <summary>Draw 2 (+1) cards. Upgrade them.</summary>
public sealed class SharpeningSheath : AbstractBlacksmithCard
{
    public SharpeningSheath() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithVar("Magic", 2, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        foreach (var card in (await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner)).ToList())
            if (card.IsUpgradable) CardCmd.Upgrade(card);
    }
}
