// Ported from src/main/java/thePackmaster/cards/quietpack/GlaiveThrow.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>Deal 7 damage. Draw 2 (+1) cards, then discard 2 (+1) cards.</summary>
public sealed class GlaiveThrow : AbstractQuietCard
{
    public GlaiveThrow() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(7);
        WithVar("Magic", 2, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
        await Wiz.ChooseAndDiscard(ctx, this, DynamicVars["Magic"].IntValue);
    }
}
