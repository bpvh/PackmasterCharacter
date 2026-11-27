using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Packmaster.PackmasterCode.Character;
using Packmaster.PackmasterCode.Extensions;

namespace Packmaster.PackmasterCode.Cards;

/// <summary>
/// Base class for every Packmaster card.
///
/// Built on BaseLib's ConstructedCardModel: the constructor declares vars, keywords and upgrades
/// (WithDamage(6, 3), WithKeyword(CardKeyword.Exhaust), ...), which maps 1:1 onto the StS1
/// "baseDamage = X; upp() { upgradeDamage(Y); }" pattern. Override OnUpgrade only for upgrades
/// that can't be declared that way.
///
/// Art: StS1 art is keyed by the card's ID without prefix, e.g. images/cards/Rummage.png
/// (250x190) and Rummage_p.png (500x380). tools/sync_assets.py copies those to
/// Packmaster/images/card_portraits/{ArtId}.png and card_portraits/big/{ArtId}.png.
/// Missing art falls back to the template's placeholder card.png.
/// </summary>
[Pool(typeof(PackmasterCardPool))]
public abstract class PackmasterCard(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true)
    : ConstructedCardModel(cost, type, rarity, target, showInCardLibrary)
{
    /// <summary>StS1 card ID without the "anniv5:" prefix. Defaults to the class name, which the generator keeps identical.</summary>
    public virtual string ArtId => GetType().Name;

    public override string CustomPortraitPath => $"{ArtId}.png".BigCardImagePath();
    public override string PortraitPath => $"{ArtId}.png".CardImagePath();
    public override string BetaPortraitPath => $"{ArtId}.png".CardImagePath();
}
