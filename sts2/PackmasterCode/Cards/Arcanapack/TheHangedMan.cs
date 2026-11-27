// Ported from src/main/java/thePackmaster/cards/arcanapack/TheHangedMan.java
using Packmaster.PackmasterCode.Powers.Arcanapack;

namespace Packmaster.PackmasterCode.Cards.Arcanapack;

/// <summary>0 cost. This turn, whenever you draw a card, ALL enemies lose 4 (+2) HP.</summary>
public sealed class TheHangedMan : AbstractAstrologerCard
{
    public TheHangedMan() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVar("Magic", 4, 2);
        WithTip(typeof(HangedPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<HangedPower>(ctx, this, DynamicVars["Magic"].BaseValue);
    }
}
