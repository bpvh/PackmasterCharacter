// Ported from src/main/java/thePackmaster/cards/orbpack/IcicleCrash.java
using MegaCrit.Sts2.Core.Models.Orbs;

namespace Packmaster.PackmasterCode.Cards.Orbpack;

/// <summary>Deal 13 (+4) damage. Channel 1 Frost for each empty orb slot.</summary>
public sealed class IcicleCrash : AbstractOrbCard
{
    public IcicleCrash() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(13, 4);
        WithVar("Magic", 1);
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(FrostOrb));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        var n = EmptyOrbSlots * DynamicVars["Magic"].IntValue;
        for (int i = 0; i < n; i++) await OrbCmd.Channel<FrostOrb>(ctx, Owner);
    }
}
