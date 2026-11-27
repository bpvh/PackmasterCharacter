// Ported from src/main/java/thePackmaster/cards/ringofpainpack/Vision.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>Put 1 (+1) card(s) from your draw pile into your hand. Retain your hand this turn. Exhaust.</summary>
public sealed class Vision : AbstractRingOfPainCard
{
    public Vision() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithVar("Magic", 1, 1);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(RetainHandPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var picked = await CommonActions.SelectCards(this, SelectionScreenPrompt, ctx, PileType.Draw, DynamicVars["Magic"].IntValue);
        foreach (var card in picked.ToList()) await CardPileCmd.Add(card, PileType.Hand);
        await CommonActions.ApplySelf<RetainHandPower>(ctx, this, 1);
    }
}
