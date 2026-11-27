// Ported from src/main/java/thePackmaster/cards/aggressionpack/Slam.java
namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

/// <summary>Deal 5 (+2) damage. Gain Block equal to unblocked damage dealt (StS1 Wallop).</summary>
public sealed class Slam : AbstractAggressionCard
{
    public Slam() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(5, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var attack = await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
        var unblocked = attack.Results.Sum(r => r.UnblockedDamage);
        if (unblocked > 0) await CreatureCmd.GainBlock(Owner.Creature, unblocked, ValueProp.Unpowered, play);
    }
}
