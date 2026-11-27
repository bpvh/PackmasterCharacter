// Ported from src/main/java/thePackmaster/cards/upgradespack/RedHotHammer.java
namespace Packmaster.PackmasterCode.Cards.Upgradespack;

/// <summary>Deal 10 damage (+3 per upgrade). Can be upgraded any number of times.</summary>
public sealed class RedHotHammer : AbstractBlacksmithCard
{
    public RedHotHammer() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(10, 3);
    }

    public override int MaxUpgradeLevel => 99;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
    }
}
