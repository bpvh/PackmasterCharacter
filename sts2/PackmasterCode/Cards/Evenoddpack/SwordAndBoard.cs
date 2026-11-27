// Ported from src/main/java/thePackmaster/cards/evenoddpack/SwordAndBoard.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>Even: deal 10 (+3) damage. Odd: gain 8 (+3) Block.</summary>
public sealed class SwordAndBoard : AbstractEvenOddCard
{
    public SwordAndBoard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(10, 3);
        WithBlock(8, 3);
        WithTip(Keywords.Even);
        WithTip(Keywords.Odd);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (IsEven(play)) await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        else await CommonActions.CardBlock(this, play);
    }
}
