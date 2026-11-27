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

    // ModifyDamageAdditive returns the delta (the game adds it). Game 0.111 (HeartMod) adds a CardPlay?
    // parameter that the older reference DLL lacks.
#if OLD_STS2_API
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
#else
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
#endif
    {
        return target == Owner && props.HasFlag(ValueProp.Move) ? Amount : 0m;
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
