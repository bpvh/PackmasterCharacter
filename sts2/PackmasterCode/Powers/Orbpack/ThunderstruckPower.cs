// Ported from src/main/java/thePackmaster/powers/orbpack/ThunderstruckPower.java
using MegaCrit.Sts2.Core.Models.Orbs;

namespace Packmaster.PackmasterCode.Powers.Orbpack;

/// <summary>Whenever you play a Strike, Channel [Amount] Lightning.</summary>
public sealed class ThunderstruckPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (!IsOwnersCard(cardPlay.Card) || !cardPlay.Card.Tags.Contains(CardTag.Strike)) return;
        Flash();
        for (int i = 0; i < Amount; i++) await OrbCmd.Channel<LightningOrb>(context, Owner.Player!);
    }
}
