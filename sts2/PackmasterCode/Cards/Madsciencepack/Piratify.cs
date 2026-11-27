// Ported from src/main/java/thePackmaster/cards/madsciencepack/Piratify.java
using Packmaster.PackmasterCode.Cards.Legacypack;
using Packmaster.PackmasterCode.Powers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

/// <summary>At the start of each turn, Modify a random card in hand: "Gain a Cannonball" (upgraded: Cannonball+).</summary>
public sealed class Piratify : AbstractMadScienceCard
{
    public Piratify() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithTip(PmTips.Modify);
        WithTip(typeof(Cannonball));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (IsUpgraded) await CommonActions.ApplySelf<PiratifyPowerUpp>(ctx, this, 1);
        else await CommonActions.ApplySelf<PiratifyPower>(ctx, this, 1);
    }
}
