// Ported from src/main/java/thePackmaster/cards/evenoddpack/LeftHook.java
using BaseLib.Cards.Variables;

using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Evenoddpack;

/// <summary>0 cost. Deal 3 (+2) damage. Odd: return this to your hand. Exhaustive 7.</summary>
public sealed class LeftHook : AbstractEvenOddCard
{
    private bool _returnToHand;

    public LeftHook() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(3, 2);
        WithVar(new ExhaustiveVar(7));
        WithTip(PmTips.Odd);
    }

    protected override bool ShouldGlowGoldInternal => IsOdd();

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        _returnToHand = IsOdd(play);
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_blunt").Execute(ctx);
    }

    // StS1 returnToHand. Runs after the card reaches its result pile; exhausted copies stay exhausted.
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this || !_returnToHand) return;
        _returnToHand = false;
        if (Pile?.Type == PileType.Discard) await CardPileCmd.Add(this, PileType.Hand);
    }
}
