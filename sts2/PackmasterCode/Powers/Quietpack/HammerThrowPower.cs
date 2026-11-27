// Ported from src/main/java/thePackmaster/powers/quietpack/HammerThrowPower.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Quietpack;

/// <summary>At the end of your turn, deal [Amount] damage to ALL enemies for each Retained card in your hand.</summary>
public sealed class HammerThrowPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || CombatState == null) return;
        var retained = Wiz.Hand(player).Count(c => c.Keywords.Contains(CardKeyword.Retain));
        if (retained == 0) return;
        Flash();
        for (int i = 0; i < retained; i++)
            await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies.ToList(), Amount, ValueProp.Unpowered, Owner);
    }
}
