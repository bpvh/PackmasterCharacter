namespace Packmaster.PackmasterCode.Cards.Starter;

/// <summary>
/// StS1: 2 cost Attack. Deal 10 (+4) damage. Draw 1 card and reduce its cost by 1 this turn.
/// Vars: Damage, Cards (cards drawn, StS1 secondMagic), Reduction (cost reduction, StS1 magicNumber).
/// </summary>
public sealed class Rummage : PackmasterCard
{
    public const string ReductionVar = "Reduction";

    public Rummage() : base(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
        WithDamage(10, 4);
        WithCards(1);
        WithVar(ReductionVar, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_blunt").Execute(ctx);

        var drawn = await CommonActions.Draw(this, ctx);
        var reduction = DynamicVars[ReductionVar].IntValue;
        foreach (var card in drawn)
        {
            // StS1 only reduced cards with a non-negative cost that currently cost more than 0 (X and unplayable cards are skipped).
            if (card.EnergyCost.CostsX) continue;
            var current = card.EnergyCost.GetResolved();
            if (current <= 0) continue;
            // VERIFY: SetThisTurn(int, bool reduceOnly) is the CardEnergyCost API mirrored by BaseLib's ICustomResourceCost.
            card.EnergyCost.SetThisTurn(Math.Max(0, current - reduction), reduceOnly: true);
        }
    }
}
