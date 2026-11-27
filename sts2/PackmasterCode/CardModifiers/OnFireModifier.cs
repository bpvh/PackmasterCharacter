// Ported from src/main/java/thePackmaster/cardmodifiers/rimworldpack/OnFireModifier.java
namespace Packmaster.PackmasterCode.CardModifiers;

/// <summary>
/// On Fire [Amount]: the card Exhausts, and when played it is played [Amount] extra times. Each turn it ends in your
/// hand, [Amount] goes up by 1. Applying it again adds the amounts.
/// </summary>
public sealed class OnFireModifier : CardModifier, ICustomModel
{
    public override bool ShouldReceiveCombatHooks => true;

    public override bool ApplyStacked(CardModifier newApplied)
    {
        Amount += Math.Max(1, newApplied.Amount);
        return true;
    }

    public override void OnInitialApplication()
    {
        if (Amount <= 0) Amount = 1;
        if (Owner != null && !Owner.Keywords.Contains(CardKeyword.Exhaust)) CardCmd.ApplyKeyword(Owner, CardKeyword.Exhaust);
    }

    public override void ModifyDescription(Creature? target, ref string description)
    {
        description = GetLoc().GetFormattedText() + "\n" + description;
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        card == Owner ? playCount + Amount : playCount;

    public override Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != null && Owner.Owner == player && Owner.Pile?.Type == PileType.Hand) Amount++;
        return Task.CompletedTask;
    }
}
