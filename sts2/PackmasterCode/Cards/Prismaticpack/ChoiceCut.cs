// Ported from src/main/java/thePackmaster/cards/prismaticpack/ChoiceCut.java (+ actions/prismaticpack/ChoiceCutAction.java)
using MegaCrit.Sts2.Core.Models.Cards;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Prismaticpack;

/// <summary>
/// 2 cost. Choose one of Uppercut, Predator or Sweeping Beam (upgraded if this is) to add to your hand. It costs 0 this
/// turn. Exhaust.
/// DESIGN: StS1 offered Uppercut, Doom and Gloom, Predator and Reach Heaven. Doom and Gloom and Reach Heaven don't exist
/// in StS2, so Defect's Sweeping Beam stands in and the Watcher option is dropped.
/// </summary>
public sealed class ChoiceCut : AbstractPrismaticCard
{
    public ChoiceCut() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithKeyword(CardKeyword.Exhaust);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        CardModel[] options = [ModelDb.Card<Uppercut>(), ModelDb.Card<Predator>(), ModelDb.Card<SweepingBeam>()];
        var cards = options.Select(c => Wiz.CreateCard(c, Owner)).ToList();
        if (IsUpgraded)
            foreach (var c in cards.Where(c => c.IsUpgradable)) CardCmd.Upgrade(c);
        var picked = await CardSelectCmd.FromChooseACardScreen(ctx, cards, Owner, canSkip: true);
        if (picked == null) return;
        picked.EnergyCost.SetThisTurnOrUntilPlayed(0);
        await Wiz.MakeInHand(picked, Owner);
    }
}
