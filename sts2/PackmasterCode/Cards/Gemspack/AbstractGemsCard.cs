// Ported from src/main/java/thePackmaster/cards/gemspack/AbstractGemsCard.java
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Gemspack;

/// <summary>
/// A Gem: a 0-cost card whose effect can be socketed into another card at a rest site (Enhance). Upgraded gems
/// have 2 uses: socketing downgrades them instead of removing them from the deck.
/// </summary>
public abstract class AbstractGemsCard : PackmasterCard
{
    protected AbstractGemsCard(CardRarity rarity, bool playable = true) : base(0, CardType.Skill, rarity, TargetType.Self)
    {
        if (!playable) WithKeyword(CardKeyword.Unplayable);
        WithTip(PmTips.Gem);
        WithTip(PmTips.Modify);
    }

    /// <summary>The modifier this gem puts on a card (mutable for socketing, canonical for the gem's own effect).</summary>
    public abstract GemModifier CreateModifier(bool mutable);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreateModifier(false).Effect(ctx, Owner);
    }
}
