// Ported from src/main/java/thePackmaster/cards/coresetpack/BoosterTutor.java (+ actions/BoosterTutorAction.java)
using Packmaster.PackmasterCode.Packs;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Coresetpack;

/// <summary>Put 3 (+1) random non-Basic pack cards from your draw pile into your hand, each from a different pack.</summary>
public sealed class BoosterTutor : PackmasterCard
{
    public BoosterTutor() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 3, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var targets = Wiz.DrawPile(Owner)
            .Where(c => c.Rarity != CardRarity.Basic && PackRegistry.IsPackCard(c))
            .ToList();
        var rng = Wiz.CardRng(Owner);
        for (int i = 0; i < DynamicVars["Magic"].IntValue && targets.Count > 0; i++)
        {
            var card = rng.NextItem(targets)!;
            var packs = PackRegistry.PacksOf(card);
            await CardPileCmd.Add(card, PileType.Hand);
            targets.RemoveAll(c => PackRegistry.PacksOf(c).Any(packs.Contains));
        }
    }
}
