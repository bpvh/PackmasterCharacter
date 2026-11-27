// Ported from src/main/java/thePackmaster/cards/ringofpainpack/IceBeast.java
using MegaCrit.Sts2.Core.Models.Orbs;

namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>3 cost. Gain 14 (+3 per upgrade) Block. Deal 10 damage. Evolves when you gain Block. Evolved: +10 damage, Channel 1 Frost.</summary>
public sealed class IceBeast : AbstractEvolveCard
{
    public IceBeast() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(10);
        WithBlock(14);
        WithVar(EvolvedVar, 0);
        WithTip(typeof(FrostOrb));
        WithTip(Util.PmTips.Evolve);
    }

    protected override void OnUpgrade()
    {
        if (CurrentUpgradeLevel >= MaxUpgrades) DynamicVars.Damage.UpgradeValueBy(10);
        else DynamicVars.Block.UpgradeValueBy(3);
        base.OnUpgrade();
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await CommonActions.CardAttack(this, play, vfx: IsEvolved ? "vfx/vfx_heavy_blunt" : "vfx/vfx_attack_blunt").Execute(ctx);
        if (IsEvolved) await OrbCmd.Channel<FrostOrb>(ctx, Owner);
    }

    public override Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (InHand && creature == Owner.Creature && amount > 0) Evolve();
        return Task.CompletedTask;
    }
}
