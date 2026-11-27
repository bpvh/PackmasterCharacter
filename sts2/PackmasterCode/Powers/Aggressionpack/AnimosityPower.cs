// Ported from src/main/java/thePackmaster/powers/aggressionpack/AnimosityPower.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Stances;
using Packmaster.PackmasterCode.Stances.Aggressionpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Aggressionpack;

/// <summary>When you enter, exit, or start your turn in Aggression, gain [Amount] Strength.</summary>
public sealed class AnimosityPower : PackmasterPower, IStanceListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || !Stance.IsIn<Aggression>(Owner)) return;
        await Trigger(choiceContext);
    }

    public async Task AfterStanceChanged(PlayerChoiceContext ctx, Type? oldStance, Type? newStance)
    {
        if (oldStance == typeof(Aggression) || newStance == typeof(Aggression)) await Trigger(ctx);
    }

    private async Task Trigger(PlayerChoiceContext ctx)
    {
        Flash();
        await Wiz.ApplyPower<StrengthPower>(ctx, Owner, Amount, Owner);
    }
}
