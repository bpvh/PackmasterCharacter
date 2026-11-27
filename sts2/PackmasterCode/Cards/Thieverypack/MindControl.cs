// Ported from src/main/java/thePackmaster/cards/thieverypack/MindControl.java and actions/thieverypack/MindControlAction.java
using Packmaster.PackmasterCode.Powers.Thieverypack;

namespace Packmaster.PackmasterCode.Cards.Thieverypack;

/// <summary>X cost. Choose an enemy with 12 (+3) times X or less HP: it attacks other enemies for the rest of combat. Exhaust.</summary>
public sealed class MindControl : AbstractThieveryCard
{
    public MindControl() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithVar("Magic", 12, 3);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(MindControlledPower));
    }

    protected override bool HasEnergyCostX => true;

    private decimal Limit(int energy) => DynamicVars["Magic"].BaseValue * energy;

    protected override bool IsPlayable =>
        Owner?.PlayerCombatState is { } s && CombatState?.HittableEnemies.Any(e => e.CurrentHp <= Limit(s.Energy)) == true;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var x = EnergyCost.CapturedXValue;
        if (play.Target == null) return;
        if (play.Target.CurrentHp > Limit(x))
        {
            // StS1 refused the play and kept the energy; StS2 has already spent it, so give it back.
            if (x > 0) await PlayerCmd.GainEnergy(x, Owner);
            return;
        }
        await CommonActions.Apply<MindControlledPower>(ctx, play.Target, this, 1);
    }
}
