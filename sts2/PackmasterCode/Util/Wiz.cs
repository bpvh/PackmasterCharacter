using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Random;
using Packmaster.PackmasterCode.Packs;

namespace Packmaster.PackmasterCode.Util;

/// <summary>
/// StS2 counterparts of the StS1 helpers most cards used (thePackmaster.util.Wiz, AbstractPackmasterCard,
/// SpireAnniversary5Mod.getRandomPacks/getRandomCardFromPack, FlexibleDiscoveryAction). Keeping the names close
/// to the Java ones makes ports easier to review side by side.
///
/// Game API calls whose signature changed between StS2 versions are wrapped here, so a future game update
/// needs one fix instead of hundreds. OLD_STS2_API is only defined by tools/compile_check (old reference DLL);
/// the real build always uses the current API.
/// </summary>
public static class Wiz
{
    // ---- piles ---------------------------------------------------------------------------------------

    public static IReadOnlyList<CardModel> Hand(Player p) => p.PlayerCombatState?.Hand.Cards ?? [];
    public static IReadOnlyList<CardModel> DrawPile(Player p) => p.PlayerCombatState?.DrawPile.Cards ?? [];
    public static IReadOnlyList<CardModel> DiscardPile(Player p) => p.PlayerCombatState?.DiscardPile.Cards ?? [];
    public static IReadOnlyList<CardModel> ExhaustPile(Player p) => p.PlayerCombatState?.ExhaustPile.Cards ?? [];

    // ---- randomness ----------------------------------------------------------------------------------

    /// <summary>StS1 AbstractDungeon.cardRandomRng, for in-combat card generation.</summary>
    public static Rng CardRng(Player p) => p.RunState.Rng.CombatCardGeneration;

    public static T? RandomItem<T>(IReadOnlyCollection<T> items, Rng rng) => items.Count == 0 ? default : rng.NextItem(items);

    // ---- packs ---------------------------------------------------------------------------------------

    public static PackmasterPack? PackOf(CardModel card) => PackRegistry.PacksOf(card).FirstOrDefault();

    /// <summary>
    /// StS1 AbstractPackmasterCard.otherPacksInHandCheck: number of different packs, other than this card's own,
    /// among the cards in hand.
    /// </summary>
    public static int OtherPacksInHand(CardModel card)
    {
        if (card.IsCanonical || card.Owner?.PlayerCombatState == null) return 0;
        var own = PackRegistry.PacksOf(card);
        return Hand(card.Owner)
            .SelectMany(PackRegistry.PacksOf)
            .Where(p => !own.Contains(p))
            .Distinct()
            .Count();
    }

    /// <summary>StS1 hasSynergy: cards from at least 2 other packs in hand.</summary>
    public static bool HasSynergy(CardModel card) => OtherPacksInHand(card) >= 2;

    /// <summary>StS1 getRandomPacks: distinct random packs from the run (onlyCurrent) or from every pack.</summary>
    public static List<PackmasterPack> RandomPacks(Player p, bool onlyCurrent, int count)
    {
        var pool = (onlyCurrent && PackRunState.IsPackmaster(p) ? PackRunState.Selected(p) : PackRegistry.Enabled).ToList();
        var result = new List<PackmasterPack>();
        var rng = CardRng(p);
        while (result.Count < count && pool.Count > 0)
        {
            var pick = rng.NextItem(pool)!;
            pool.Remove(pick);
            result.Add(pick);
        }
        return result;
    }

    /// <summary>StS1 getRandomCardFromPack: a random Common/Uncommon/Rare card of the pack (canonical model).</summary>
    public static CardModel? RandomCardFromPack(PackmasterPack pack, Player p)
    {
        var valid = pack.Cards.Where(c => c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare).ToList();
        return valid.Count == 0 ? null : CardRng(p).NextItem(valid);
    }

    // ---- card colors (StS1 card.color != player.getCardColor()) ------------------------------------------

    /// <summary>A card from outside the player's own pool (other characters' and colorless cards, as in StS1).</summary>
    public static bool IsOtherColor(CardModel card, Player p) => card.Pool != p.Character.CardPool;

    /// <summary>Cards the player finished playing this combat (StS1 cardsPlayedThisCombat), oldest first.</summary>
    public static IEnumerable<CardModel> CardsPlayedThisCombat(Player p) =>
        CombatManager.Instance?.History.CardPlaysFinished
            .Where(e => e.CardPlay.Card.Owner == p)
            .Select(e => e.CardPlay.Card) ?? [];

    /// <summary>Cards the player finished playing this turn (StS1 cardsPlayedThisTurn).</summary>
    public static IEnumerable<CardModel> CardsPlayedThisTurn(Player p)
    {
        var state = p.Creature.CombatState;
        if (state == null || CombatManager.Instance == null) return [];
        return CombatManager.Instance.History.CardPlaysFinished
            .Where(e => e.CardPlay.Card.Owner == p && e.HappenedThisTurn(state))
            .Select(e => e.CardPlay.Card);
    }

    public static bool PlayedOtherColorThisTurn(Player p) => CardsPlayedThisTurn(p).Any(c => IsOtherColor(c, p));

    public static int OtherColorPlayedThisCombat(Player p) => CardsPlayedThisCombat(p).Count(c => IsOtherColor(c, p));

