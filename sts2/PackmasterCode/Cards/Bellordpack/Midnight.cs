// Ported from src/main/java/thePackmaster/cards/bellordpack/Midnight.java
using MegaCrit.Sts2.Core.Models.Cards;

namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>Gain 10 (+3) Block. Deal 10 (+3) damage. When obtained, add a Curse of the Bell to your deck.</summary>
public sealed class Midnight : AbstractBellordCard
{
    public Midnight() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(10, 3);
        WithBlock(10, 3);
        WithTip(typeof(CurseOfTheBell));
    }

    protected override int CursesOnObtain => 1;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
    }
}
