// Ported from src/main/java/thePackmaster/cards/rimworldpack/EatWithoutTable.java
using Packmaster.PackmasterCode.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>0 cost. Gain 4 (+2) Block and 3 (+1) Temporary HP. Add a Despair to your hand.</summary>
public sealed class EatWithoutTable : AbstractRimCard
{
    public EatWithoutTable() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(4, 2);
        WithVar("Magic", 3, 1);
        WithTip(typeof(TempHpPower));
        WithTip(typeof(Despair));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await Wiz.GainTempHp(ctx, Owner.Creature, DynamicVars["Magic"].BaseValue, Owner.Creature, this);
        await Wiz.MakeInHand(Wiz.CreateCard<Despair>(Owner), Owner);
    }
}
