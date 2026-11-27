// Ported from src/main/java/thePackmaster/cards/orbpack/Emptiness.java
using Packmaster.PackmasterCode.Orbs;
using Packmaster.PackmasterCode.Powers.Orbpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Orbpack;

/// <summary>Discard a card. Channel 1 (+1) Oblivion.</summary>
public sealed class Emptiness : AbstractOrbCard
{
    public Emptiness() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVar("Magic", 1, 1);
        WithTip(StaticHoverTip.Channeling);
        WithTip(typeof(Oblivion));
        WithTip(typeof(RuinPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Wiz.ChooseAndDiscard(ctx, this, 1);
        for (int i = 0; i < DynamicVars["Magic"].IntValue; i++) await OrbCmd.Channel<Oblivion>(ctx, Owner);
    }
}
