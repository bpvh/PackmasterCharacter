// Ported from src/main/java/thePackmaster/powers/bellordpack/LikeClockworkPower.java
using Packmaster.PackmasterCode.Cards.Bellordpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Bellordpack;

/// <summary>At the end of your turn, gain [Amount] Block for each Curse or Status in your hand.</summary>
public sealed class LikeClockworkPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        var count = Wiz.Hand(player).Count(AbstractBellordCard.IsCurseOrStatus);
        if (count == 0) return;
        Flash();
        for (int i = 0; i < count; i++) await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
