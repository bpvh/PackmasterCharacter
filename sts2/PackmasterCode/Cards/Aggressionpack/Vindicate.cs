// Ported from src/main/java/thePackmaster/cards/aggressionpack/Vindicate.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

/// <summary>Deal 7 (+1) damage. Whenever you play an Attack this turn, gain 3 (+2) Block (vanilla Rage).</summary>
public sealed class Vindicate : AbstractAggressionCard
{
    public Vindicate() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(7, 1);
        WithVar("Magic", 3, 2);
        WithTip(typeof(RagePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        await CommonActions.ApplySelf<RagePower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
