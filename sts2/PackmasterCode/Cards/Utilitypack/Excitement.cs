// Ported from src/main/java/thePackmaster/cards/utilitypack/Excitement.java
using BaseLib.Cards.Variables;

namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>Gain 2 energy. Exhaust. Upgrade: instead of Exhaust, Exhaustive 2.</summary>
public sealed class Excitement : AbstractUtilityCard
{
    public const int Energy = 2;
    public const int ExhaustiveUpgraded = 2;

    public Excitement() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithKeyword(CardKeyword.Exhaust, UpgradeType.Remove);
        WithVar(new ExhaustiveVar(0));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars[ExhaustiveVar.Key].UpgradeValueBy(ExhaustiveUpgraded);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PlayerCmd.GainEnergy(Energy, Owner);
    }
}
