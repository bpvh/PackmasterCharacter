// Ported from src/main/java/thePackmaster/powers/ringofpainpack/RendPower.java
using MegaCrit.Sts2.Core.Combat;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Ringofpainpack;

/// <summary>Takes [Amount] more damage from attacks. At the end of each round, Rend is reduced by half (rounded up).</summary>
public sealed class RendPower : PackmasterPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string ArtId => "Rend"; // StS1 used the vanilla rupture icon

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return target == Owner && props.HasFlag(ValueProp.Move) ? amount + Amount : amount;
    }

    // StS1 atEndOfRound: after the enemies' turn.
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Enemy) return;
        var decay = (int)Math.Ceiling(Amount / 2m);
        if (decay >= Amount) await PowerCmd.Remove(this);
        else await Wiz.ModifyPowerAmount(this, -decay);
    }
}
