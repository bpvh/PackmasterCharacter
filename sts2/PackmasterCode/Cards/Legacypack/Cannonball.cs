// Ported from src/main/java/thePackmaster/cards/legacypack/Cannonball.java
namespace Packmaster.PackmasterCode.Cards.Legacypack;

/// <summary>Deal 6 (+2) damage. Exhaust. (Legacy pack token; also made by Piratify.)</summary>
public sealed class Cannonball : PackmasterCard
{
    public Cannonball() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
    {
        WithDamage(6, 2);
        WithKeyword(CardKeyword.Exhaust);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_heavy_blunt").Execute(ctx);
    }
}
