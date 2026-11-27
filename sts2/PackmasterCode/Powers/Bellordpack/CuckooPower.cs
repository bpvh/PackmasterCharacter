// Ported from src/main/java/thePackmaster/powers/bellordpack/CuckooPower.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Bellordpack;

/// <summary>Whenever you draw a Curse, deal [Amount] damage to a random enemy.</summary>
public sealed class CuckooPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (!IsOwnersCard(card) || card.Type != CardType.Curse || CombatState == null) return;
        var enemies = CombatState.HittableEnemies.ToList();
        if (enemies.Count == 0) return;
        Flash();
        var target = Wiz.CardRng(Owner.Player!).NextItem(enemies)!;
        await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Unpowered, Owner);
    }
}
