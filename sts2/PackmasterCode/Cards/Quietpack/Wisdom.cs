// Ported from src/main/java/thePackmaster/cards/quietpack/Wisdom.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>0 cost. Gain 1 (+1) energy. Discard any number of cards, then draw that many. Exhaust.</summary>
public sealed class Wisdom : AbstractQuietCard
{
    public Wisdom() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 1, 1);
        WithKeyword(CardKeyword.Exhaust);
        WithEnergyTip();
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PlayerCmd.GainEnergy(DynamicVars["Magic"].BaseValue, Owner);
        await Wiz.GamblingChip(ctx, this);
    }
}
