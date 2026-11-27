// Ported from src/main/java/thePackmaster/cards/rimworldpack/Catharsis.java
using Packmaster.PackmasterCode.Powers.Rimworldpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>2 (1) cost. Shuffle a Despair into your draw pile. Your next card this turn is free and played twice. Exhaust.</summary>
public sealed class Catharsis : AbstractRimCard
{
    public Catharsis() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 1);
        WithKeyword(CardKeyword.Exhaust);
        WithCostUpgradeBy(-1);
        WithTip(typeof(CatharsisPower));
        WithTip(typeof(Despair));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Wiz.ShuffleIn(Wiz.CreateCard<Despair>(Owner), Owner);
        await CommonActions.ApplySelf<CatharsisPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
