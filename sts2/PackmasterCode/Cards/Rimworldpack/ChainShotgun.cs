// Ported from src/main/java/thePackmaster/cards/rimworldpack/ChainShotgun.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>2 cost. Deal 7 (+2) damage to a random enemy 3 times.</summary>
public sealed class ChainShotgun : AbstractRimCard
{
    public ChainShotgun() : base(2, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
    {
        WithDamage(7, 2);
        WithVar("Magic", 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        for (int i = 0; i < DynamicVars["Magic"].IntValue; i++)
            await Wiz.AttackRandomEnemy(ctx, this, play, "vfx/vfx_heavy_blunt");
    }
}
