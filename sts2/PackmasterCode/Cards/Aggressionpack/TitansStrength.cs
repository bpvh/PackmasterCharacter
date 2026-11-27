// Ported from src/main/java/thePackmaster/cards/aggressionpack/TitansStrength.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

/// <summary>
/// Gain !B! Block.
/// Gain !M! Strength.
/// </summary>
public sealed class TitansStrength : AbstractAggressionCard
{
    public TitansStrength() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithBlock(14, 4);
        WithVar("Magic", 1);
        WithTip(typeof(StrengthPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await CommonActions.Apply<StrengthPower>(ctx, Owner.Creature, this, DynamicVars["Magic"].BaseValue);
    }
}
