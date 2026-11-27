// Ported from src/main/java/thePackmaster/cards/prismaticpack/ManyAsOne.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Prismaticpack;

/// <summary>2 cost. Deal 10 (+4) damage. Put every card of another color from your discard pile into your hand.</summary>
public sealed class ManyAsOne : AbstractPrismaticCard
{
    public ManyAsOne() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(10, 4);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
        foreach (var card in Wiz.DiscardPile(Owner).Where(c => Wiz.IsOtherColor(c, Owner)).ToList())
            await CardPileCmd.Add(card, PileType.Hand);
    }
}
