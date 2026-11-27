// Ported from src/main/java/thePackmaster/potions/thieverypack/DivinePotion.java
using MegaCrit.Sts2.Core.Entities.Potions;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Potions;

/// <summary>Gain 8 Temporary HP. Only obtained from Witchcraft (StS1 PLACEHOLDER rarity), so Token here.</summary>
public sealed class DivinePotion : PackmasterPotion
{
    public const int TempHp = 8;

    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Amount", TempHp)];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await Wiz.GainTempHp(choiceContext, Owner.Creature, TempHp, Owner.Creature);
    }
}
