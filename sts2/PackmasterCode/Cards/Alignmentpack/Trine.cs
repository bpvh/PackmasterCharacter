// Ported from src/main/java/thePackmaster/cards/alignmentpack/Trine.java
using Packmaster.PackmasterCode.Powers.Alignmentpack;

namespace Packmaster.PackmasterCode.Cards.Alignmentpack;

/// <summary>Ethereal. When drawn, the next card you play this turn is played twice. Upgrade: cost 1.</summary>
public sealed class Trine : global::Packmaster.PackmasterCode.Cards.Arcanapack.AbstractAstrologerCard
{
    public Trine() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithTip(typeof(TrinePower));
        WithKeyword(CardKeyword.Ethereal);
    }

    /// <summary>Playing it does nothing; the effect happens when drawn (StS1 triggerWhenDrawn).</summary>
    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;

    public override async Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;
        await CommonActions.ApplySelf<TrinePower>(ctx, this, 1);
    }
}
