// Ported from src/main/java/thePackmaster/cards/coresetpack/Flick.java
namespace Packmaster.PackmasterCode.Cards.Coresetpack;

/// <summary>Deal 4 (+2) damage. Choose a card in your hand: deal this damage again once per energy it costs, then discard it.</summary>
public sealed class Flick : PackmasterCard
{
    public Flick() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(4, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);

        var chosen = await CommonActions.SelectSingleCard(this, SelectionScreenPrompt, ctx, PileType.Hand);
        if (chosen == null) return;
        var cost = chosen.EnergyCost.CostsX ? 0 : chosen.EnergyCost.GetResolved();
        if (cost > 0 && play.Target is { IsAlive: true })
            await CommonActions.CardAttack(this, play, hitCount: cost, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        await CardCmd.Discard(ctx, chosen);
    }
}
