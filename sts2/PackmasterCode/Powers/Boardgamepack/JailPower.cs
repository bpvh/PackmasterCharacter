// Ported from src/main/java/thePackmaster/powers/boardgamepack/JailPower.java
namespace Packmaster.PackmasterCode.Powers.Boardgamepack;

/// <summary>You can't play Attacks or take attack damage. Lasts [Amount] turn(s) (removed at the start of your turn).</summary>
public sealed class JailPower : PackmasterPower
{
    public override PowerType Type => PowerType.None; // StS1 NEUTRAL
    public override PowerStackType StackType => PowerStackType.Counter;

    // StS1 zeroed NORMAL and THORNS damage. StS2 attack damage carries Move; unpowered damage (thorns-like) is
    // left alone, as is HP loss.
#if OLD_STS2_API
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
#else
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
#endif
    {
        return target == Owner && props.HasFlag(ValueProp.Move) ? 0m : 1m;
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType) =>
        !(IsOwnersCard(card) && card.Type == CardType.Attack);

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        if (Amount <= 1) await PowerCmd.Remove(this);
        else await Util.Wiz.ModifyPowerAmount(this, -1);
    }
}
