// Ported from src/main/java/thePackmaster/cards/bellordpack/Clang.java
namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>0 cost. Deal 14 (+4) damage. Can only be played if you have a Curse or Status in your hand.</summary>
public sealed class Clang : AbstractBellordCard
{
    public Clang() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(14, 4);
    }

    protected override bool IsPlayable => base.IsPlayable && CurseOrStatusInHand;
    protected override bool ShouldGlowGoldInternal => CurseOrStatusInHand;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
    }
}
