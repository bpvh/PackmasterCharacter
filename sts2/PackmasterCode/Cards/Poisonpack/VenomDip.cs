// Ported from src/main/java/thePackmaster/cards/poisonpack/VenomDip.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Powers.Poisonpack;

namespace Packmaster.PackmasterCode.Cards.Poisonpack;

/// <summary>This turn, whenever an Attack deals unblocked damage, apply 2 (+1) Poison.</summary>
public sealed class VenomDip : PackmasterCard
{
    public VenomDip() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 2, 1);
        WithTip(typeof(PoisonPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<ToxicDipPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
