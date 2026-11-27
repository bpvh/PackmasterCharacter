// Ported from src/main/java/thePackmaster/powers/madsciencepack/PiratifyPower.java and PiratifyPowerUpp.java
using Packmaster.PackmasterCode.CardModifiers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Madsciencepack;

/// <summary>At the start of each turn, Modify [Amount] random card(s) in hand: "Gain a Cannonball."</summary>
public class PiratifyPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string ArtId => "PiratifyPower";

    protected virtual int CannonballLevel => 1;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        var hand = Wiz.Hand(player);
        var options = hand.Where(c => MadScience.Unmodified(c) && MadScience.Playable(c)).ToList();
        if (options.Count == 0) return;
        Flash();
        var rng = Wiz.CardRng(player);
        for (int i = 0; i < Amount && options.Count > 0; i++)
        {
            var c = rng.NextItem(options)!;
            options.Remove(c);
            CardModifier.AddModifier(c, MadScience.New<GainCannonballModifier>(CannonballLevel)());
        }
        await Task.CompletedTask;
    }
}

/// <summary>Piratify+: the Cannonballs are upgraded.</summary>
public sealed class PiratifyPowerUpp : PiratifyPower
{
    public override string ArtId => "PiratifyPowerUpp";
    protected override int CannonballLevel => 2;
}
