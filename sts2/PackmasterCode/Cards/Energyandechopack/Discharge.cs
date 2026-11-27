// Ported from src/main/java/thePackmaster/cards/energyandechopack/Discharge.java
using Packmaster.PackmasterCode.Mechanics;

namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

/// <summary>Deal damage equal to 5 times the energy spent this turn. Upgrade: costs 0.</summary>
public sealed class Discharge : AbstractEchoCard
{
    public Discharge() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithCalculatedDamage(0, 5, (card, _) => card.IsCanonical ? 0 : EnergyTracker.SpentThisTurn(card.Owner));
        WithCostUpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_lightning").Execute(ctx);
    }
}
