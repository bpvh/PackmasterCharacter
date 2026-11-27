// Ported from src/main/java/thePackmaster/cards/aggressionpack/Decapitate.java
// StS1 applied -Strength plus GainStrengthPower; this uses a temporary Strength loss (see Penalty Strike).
using Packmaster.PackmasterCode.Powers.Aggressionpack;

namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

/// <summary>Deal 10 (+3) damage. The enemy loses 4 (+2) Strength this turn. Exhaust.</summary>
public sealed class Decapitate : AbstractAggressionCard
{
    public Decapitate() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(10, 3);
        WithVar("Magic", 4, 2);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(DecapitatePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        if (play.Target is { IsDead: false })
            await CommonActions.Apply<DecapitatePower>(ctx, play.Target, this, DynamicVars["Magic"].BaseValue);
    }
}
