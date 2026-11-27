// Ported from src/main/java/thePackmaster/cards/upgradespack/CarelessSwing.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Upgradespack;

/// <summary>Deal 10 (+3) damage. Exhaust a random unupgraded card in your hand.</summary>
public sealed class CarelessSwing : AbstractBlacksmithCard
{
    public CarelessSwing() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(10, 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        var targets = Wiz.Hand(Owner).Where(c => !c.IsUpgraded).ToList();
        if (targets.Count > 0) await CardCmd.Exhaust(ctx, Wiz.CardRng(Owner).NextItem(targets)!);
    }
}
