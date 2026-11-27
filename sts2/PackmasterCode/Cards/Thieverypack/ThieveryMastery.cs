// Ported from src/main/java/thePackmaster/cards/thieverypack/ThieveryMastery.java
using Packmaster.PackmasterCode.Powers.Thieverypack;

namespace Packmaster.PackmasterCode.Cards.Thieverypack;

/// <summary>Whenever you steal something, gain twice as much of it. Upgrade: Innate.</summary>
public sealed class ThieveryMastery : AbstractThieveryCard
{
    public ThieveryMastery() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithKeyword(CardKeyword.Innate, UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!Owner.Creature.HasPower<ThieveryMasteryPower>())
            await CommonActions.ApplySelf<ThieveryMasteryPower>(ctx, this, 1);
    }
}
