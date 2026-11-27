// Ported from src/main/java/thePackmaster/cards/quietpack/Oppressor.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>
/// Deal !D! damage.
/// Discard 1 card.
/// </summary>
public sealed class Oppressor : AbstractQuietCard
{
    public Oppressor() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(11, 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_blunt").Execute(ctx);
        await Wiz.ChooseAndDiscard(ctx, this, (int)1m);
    }
}
