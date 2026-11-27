// Ported from src/main/java/thePackmaster/cards/energyandechopack/MagnetRise.java
using Packmaster.PackmasterCode.Mechanics;

namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

/// <summary>Gain 8 (+3) Block. Costs 0 if you generated extra energy this turn.</summary>
public sealed class MagnetRise : AbstractEchoCard
{
    public MagnetRise() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(8, 3);
    }

    private bool Generated => Owner != null && EnergyTracker.GeneratedThisTurn(Owner) > 0;

    protected override bool ShouldGlowGoldInternal => Generated;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || !Generated) return false;
        modifiedCost = 0;
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
    }
}
