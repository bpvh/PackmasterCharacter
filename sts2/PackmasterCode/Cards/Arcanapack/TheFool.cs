// Ported from src/main/java/thePackmaster/cards/arcanapack/TheFool.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Arcanapack;

/// <summary>
/// Innate.
/// Deal !D! damage.
/// Apply !M! Weak.
/// Exhaust.
/// </summary>
public sealed class TheFool : AbstractAstrologerCard
{
    public TheFool() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(3, 1);
        WithVar("Magic", 3, 1);
        WithKeyword(CardKeyword.Innate);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(WeakPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_blunt").Execute(ctx);
        await CommonActions.Apply<WeakPower>(ctx, play.Target!, this, DynamicVars["Magic"].BaseValue);
    }
}
