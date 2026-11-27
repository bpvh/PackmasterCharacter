// Ported from src/main/java/thePackmaster/cards/quietpack/Shine.java (+ actions/quietpack/ShineAction.java)
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>Unplayable. Retain. When drawn, Upgrade a random other card in your hand (upgraded: every card in your hand).</summary>
public sealed class Shine : AbstractQuietCard
{
    public Shine() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithKeyword(CardKeyword.Unplayable);
        WithKeyword(CardKeyword.Retain);
    }

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;

    public override Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        if (card != this) return Task.CompletedTask;
        var others = Wiz.Hand(Owner).Where(c => c != this).ToList();
        if (others.Count == 0) return Task.CompletedTask;
        if (IsUpgraded)
        {
            var upgradable = others.Where(c => c.IsUpgradable).ToList();
            if (upgradable.Count > 0) foreach (var c in upgradable) CardCmd.Upgrade(c);
        }
        else
        {
            var pick = Wiz.CardRng(Owner).NextItem(others)!;
            if (pick.IsUpgradable) CardCmd.Upgrade(pick);
        }
        return Task.CompletedTask;
    }
}
