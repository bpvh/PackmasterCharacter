// Ported from src/main/java/thePackmaster/cards/energyandechopack/Voices.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

/// <summary>Draw 3 (+1) cards. Echo. Exhaust.</summary>
public sealed class Voices : AbstractEchoCard
{
    public Voices() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Magic", 3, 1);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(PmTips.Echo);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardPileCmd.Draw(ctx, DynamicVars["Magic"].BaseValue, Owner);
        await Echo();
    }
}
