// Ported from src/main/java/thePackmaster/cards/alignmentpack/Quincunx.java
using Packmaster.PackmasterCode.Powers.Alignmentpack;

namespace Packmaster.PackmasterCode.Cards.Alignmentpack;

/// <summary>Ethereal. When drawn, ALL enemies lose 4 (+3) Strength this turn.</summary>
public sealed class Quincunx : global::Packmaster.PackmasterCode.Cards.Arcanapack.AbstractAstrologerCard
{
    public Quincunx() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVar("Magic", 4, 3);
        WithTip(typeof(QuincunxPower));
        WithKeyword(CardKeyword.Ethereal);
    }

    /// <summary>Playing it does nothing; the effect happens when drawn (StS1 triggerWhenDrawn).</summary>
    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;

    public override async Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;
        foreach (var enemy in CombatState?.HittableEnemies.ToList() ?? [])
            await CommonActions.Apply<QuincunxPower>(ctx, enemy, this, DynamicVars["Magic"].BaseValue);
    }
}
