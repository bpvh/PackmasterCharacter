// Ported from src/main/java/thePackmaster/powers/energyandechopack/PanacherPower.java
namespace Packmaster.PackmasterCode.Powers.Energyandechopack;

/// <summary>Every time 5 cards are created, deal [Amount] damage to ALL enemies.</summary>
public sealed class PanacherPower : PackmasterPower
{
    public const int CardsNeeded = 5;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private int _left = CardsNeeded;

    // StS1 OnCreateCardInterface: any card made for combat (generated), by anyone's effect for this player.
    public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
    {
        if (!IsOwnersCard(card)) return;
        _left--;
        if (_left > 0) return;
        _left = CardsNeeded;
        Flash();
        var enemies = Owner.CombatState?.HittableEnemies.ToList();
        if (enemies is { Count: > 0 })
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), enemies, Amount, ValueProp.Unpowered, Owner);
    }
}
