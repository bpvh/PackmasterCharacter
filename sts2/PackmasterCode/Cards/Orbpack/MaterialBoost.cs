// Ported from src/main/java/thePackmaster/cards/orbpack/MaterialBoost.java
namespace Packmaster.PackmasterCode.Cards.Orbpack;

/// <summary>Discard any number of cards. Evoke your next orb once for each card discarded. Upgrade: Retain.</summary>
public sealed class MaterialBoost : AbstractOrbCard
{
    public MaterialBoost() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Retain, UpgradeType.Add);
        WithTip(StaticHoverTip.Evoke);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var discarded = await DiscardFromHand(ctx, 0, 99);
        var n = discarded.Count;
        if (n == 0 || Owner.PlayerCombatState!.OrbQueue.Orbs.Count == 0) return;
        for (int i = 0; i < n - 1; i++) await OrbCmd.EvokeNext(ctx, Owner, dequeue: false);
        await OrbCmd.EvokeNext(ctx, Owner);
    }
}
