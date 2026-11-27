// Ported from src/main/java/thePackmaster/cards/bellordpack/TimeOut.java
using Packmaster.PackmasterCode.Powers.Bellordpack;

namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>Gain 7 (+2) Block. Draw 1 card. If it's a Curse or Status, ALL enemies lose 1 (+1) Strength.</summary>
public sealed class TimeOut : AbstractBellordCard
{
    public TimeOut() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithBlock(7, 2);
        WithVar("Magic", 1, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        var drawn = await CardPileCmd.Draw(ctx, 1, Owner);
        if (!drawn.Any(IsCurseOrStatus) || CombatState == null) return;
        // StS1 applied permanent -Strength to all enemies.
        foreach (var enemy in CombatState.HittableEnemies.ToList())
            await CommonActions.Apply<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>(ctx, enemy, this, -DynamicVars["Magic"].BaseValue);
    }
}
