using MegaCrit.Sts2.Core.Entities.Relics;

namespace Packmaster.PackmasterCode.Relics;

/// <summary>
/// Starter relic. StS1: "At the start of each combat, gain [E] and draw 1 card."
/// (StS1 drew at battle start and gave energy on the first turn; here both happen once the first turn has started.)
/// Upgraded by Bag of Holding (an Ancient option, ported later).
/// </summary>
public sealed class HandyHaversack : PackmasterRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1), new CardsVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || Owner.PlayerCombatState?.TurnNumber != 1) return;
        Flash();
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }
}
