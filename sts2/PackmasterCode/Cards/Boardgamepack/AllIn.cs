// Ported from src/main/java/thePackmaster/cards/boardgamepack/AllIn.java
using Packmaster.PackmasterCode.Powers.Boardgamepack;

namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

/// <summary>Deal 30 (+10) damage. Draw 3 (+1) cards. Gain 3 (+1) energy. Skip your next turn. Exhaust.</summary>
public sealed class AllIn : AbstractBoardCard
{
    public AllIn() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(30, 10);
        WithVar("Magic", 3, 1); // cards drawn and energy gained (StS1: 3/4 energy, same as the draw)
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(SkipNextTurnPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
        await PlayerCmd.GainEnergy(DynamicVars["Magic"].BaseValue, Owner);
        await CommonActions.ApplySelf<SkipNextTurnPower>(ctx, this, 1);
    }
}
