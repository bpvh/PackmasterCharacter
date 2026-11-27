// Ported from src/main/java/thePackmaster/cards/prismaticpack/PrismaticBarrier.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Prismaticpack;

/// <summary>Gain 7 (+2) Block. If you played a card of another color this turn, gain 2 (+1) Plating and Exhaust this card.</summary>
public sealed class PrismaticBarrier : AbstractPrismaticCard
{
    public PrismaticBarrier() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        WithBlock(7, 2);
        WithVar("Magic", 2, 1);
        WithTip(typeof(PlatingPower));
    }

    protected override bool ShouldGlowGoldInternal => !IsCanonical && Wiz.PlayedOtherColorThisTurn(Owner);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        if (!Wiz.PlayedOtherColorThisTurn(Owner)) return;
        await CommonActions.ApplySelf<PlatingPower>(ctx, this, DynamicVars["Magic"].BaseValue);
        // StS1 ChangePlayedCardExhaustAction. VERIFY: the result pile is decided after OnPlay, so the keyword applies
        // to this play. Only this combat copy changes (it's exhausted anyway).
        CardCmd.ApplyKeyword(this, CardKeyword.Exhaust);
    }
}
