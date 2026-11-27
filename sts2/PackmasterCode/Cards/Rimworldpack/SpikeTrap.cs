// Ported from src/main/java/thePackmaster/cards/rimworldpack/SpikeTrap.java
using Packmaster.PackmasterCode.Powers.Rimworldpack;

namespace Packmaster.PackmasterCode.Cards.Rimworldpack;

/// <summary>2 cost. Gain 12 (+4) Block. The next time an enemy attacks you, deal that much damage to it.</summary>
public sealed class SpikeTrap : AbstractRimCard
{
    public SpikeTrap() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        WithBlock(12, 4);
        WithTip(typeof(SpikeTrapPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var gained = await CommonActions.CardBlock(this, play);
        await CommonActions.ApplySelf<SpikeTrapPower>(ctx, this, gained);
    }
}
