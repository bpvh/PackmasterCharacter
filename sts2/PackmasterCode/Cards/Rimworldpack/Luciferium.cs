// Ported from src/main/java/thePackmaster/cards/rimworldpack/Luciferium.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Powers.Rimworldpack;

namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>0 cost. Gain 2 energy and 5 Strength. In 1 (+1) turns, add 2 Despair to your hand and lose 5 Strength. Exhaust.</summary>
public sealed class Luciferium : AbstractRimCard
{
    private const int Strength = 5;

    public Luciferium() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 1, 1);
        WithKeyword(CardKeyword.Exhaust);
        WithEnergyTip();
        WithTip(typeof(StrengthPower));
        WithTip(typeof(LuciferiumAddictionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PlayerCmd.GainEnergy(2, Owner);
        await CommonActions.ApplySelf<StrengthPower>(ctx, this, Strength);
        await CommonActions.ApplySelf<LuciferiumAddictionPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
