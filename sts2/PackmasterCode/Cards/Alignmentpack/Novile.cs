// Ported from src/main/java/thePackmaster/cards/alignmentpack/Novile.java

namespace Packmaster.PackmasterCode.Cards.Alignmentpack;

/// <summary>Ethereal. When drawn, gain 1 (+1) energy.</summary>
public sealed class Novile : global::Packmaster.PackmasterCode.Cards.Arcanapack.AbstractAstrologerCard
{
    public Novile() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVar("Magic", 1, 1);
        WithEnergyTip();
        WithKeyword(CardKeyword.Ethereal);
    }

    /// <summary>Playing it does nothing; the effect happens when drawn (StS1 triggerWhenDrawn).</summary>
    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;

    public override async Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;
        await PlayerCmd.GainEnergy(DynamicVars["Magic"].BaseValue, Owner);
    }
}
