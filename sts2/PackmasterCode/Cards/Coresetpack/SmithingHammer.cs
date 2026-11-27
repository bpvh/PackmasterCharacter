// Ported from src/main/java/thePackmaster/cards/coresetpack/SmithingHammer.java
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Coresetpack;

/// <summary>Deal 3 damage 2 (+1) times. Upgrade 2 random cards in your hand for the rest of combat.</summary>
public sealed class SmithingHammer : PackmasterCard
{
    public SmithingHammer() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(3);
        WithVar("Magic", 2, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, hitCount: DynamicVars["Magic"].IntValue).Execute(ctx);

        // StS1 UpgradeRandomCardAction twice: a random upgradable card in hand each time.
        var rng = Wiz.CardRng(Owner);
        for (int i = 0; i < 2; i++)
        {
            var upgradable = Wiz.Hand(Owner).Where(c => c.IsUpgradable).ToList();
            if (upgradable.Count == 0) break;
            CardCmd.Upgrade(rng.NextItem(upgradable)!);
        }
    }
}
