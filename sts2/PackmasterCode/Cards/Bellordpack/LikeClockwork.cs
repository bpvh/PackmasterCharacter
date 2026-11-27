// Ported from src/main/java/thePackmaster/cards/bellordpack/LikeClockwork.java
using Packmaster.PackmasterCode.Powers.Bellordpack;

namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>Power. At the end of your turn, gain 4 Block for each Curse or Status in your hand. Upgrade: Innate.</summary>
public sealed class LikeClockwork : AbstractBellordCard
{
    public LikeClockwork() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 4);
        WithKeyword(CardKeyword.Innate, UpgradeType.Add);
        WithTip(typeof(LikeClockworkPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<LikeClockworkPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
