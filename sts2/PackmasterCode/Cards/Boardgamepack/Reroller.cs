// Ported from src/main/java/thePackmaster/cards/boardgamepack/Reroller.java
using Packmaster.PackmasterCode.Powers.Boardgamepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

/// <summary>Gain 8 (+3) Block. Deal 8 (+3) damage. Reroll (doesn't use up Dice).</summary>
public sealed class Reroller : AbstractBoardCard, IRerollCard
{
    public Reroller() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithBlock(8, 3);
        WithDamage(8, 3);
        WithTip(PmTips.Reroll);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_blunt").Execute(ctx);
    }
}
