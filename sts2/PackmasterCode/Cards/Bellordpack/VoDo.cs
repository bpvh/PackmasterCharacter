// Ported from src/main/java/thePackmaster/cards/bellordpack/VoDo.java
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>Power. Gain 2 (+1) Strength. The enemy loses 2 Strength. When obtained, add a Curse of the Bell to your deck.</summary>
public sealed class VoDo : AbstractBellordCard
{
    private const int EnemyStrengthLoss = 2;

    public VoDo() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithVar("Magic", 2, 1);
        WithTip(typeof(StrengthPower));
        WithTip(typeof(CurseOfTheBell));
    }

    protected override int CursesOnObtain => 1;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<StrengthPower>(ctx, this, DynamicVars["Magic"].BaseValue);
        if (play.Target != null) await CommonActions.Apply<StrengthPower>(ctx, play.Target, this, -EnemyStrengthLoss);
    }
}
