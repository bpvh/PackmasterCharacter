// Ported from src/main/java/thePackmaster/cards/bellordpack/SavedByTheBell.java
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Packmaster.PackmasterCode.Cards.Bellordpack;

/// <summary>2 (1) cost. Retain. Gain 1 Intangible. Exhaust. When obtained, add 2 Curses of the Bell to your deck.</summary>
public sealed class SavedByTheBell : AbstractBellordCard
{
    public SavedByTheBell() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        WithKeyword(CardKeyword.Retain);
        WithKeyword(CardKeyword.Exhaust);
        WithCostUpgradeBy(-1);
        WithTip(typeof(IntangiblePower));
        WithTip(typeof(CurseOfTheBell));
    }

    protected override int CursesOnObtain => 2;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<IntangiblePower>(ctx, this, 1);
    }
}
