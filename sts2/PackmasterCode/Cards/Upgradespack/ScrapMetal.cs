// Ported from src/main/java/thePackmaster/cards/upgradespack/ScrapMetal.java (+ actions/upgradespack/ScrapMetalAction.java)
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Upgradespack;

/// <summary>0 cost. For every 3 (2) Block you have, upgrade a random card in your hand, draw or discard pile. Exhaust.</summary>
public sealed class ScrapMetal : AbstractBlacksmithCard
{
    public ScrapMetal() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 3, -1);
        WithKeyword(CardKeyword.Exhaust);
    }

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var per = Math.Max(1, DynamicVars["Magic"].IntValue);
        var upgrades = Owner.Creature.Block / per;
        Wiz.UpgradeRandom(Owner, Wiz.Hand(Owner).Concat(Wiz.DrawPile(Owner)).Concat(Wiz.DiscardPile(Owner)), upgrades);
        return Task.CompletedTask;
    }
}
