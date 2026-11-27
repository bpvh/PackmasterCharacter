// Ported from src/main/java/thePackmaster/powers/poisonpack/ToxicDipPower.java
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Poisonpack;

/// <summary>This turn, whenever you deal unblocked attack damage, apply [Amount] Poison (a one-turn Envenom).</summary>
public sealed class ToxicDipPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    // StS1 used the vanilla Envenom icon (loadRegion("envenom")); so does this. VERIFY the vanilla paths in game.
    public override string CustomPackedIconPath => ImageHelper.GetImagePath("atlases/power_atlas.sprites/envenom_power.tres");
    public override string CustomBigIconPath => ImageHelper.GetImagePath("powers/envenom_power.png");

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || target == Owner || !(props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered)) || result.UnblockedDamage <= 0) return;
        Flash();
        await Wiz.ApplyPower<PoisonPower>(choiceContext, target, Amount, Owner);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player) await PowerCmd.Remove(this);
    }
}
