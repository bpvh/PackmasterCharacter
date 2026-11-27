// Ported from src/main/java/thePackmaster/cards/upgradespack/HeartOfTheForge.java
namespace Packmaster.PackmasterCode.Cards.Upgradespack;

/// <summary>Retain. Gain 1 energy (+1 per upgrade; can be upgraded any number of times). When Retained, upgrade this. Exhaust.</summary>
public sealed class HeartOfTheForge : AbstractBlacksmithCard
{
    public HeartOfTheForge() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 1, 1);
        WithKeyword(CardKeyword.Retain);
        WithKeyword(CardKeyword.Exhaust);
        WithEnergyTip();
    }

    public override int MaxUpgradeLevel => 99;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PlayerCmd.GainEnergy(DynamicVars["Magic"].BaseValue, Owner);
    }

    public override Task AfterCardRetained(CardModel card)
    {
        if (card == this && IsUpgradable) CardCmd.Upgrade(this);
        return Task.CompletedTask;
    }
}
