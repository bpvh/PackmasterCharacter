// Ported from src/main/java/thePackmaster/cards/alignmentpack/Opposition.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Alignmentpack;

/// <summary>Ethereal. When drawn, a random enemy loses 13 (+4) HP.</summary>
public sealed class Opposition : global::Packmaster.PackmasterCode.Cards.Arcanapack.AbstractAstrologerCard
{
    public Opposition() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVar("Magic", 13, 4);
        WithKeyword(CardKeyword.Ethereal);
    }

    /// <summary>Playing it does nothing; the effect happens when drawn (StS1 triggerWhenDrawn).</summary>
    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;

    public override async Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;
        var enemies = CombatState?.HittableEnemies.ToList() ?? [];
        if (enemies.Count == 0) return;
        var target = Wiz.CardRng(Owner).NextItem(enemies)!;
        await CreatureCmd.Damage(ctx, target, DynamicVars["Magic"].BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
    }
}
