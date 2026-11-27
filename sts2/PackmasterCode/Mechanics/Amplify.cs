// Ported from src/main/java/thePackmaster/cards/marisapack/AmplifyCard.java and patches/marisapack/AmplifyPatches.java
namespace Packmaster.PackmasterCode.Mechanics;

/// <summary>A card with an Amplify effect: if you have enough energy when it is played, pay the extra cost for more.</summary>
public interface IAmplifyCard
{
    /// <summary>Extra energy on top of the card's cost.</summary>
    int AmplifyCost { get; }
}

public static class Amplify
{
    public static int Cost(CardModel card) => card is IAmplifyCard a ? a.AmplifyCost : 0;

    /// <summary>Before playing (glow, previews): would the card be amplified if played now?</summary>
    public static bool WouldAmplify(CardModel card) =>
        card.Owner?.PlayerCombatState is { } state && state.Energy >= card.EnergyCost.GetResolved() + Cost(card);

    /// <summary>
    /// From the card's OnPlay (its own cost is already paid): pay the Amplify cost if you can. StS1 checked
    /// energy >= cost + amplify before paying the card, which is the same thing.
    /// </summary>
    public static async Task<bool> TryPay(CardModel card)
    {
        var state = card.Owner?.PlayerCombatState;
        var cost = Cost(card);
        if (state == null || state.Energy < cost) return false;
        if (cost > 0) await PlayerCmd.LoseEnergy(cost, card.Owner!);
        return true;
    }
}
