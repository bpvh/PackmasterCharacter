// Ported from src/main/java/thePackmaster/cards/orbpack/Stockpile.java
using MegaCrit.Sts2.Core.Models.Orbs;

namespace Packmaster.PackmasterCode.Cards.Orbpack;

/// <summary>Discard a card. If it's a Power, Channel 1 Plasma; otherwise Channel 2 Lightning. Upgrade: costs 0.</summary>
public sealed class Stockpile : AbstractOrbCard
{
    public Stockpile() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVar("Magic", 1);
        WithVar("Magic2", 2);
        WithCostUpgradeBy(-1);
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(PlasmaOrb));
        WithTip(typeof(LightningOrb));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        foreach (var c in await DiscardFromHand(ctx, 1, 1))
        {
            if (c.Type == CardType.Power)
                for (int i = 0; i < DynamicVars["Magic"].IntValue; i++) await OrbCmd.Channel<PlasmaOrb>(ctx, Owner);
            else
                for (int i = 0; i < DynamicVars["Magic2"].IntValue; i++) await OrbCmd.Channel<LightningOrb>(ctx, Owner);
        }
    }
}
