// StSLib Temporary HP (AddTemporaryHPAction / TempHPField), which several packs use. StS2 has no equivalent.
namespace Packmaster.PackmasterCode.Powers;

/// <summary>
/// Temporary HP: absorbs HP loss after Block, then goes down by the amount absorbed. Lasts the whole combat.
/// Same shape as the vanilla Buffer power (ModifyHpLostAfterOsty + AfterModifyingHpLostAfterOsty).
/// </summary>
public sealed class TempHpPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string ArtId => "TempHP"; // no StS1 art; falls back to the placeholder

    private decimal _pendingAbsorb;

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || amount <= 0 || Amount <= 0) return amount;
        _pendingAbsorb = Math.Min(Amount, amount);
        return amount - _pendingAbsorb;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        if (_pendingAbsorb <= 0) return;
        var absorbed = _pendingAbsorb;
        _pendingAbsorb = 0;
        Flash();
        if (absorbed >= Amount) await PowerCmd.Remove(this);
        else await Util.Wiz.ModifyPowerAmount(this, -absorbed);
    }
}
