// Ported from src/main/java/thePackmaster/cards/bellordpack/Cuckoo.java
using Packmaster.PackmasterCode.Powers.Bellordpack;

namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>Power. Whenever you draw a Curse, deal 13 damage to a random enemy. Upgrade: Innate.</summary>
public sealed class Cuckoo : AbstractBellordCard
{
    public Cuckoo() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 13);
        WithKeyword(CardKeyword.Innate, UpgradeType.Add);
        WithTip(typeof(CuckooPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<CuckooPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
