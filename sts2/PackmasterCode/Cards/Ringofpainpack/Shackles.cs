// Ported from src/main/java/thePackmaster/cards/ringofpainpack/Shackles.java
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Ringofpainpack;

/// <summary>2 cost. Retain. Gain 10 (+3 per upgrade, +4 the last) Block. Evolves when you play a card that costs 2 or more. Evolved: gain 1 Blur.</summary>
public sealed class Shackles : AbstractEvolveCard
{
    private const int EvolveCost = 2;

    public Shackles() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithBlock(10);
        WithVar("Magic", EvolveCost);
        WithVar(EvolvedVar, 0);
        WithKeyword(CardKeyword.Retain);
        WithTip(typeof(BlurPower));
        WithTip(Util.PmTips.Evolve);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(CurrentUpgradeLevel >= MaxUpgrades ? 4 : 3);
        base.OnUpgrade();
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        if (IsEvolved) await CommonActions.ApplySelf<BlurPower>(ctx, this, 1);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (InHand && card != this && card.Owner == Owner && !card.EnergyCost.CostsX && card.EnergyCost.GetResolved() >= EvolveCost) Evolve();
        return Task.CompletedTask;
    }
}