    /// <summary>
    /// StS1 PrismaticUtil.getRandomDifferentColorCardInCombat: random cards (with repeats) from other characters'
    /// pools and the colorless pool. type/rarity null = any non-Status/Curse type, any of Common/Uncommon/Rare.
    /// </summary>
    public static List<CardModel> RandomOtherColorCards(Player p, CardType? type, CardRarity? rarity, int count)
    {
        var pools = ModelDb.AllCharacterCardPools.Append(ModelDb.CardPool<ColorlessCardPool>()).ToHashSet();
        var valid = ModelDb.AllCards.Where(c =>
            pools.Contains(c.Pool) && IsOtherColor(c, p) && c.CanBeGeneratedInCombat &&
            (rarity == null ? c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare : c.Rarity == rarity) &&
            (type == null ? c.Type is not (CardType.Status or CardType.Curse) : c.Type == type)).ToList();
        var result = new List<CardModel>();
        if (valid.Count == 0) return result;
        var rng = CardRng(p);
        for (int i = 0; i < count; i++) result.Add(rng.NextItem(valid)!);
        return result;
    }

    // ---- powers --------------------------------------------------------------------------------------

    /// <summary>
    /// PowerCmd.Apply with the current signature (choice context first), as BaseLib 3.4.8 calls it. Prefer
    /// CommonActions.Apply from cards; use this from powers/relics, where there is no card.
    /// </summary>
    public static Task<T?> ApplyPower<T>(PlayerChoiceContext ctx, Creature target, decimal amount, Creature? applier,
        CardModel? source = null) where T : PowerModel
#if OLD_STS2_API
        => PowerCmd.Apply<T>(target, amount, applier, source, false);
#else
        => PowerCmd.Apply<T>(ctx, target, amount, applier, source, false);
#endif

    // ---- common card effects -------------------------------------------------------------------------

    /// <summary>StS1 DiscardAction(p, p, n, false): choose n cards in hand and discard them.</summary>
    public static async Task ChooseAndDiscard(PlayerChoiceContext ctx, CardModel source, int count)
    {
        var p = source.Owner;
        var inHand = Hand(p).Count;
        if (count <= 0 || inHand == 0) return;
        var n = Math.Min(count, inHand);
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, n);
        var chosen = (await CardSelectCmd.FromHand(ctx, p, prefs, null, source)).ToList();
        if (chosen.Count > 0) await CardCmd.Discard(ctx, chosen);
    }

    /// <summary>StS1 GamblingChipAction: discard any number of cards, then draw that many.</summary>
    public static async Task GamblingChip(PlayerChoiceContext ctx, CardModel source)
    {
        var p = source.Owner;
        var inHand = Hand(p).Count;
        if (inHand == 0) return;
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, inHand);
        var chosen = (await CardSelectCmd.FromHand(ctx, p, prefs, null, source)).ToList();
        if (chosen.Count == 0) return;
        await CardCmd.Discard(ctx, chosen);
        await CardPileCmd.Draw(ctx, chosen.Count, p);
    }

    /// <summary>StS1 AttackDamageRandomEnemyAction(card): one hit of the card's damage on a random enemy.</summary>
    public static async Task AttackRandomEnemy(PlayerChoiceContext ctx, CardModel card, CardPlay? play, string? vfx = null)
    {
        var state = card.CombatState;
        if (state == null) return;
        var cmd = DamageCmd.Attack(card.DynamicVars.Damage.BaseValue).FromCard(card).TargetingRandomOpponents(state);
        if (vfx != null) cmd.WithHitFx(vfx);
        await cmd.Execute(ctx);
    }

    // ---- creating cards ------------------------------------------------------------------------------

    /// <summary>A new combat card owned by p, from a canonical model (StS1 makeCopy / makeStatEquivalentCopy).</summary>
    public static CardModel CreateCard(CardModel canonical, Player p)
    {
        var state = p.Creature.CombatState ?? throw new InvalidOperationException("CreateCard needs an active combat");
        return state.CreateCard(canonical.IsCanonical ? canonical : ModelDb.GetById<CardModel>(canonical.Id), p);
    }

    public static CardModel CreateCard<T>(Player p) where T : CardModel => CreateCard(ModelDb.Card<T>(), p);

    /// <summary>StS1 makeInHand: add a generated card to the hand (goes to discard if the hand is full).</summary>
    public static async Task MakeInHand(CardModel card, Player p)
    {
        // Signature from current mods (card, pile, player); older game builds took (card, pile, bool addedByPlayer).
#if OLD_STS2_API
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true);
#else
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, p);
#endif
    }

    public static async Task ShuffleIn(CardModel card, Player p)
    {
#if OLD_STS2_API
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, true, CardPilePosition.Random);
#else
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, p, CardPilePosition.Random);
#endif
    }

    /// <summary>
    /// StS1 FlexibleDiscoveryAction: choose one of the given cards (canonical or not) and add a copy to your hand.
    /// Returns the added card, or null if skipped.
    /// </summary>
    public static async Task<CardModel?> Discover(PlayerChoiceContext ctx, Player p, IEnumerable<CardModel> options,
        bool costsZeroThisTurn, bool canSkip = false)
    {
        var choices = options.Select(c => CreateCard(c, p)).ToList();
        if (choices.Count == 0) return null;
        var picked = await CardSelectCmd.FromChooseACardScreen(ctx, choices, p, canSkip);
        if (picked == null) return null;
        if (costsZeroThisTurn) picked.EnergyCost.SetThisTurnOrUntilPlayed(0);
        await MakeInHand(picked, p);
        return picked;
    }
}
