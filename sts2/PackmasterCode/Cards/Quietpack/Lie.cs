// Ported from src/main/java/thePackmaster/cards/quietpack/Lie.java
using Packmaster.PackmasterCode.Powers.Quietpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Quietpack;

/// <summary>0 cost. The enemy loses 4 (+2) Strength this turn. Add a Weight to your hand.</summary>
public sealed class Lie : AbstractQuietCard
{
    public Lie() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithVar("Magic", 4, 2);
        WithTip(typeof(LiePower));
        WithTip(typeof(Weight));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (play.Target != null)
            await CommonActions.Apply<LiePower>(ctx, play.Target, this, DynamicVars["Magic"].BaseValue);
        await Wiz.MakeInHand(Wiz.CreateCard<Weight>(Owner), Owner);
    }
}
