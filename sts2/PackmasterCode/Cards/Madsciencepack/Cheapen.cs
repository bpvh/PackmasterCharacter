// Ported from src/main/java/thePackmaster/cards/madsciencepack/Cheapen.java
using Packmaster.PackmasterCode.Mechanics;
using Packmaster.PackmasterCode.CardModifiers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

/// <summary>Modify a random (upgraded: chosen) card in your draw pile: "Costs 0". Amplify 1: don't Exhaust. Exhaust.</summary>
public sealed class Cheapen : AbstractMadScienceCard, IAmplifyCard
{
    public int AmplifyCost => 1;
    private bool _amplified;

    public Cheapen() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithKeyword(CardKeyword.Exhaust);
        WithTip(PmTips.Modify);
        WithTip(PmTips.Amplify);
    }

    protected override bool ShouldGlowGoldInternal => Amplify.WouldAmplify(this);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MadScience.Modify(ctx, this, Wiz.DrawPile(Owner), 1, random: !IsUpgraded, c => c.EnergyCost.GetResolved() > 0,
            MadScience.New<CheapenModifier>(), fromHand: false);
        _amplified = await Amplify.TryPay(this);
    }

    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay,
        ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        if (card != this || !_amplified || pileType != PileType.Exhaust) return (pileType, position);
        _amplified = false;
        return (PileType.Discard, position);
    }
}
