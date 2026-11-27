// Ported from src/main/java/thePackmaster/cards/boardgamepack/GoToJail.java
using Packmaster.PackmasterCode.Powers.Boardgamepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Boardgamepack;

/// <summary>Chance (played when drawn). Unplayable. Go to Jail. Upgraded: gain 20 Gold. Exhaust.</summary>
public sealed class GoToJail : AbstractBoardCard
{
    public const int Gold = 20;

    public GoToJail() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithKeyword(CardKeyword.Unplayable);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(PmTips.Chance);
        WithTip(typeof(JailPower));
    }

    protected override bool IsChance => true;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<JailPower>(ctx, this, 1);
        if (IsUpgraded) await PlayerCmd.GainGold(Gold, Owner);
    }
}
