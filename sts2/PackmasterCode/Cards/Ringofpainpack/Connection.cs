// Ported from src/main/java/thePackmaster/cards/ringofpainpack/Connection.java
using Packmaster.PackmasterCode.Powers.Ringofpainpack;

namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>Power, 0 cost. Whenever you play a card, Upgrade it and ALL copies of it. Upgrade: Innate.</summary>
public sealed class Connection : AbstractRingOfPainCard
{
    public Connection() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithKeyword(CardKeyword.Innate, UpgradeType.Add);
        WithTip(typeof(ConnectionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<ConnectionPower>(ctx, this, 1);
    }
}
