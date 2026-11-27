// Ported from src/main/java/thePackmaster/cards/energyandechopack/Crystallize.java
namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

/// <summary>Gain 3 (+1) Block for each energy you have.</summary>
public sealed class Crystallize : AbstractEchoCard
{
    public Crystallize() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        // Energy after paying for this card, as StS1 recalculated in use().
        WithCalculatedBlock(0, 3, (card, _) => card.IsCanonical ? 0 : card.Owner.PlayerCombatState?.Energy ?? 0, bonusUpgrade: 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var block = DynamicVars.CalculatedBlock.Calculate(play.Target);
        await CreatureCmd.GainBlock(Owner.Creature, block, DynamicVars.CalculatedBlock.Props, play);
    }
}
