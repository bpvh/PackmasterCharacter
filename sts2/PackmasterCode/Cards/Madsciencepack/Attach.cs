// Ported from src/main/java/thePackmaster/cards/madsciencepack/Attach.java
using Packmaster.PackmasterCode.CardModifiers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

/// <summary>Absorb a non-Power card in hand into a discarded card: Modify it, "Play the Absorbed card". Exhaust. Upgrade: costs 0.</summary>
public sealed class Attach : AbstractMadScienceCard
{
    public Attach() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithKeyword(CardKeyword.Exhaust);
        WithCostUpgradeBy(-1);
        WithTip(PmTips.Absorb);
        WithTip(PmTips.Modify);
    }

    private static bool CanAbsorb(CardModel c) => MadScience.Unmodified(c) && MadScience.Playable(c) && c.Type != CardType.Power;
    private static bool CanReceive(CardModel c) => MadScience.Unmodified(c) && MadScience.Playable(c);

    protected override bool IsPlayable =>
        Owner != null && Wiz.Hand(Owner).Any(c => c != this && CanAbsorb(c)) && Wiz.DiscardPile(Owner).Any(CanReceive);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
        var absorbed = (await CardSelectCmd.FromHand(ctx, Owner, prefs, c => c != this && CanAbsorb(c), this)).FirstOrDefault();
        if (absorbed == null) return;
        await CardCmd.Exhaust(ctx, absorbed);
        await MadScience.Modify(ctx, this, Wiz.DiscardPile(Owner), 1, random: false, CanReceive,
            () =>
            {
                var mod = (PlayCardModifier)MadScience.New<PlayCardModifier>()();
                mod.Absorbed = absorbed;
                return mod;
            }, fromHand: false);
    }
}
