// Ported from src/main/java/thePackmaster/powers/alignmentpack/RememberPower.java
namespace Packmaster.PackmasterCode.Powers.Alignmentpack;

/// <summary>At the end of your turn, shuffle up to [Amount] cards from your hand into your draw pile.</summary>
public sealed class RememberPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || player.PlayerCombatState == null || player.PlayerCombatState.Hand.IsEmpty) return;
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, Amount);
        var chosen = await CardSelectCmd.FromHand(choiceContext, player, prefs, null, this);
        foreach (var card in chosen.ToList())
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random);
    }
}
