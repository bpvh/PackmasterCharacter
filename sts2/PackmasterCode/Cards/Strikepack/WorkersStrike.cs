// Ported from src/main/java/thePackmaster/cards/strikepack/WorkersStrike.java (+ actions/strikepack/WorkersStrikeAction.java)
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Strikepack;

/// <summary>Deal 6 (+3) damage. Put a random Attack from your draw pile into your hand. If it's a Strike, gain [E].</summary>
public sealed class WorkersStrike : AbstractStrikePackCard
{
    public WorkersStrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithTags(CardTag.Strike);
        WithEnergyTip();
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        var attacks = Wiz.DrawPile(Owner).Where(c => c.Type == CardType.Attack).ToList();
        if (attacks.Count == 0) return;
        var card = Wiz.CardRng(Owner).NextItem(attacks)!;
        await CardPileCmd.Add(card, PileType.Hand);
        if (card.Tags.Contains(CardTag.Strike)) await PlayerCmd.GainEnergy(1, Owner);
    }
}
