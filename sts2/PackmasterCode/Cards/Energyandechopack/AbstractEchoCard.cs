// Ported from src/main/java/thePackmaster/cards/energyandechopack/AbstractEchoCard.java and cardmodifiers/energyandechopack/EchoMod.java
using Packmaster.PackmasterCode.Packs;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

public abstract class AbstractEchoCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : PackmasterCard(cost, type, rarity, target)
{
    /// <summary>True for the Exhaust + Ethereal copy an Echo card makes; copies don't Echo again.</summary>
    public bool IsEchoCopy { get; private set; }

    /// <summary>
    /// StS1 EchoMod: when played, add a copy of this card (upgraded if this is) with Exhaust and Ethereal to your hand.
    /// Call at the end of OnPlay.
    /// </summary>
    protected async Task Echo()
    {
        if (IsEchoCopy) return;
        var copy = (AbstractEchoCard)Wiz.CreateCard(PackRegistry.CardFor(GetType()), Owner);
        if (IsUpgraded) Wiz.ForceUpgrade(copy);
        copy.IsEchoCopy = true;
        if (!copy.Keywords.Contains(CardKeyword.Exhaust)) copy.AddKeyword(CardKeyword.Exhaust);
        if (!copy.Keywords.Contains(CardKeyword.Ethereal)) copy.AddKeyword(CardKeyword.Ethereal);
        await Wiz.MakeInHand(copy, Owner);
    }
}
