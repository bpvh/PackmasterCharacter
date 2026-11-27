// Ported from src/main/java/thePackmaster/stances/aggressionpack/AggressionStance.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Extensions;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Stances.Aggressionpack;

/// <summary>Attacks deal 25% more damage. Upon exiting this Stance, gain 2 Strength.</summary>
public sealed class Aggression : PackmasterStance
{
    public const decimal DamageMultiplier = 1.25m;
    public const int StrengthOnExit = 2;

    public override string CustomPackedIconPath => "stances/Aggression32.png".ImagePath();
    public override string CustomBigIconPath => "stances/Aggression84.png".ImagePath();

    // ModifyDamageMultiplicative returns the factor (the game multiplies by it). Game 0.111 (HeartMod) adds a CardPlay?
    // parameter that the older reference DLL lacks.
#if OLD_STS2_API
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
#else
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
#endif
    {
        return dealer == Owner && props.HasFlag(ValueProp.Move) ? DamageMultiplier : 1m;
    }

    public override async Task OnExit(PlayerChoiceContext ctx)
    {
        await Wiz.ApplyPower<StrengthPower>(ctx, Owner, StrengthOnExit, Owner);
    }
}
