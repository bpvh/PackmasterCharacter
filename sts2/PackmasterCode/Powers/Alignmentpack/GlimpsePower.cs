// Ported from src/main/java/thePackmaster/powers/alignmentpack/GlimpsePower.java
namespace Packmaster.PackmasterCode.Powers.Alignmentpack;

/// <summary>Whenever you draw an Ethereal card, gain [Amount] Block.</summary>
public sealed class GlimpsePower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (!IsOwnersCard(card) || !card.Keywords.Contains(CardKeyword.Ethereal)) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
