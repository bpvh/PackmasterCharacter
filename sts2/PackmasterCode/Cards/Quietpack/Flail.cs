// Ported from src/main/java/thePackmaster/cards/quietpack/Flail.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>2 cost. Deal 13 (+3) damage twice. Add 2 Weights to your hand.</summary>
public sealed class Flail : AbstractQuietCard
{
    public Flail() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(13, 3);
        WithTip(typeof(Weight));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, hitCount: 2, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
        for (int i = 0; i < 2; i++) await Wiz.MakeInHand(Wiz.CreateCard<Weight>(Owner), Owner);
    }
}
