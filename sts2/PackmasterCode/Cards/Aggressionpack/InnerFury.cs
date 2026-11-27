// Ported from src/main/java/thePackmaster/cards/aggressionpack/InnerFury.java
using Packmaster.PackmasterCode.Powers.Aggressionpack;
using Packmaster.PackmasterCode.Stances;
using Packmaster.PackmasterCode.Stances.Aggressionpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

/// <summary>Whenever you enter or exit Aggression, gain 4 (+2) temporary Strength and 4 (+2) Block. Enter Aggression.</summary>
public sealed class InnerFury : AbstractAggressionCard
{
    public InnerFury() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 4, 2);
        WithTip(PmTips.Aggression);
        WithTip(typeof(InnerFuryPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<InnerFuryPower>(ctx, this, DynamicVars["Magic"].BaseValue);
        await Stance.Enter<Aggression>(ctx, Owner.Creature, this);
    }
}
