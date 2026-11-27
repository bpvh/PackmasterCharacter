// Ported from src/main/java/thePackmaster/powers/rimworldpack/HoldCardPower.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Rimworldpack;

/// <summary>On an enemy: holds [Amount] card(s) (Tainted Weapon). When it dies, they return to their owner's hand.</summary>
public sealed class HoldCardPower : PackmasterPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private readonly List<CardModel> _cards = [];

    public void Hold(CardModel card) => _cards.Add(card);

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Owner || wasRemovalPrevented) return;
        foreach (var card in _cards)
        {
            var owner = card.Owner;
            if (owner?.Creature.CombatState == null) continue;
            await Wiz.MakeInHand(card.CreateClone(), owner);
        }
        _cards.Clear();
    }
}
