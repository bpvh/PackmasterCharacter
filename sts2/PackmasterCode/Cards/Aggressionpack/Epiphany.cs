// Ported from src/main/java/thePackmaster/cards/aggressionpack/Epiphany.java
using Packmaster.PackmasterCode.Stances;
using Packmaster.PackmasterCode.Stances.Aggressionpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

/// <summary>If you are in a Stance, exit it and draw 3 (+1) cards. Otherwise, enter Aggression.</summary>
public sealed class Epiphany : AbstractAggressionCard
{
    public Epiphany() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 3, 1);
        WithTip(PmTips.Aggression);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (Stance.IsNeutral(Owner.Creature))
        {
            await Stance.Enter<Aggression>(ctx, Owner.Creature, this);
            return;
        }
        // StS1 queued the stance change on top, so the exit (and its Strength) happened before the draw.
        await Stance.Exit(ctx, Owner.Creature);
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
    }
}
