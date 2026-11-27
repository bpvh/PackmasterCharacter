// Ported from src/main/java/thePackmaster/cards/prismaticpack/SwordAndDagger.java
using MegaCrit.Sts2.Core.Models.Cards;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Prismaticpack;

/// <summary>Deal 7 damage. Add 1 (+1) Shiv to your hand.</summary>
public sealed class SwordAndDagger : AbstractPrismaticCard
{
    public SwordAndDagger() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(7);
        WithVar("Magic", 1, 1);
        WithTip(typeof(Shiv));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        for (int i = 0; i < DynamicVars["Magic"].IntValue; i++)
            await Wiz.MakeInHand(Wiz.CreateCard<Shiv>(Owner), Owner);
    }
}
