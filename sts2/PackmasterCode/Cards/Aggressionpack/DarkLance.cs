// Ported from src/main/java/thePackmaster/cards/aggressionpack/DarkLance.java
using Packmaster.PackmasterCode.Stances;

namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

/// <summary>Deal 11 (+3) damage. If you've changed Stances this turn, draw 2 (+1) cards.</summary>
public sealed class DarkLance : AbstractAggressionCard
{
    public DarkLance() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(11, 3);
        WithVar("Magic", 2, 1);
    }

    protected override bool ShouldGlowGoldInternal => Owner?.Creature != null && Stance.ChangedThisTurn(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        if (Stance.ChangedThisTurn(Owner.Creature))
            await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
    }
}
