// Ported from src/main/java/thePackmaster/cards/coresetpack/Showoff.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Coresetpack;

/// <summary>
/// Gain !B! Block.
/// Next turn, draw 2 additional cards.
/// </summary>
public sealed class Showoff : global::Packmaster.PackmasterCode.Cards.PackmasterCard
{
    public Showoff() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithBlock(13, 4);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await CommonActions.Apply<DrawCardsNextTurnPower>(ctx, Owner.Creature, this, 2m);
    }
}
