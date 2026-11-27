using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;

namespace Packmaster.PackmasterCode.Packs;

/// <summary>
/// The pack draft (StS1 PackSetupScreen): one round per "choice" slot, each offering PacksPerChoice packs as
/// their preview cards in the game's own card selection grid. CardSelectCmd goes through PlayerChoiceContext,
/// so in co-op each player picks on their own client and the result is synced like any other card choice.
///
/// Where it runs: from the Packmaster's starter relic at the start of the first turn of the first combat
/// (PackDraftRelic). That hook is confirmed to work with selection screens; StS1 drafted before Neow instead.
/// Until the draft runs, the card pool already contains the fixed and random packs, so Neow's card
/// rewards still come from real packs.
/// TODO(Phase 2 follow-up): move the draft to a screen before Neow once the run-start/Neow-room hooks are
/// confirmed against sts2.dll, and add the StS1 pack summary (star ratings) to the preview cards' hover tips.
/// </summary>
public static class PackDraft
{
    private static LocString Prompt => new("packmaster_ui", "PACKMASTER-PACK_SETUP.text0"); // "Draft a Pack!"

    public static async Task RunIfPending(PlayerChoiceContext ctx, Player player)
    {
        if (!PackRunState.IsPackmaster(player)) return;

        while (PackRunState.DraftPending(player))
        {
            var offer = PackRunState.PendingOffers(player)[0];
            var previews = offer.Select(p => p.PreviewCard.ToMutable()).ToList();
            var prefs = new CardSelectorPrefs(Prompt, 1);
            var picked = (await CardSelectCmd.FromSimpleGrid(ctx, previews, player, prefs)).FirstOrDefault();

            var pack = (picked as PackPreviewCard)?.Pack;
            // A pick is required; if the screen somehow returns nothing, take the first offer (StS1 had no skip).
            PackRunState.CompleteRound(player, pack ?? offer[0]);
        }
    }
}
