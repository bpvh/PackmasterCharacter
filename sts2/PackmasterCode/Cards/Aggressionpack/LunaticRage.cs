// Ported from src/main/java/thePackmaster/cards/aggressionpack/LunaticRage.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Aggressionpack;

/// <summary>Draw 2 (+1) cards. Gain 2 energy. Discard all non-Attack non-Status cards. Exhaust.</summary>
public sealed class LunaticRage : AbstractAggressionCard
{
    public const int Energy = 2;

    public LunaticRage() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithVar("Magic", 2, 1);
        WithKeyword(CardKeyword.Exhaust);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
        await PlayerCmd.GainEnergy(Energy, Owner);
        var toDiscard = Wiz.Hand(Owner).Where(c => c != this && c.Type != CardType.Attack && c.Type != CardType.Status).ToList();
        if (toDiscard.Count > 0) await CardCmd.Discard(ctx, toDiscard);
    }
}
