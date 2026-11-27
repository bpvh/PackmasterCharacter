using Packmaster.PackmasterCode.Packs;

namespace Packmaster.PackmasterCode.Relics;

/// <summary>
/// Base for the Packmaster's starter relics (Handy Haversack and its upgrade, Bag of Holding): runs the pack
/// draft at the start of the first turn of the first combat if it hasn't happened yet. See PackDraft for why.
/// Subclasses that override AfterPlayerTurnStart must call base first.
/// </summary>
public abstract class PackDraftRelic : PackmasterRelic
{
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;
        await PackDraft.RunIfPending(choiceContext, Owner);
    }
}
