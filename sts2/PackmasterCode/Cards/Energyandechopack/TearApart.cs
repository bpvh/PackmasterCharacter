// Ported from src/main/java/thePackmaster/cards/energyandechopack/TearApart.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

/// <summary>Deal 17 (+5) damage. If this kills an enemy, gain 1 energy (StS1 Sunder). Echo.</summary>
public sealed class TearApart : AbstractEchoCard
{
    public TearApart() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(17, 5);
        WithTip(PmTips.Echo);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var attack = await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
        if (attack.Results.Any(r => r.WasTargetKilled)) await PlayerCmd.GainEnergy(1, Owner);
        await Echo();
    }
}
