// Ported from src/main/java/thePackmaster/cards/rimworldpack/Pyromaniac.java
using Packmaster.PackmasterCode.Powers.Rimworldpack;

namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>Power, 2 cost. At the start of your turn, set a random card in your hand On Fire. Upgrade: Innate.</summary>
public sealed class Pyromaniac : AbstractRimCard
{
    public Pyromaniac() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        WithKeyword(CardKeyword.Innate, UpgradeType.Add);
        WithTip(typeof(PyromaniacPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.ApplySelf<PyromaniacPower>(ctx, this, 1);
    }
}
