// Ported from src/main/java/thePackmaster/cards/energyandechopack/Panacher.java
using Packmaster.PackmasterCode.Powers.Energyandechopack;

namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

/// <summary>Every time 5 cards are created, deal 10 (+4) damage to ALL enemies.</summary>
public sealed class Panacher : AbstractEchoCard
{
    public Panacher() : base(0, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 10, 4);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<PanacherPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
