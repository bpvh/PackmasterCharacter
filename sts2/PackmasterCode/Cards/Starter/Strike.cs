namespace Packmaster.PackmasterCode.Cards.Starter;

/// <summary>StS1: 1 cost Attack, deal 6 (+3) damage.</summary>
public sealed class Strike : PackmasterCard
{
    public Strike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithTags(CardTag.Strike);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
    }
}
