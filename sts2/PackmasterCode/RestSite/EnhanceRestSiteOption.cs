// Ported from src/main/java/thePackmaster/ui/EnhanceBonfireOption.java and vfx/gemspack/SocketGemEffect.java
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Cards.Gemspack;
using Packmaster.PackmasterCode.Extensions;

namespace Packmaster.PackmasterCode.RestSite;

/// <summary>
/// Enhance (Gems pack): spend 25 Gold to socket a Gem from your deck into an unmodified card that costs 1 or more.
/// A free action, as in StS1: OnSelect returns false, so the other rest site options stay available.
/// Shown when you have a Gem in your deck; disabled when you can't afford it or have no valid card.
/// </summary>
public sealed class EnhanceRestSiteOption : CustomRestSiteOption
{
    public const int GoldCost = 25;
    private const string Table = "packmaster_ui";
    private const string Entry = "PACKMASTER-ENHANCE_BONFIRE_OPTIONS";

    public EnhanceRestSiteOption(Player owner) : base(owner)
    {
        IsEnabled = CanSocket(owner);
    }

    public override string OptionId => "PACKMASTER_ENHANCE";

    public override string? CustomIconPath =>
        (IsEnabled ? "ui/rest_site/option_packmaster_enhance.png" : "ui/rest_site/option_packmaster_enhance_disabled.png").ImagePath();

    public override LocString Description
    {
        get
        {
            var loc = new LocString(Table, IsEnabled ? $"{Entry}.description" : $"{Entry}.descriptionDisabled");
            loc.Add("Gold", GoldCost);
            return loc;
        }
    }

    public static IEnumerable<AbstractGemsCard> Gems(Player p) => p.Deck.Cards.OfType<AbstractGemsCard>();

    public static bool CanReceiveGem(CardModel c) =>
        c is not AbstractGemsCard && c.Type != CardType.Curse && c.EnergyCost.Canonical >= 1 && !GemModifier.IsModified(c);

    public static bool CanSocket(Player p) =>
        p.Gold >= GoldCost && Gems(p).Any() && p.Deck.Cards.Any(CanReceiveGem);

    // VERIFY in co-op: a successful socket returns false (free action); the card choices go through CardSelectCmd.
    public override async Task<bool> OnSelect()
    {
        if (!CanSocket(Owner)) return false;

        var gemPrefs = new CardSelectorPrefs(new LocString(Table, $"{Entry}.text5"), 1) { Cancelable = true };
        var gem = (await CardSelectCmd.FromDeckGeneric(Owner, gemPrefs, c => c is AbstractGemsCard)).FirstOrDefault() as AbstractGemsCard;
        if (gem == null) return false;

        var targetPrefs = new CardSelectorPrefs(new LocString(Table, $"{Entry}.text6"), 1) { Cancelable = true };
        var target = (await CardSelectCmd.FromDeckGeneric(Owner, targetPrefs, CanReceiveGem)).FirstOrDefault();
        if (target == null) return false;

        CardModifier.AddModifier(target, gem.CreateModifier(true));
        if (gem.IsUpgraded) CardCmd.Downgrade(gem);
        else await CardPileCmd.RemoveFromDeck(gem);
        await PlayerCmd.LoseGold(GoldCost, Owner);
        IsEnabled = CanSocket(Owner);
        return false;
    }
}

/// <summary>Run-wide hook listener for Packmaster features that aren't tied to a card, relic or power.</summary>
public sealed class PackmasterRunHooks() : CustomSingletonModel(HookType.Run)
{
    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (!EnhanceRestSiteOption.Gems(player).Any()) return false;
        options.Add(new EnhanceRestSiteOption(player));
        return true;
    }
}
