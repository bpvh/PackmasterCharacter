// Ported from src/main/java/thePackmaster/cards/energyandechopack/Flashlight.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

/// <summary>Innate. Gain 1 energy (upgraded: and 1 energy next turn). Exhaust.</summary>
public sealed class Flashlight : AbstractEchoCard
{
    public Flashlight() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 1);
        WithKeyword(CardKeyword.Innate);
        WithKeyword(CardKeyword.Exhaust);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PlayerCmd.GainEnergy(1, Owner);
        if (IsUpgraded) await Wiz.ApplyPower<EnergyNextTurnPower>(ctx, Owner.Creature, DynamicVars["Magic"].BaseValue, Owner.Creature, this);
    }
}
