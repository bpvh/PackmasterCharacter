// Ported from src/main/java/thePackmaster/cards/utilitypack/Enrage.java
using Packmaster.PackmasterCode.Stances;
using Packmaster.PackmasterCode.Stances.Aggressionpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Utilitypack;

/// <summary>Discard 2 cards. Draw 2 (+1) cards. Enter Aggression.</summary>
public sealed class Enrage : AbstractUtilityCard
{
    public Enrage() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 2, 1);
        WithVar("Magic2", 2);
        WithTip(PmTips.Aggression);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Wiz.ChooseAndDiscard(ctx, this, DynamicVars["Magic2"].IntValue);
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
        await Stance.Enter<Aggression>(ctx, Owner.Creature, this);
    }
}
