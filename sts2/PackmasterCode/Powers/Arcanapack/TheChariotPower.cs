// Ported from src/main/java/thePackmaster/powers/arcanapack/TheChariotPower.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Arcanapack;

/// <summary>Whenever you discard a card, gain [Amount] Vigor.</summary>
public sealed class TheChariotPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // VERIFY: StS1 only counted manual discards; AfterCardDiscarded should not fire for the end-of-turn flush.
    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (!IsOwnersCard(card)) return;
        Flash();
        await Wiz.ApplyPower<VigorPower>(choiceContext, Owner, Amount, Owner);
    }
}
