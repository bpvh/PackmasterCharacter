// Ported from src/main/java/thePackmaster/cardmodifiers/madsciencepack/*.java
using MegaCrit.Sts2.Core.Models.Orbs;
using Packmaster.PackmasterCode.CardModifiers.Gemspack;
using Packmaster.PackmasterCode.Cards.Legacypack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.CardModifiers.Madsciencepack;

/// <summary>
/// Mad Science modification (StS1 AbstractMadScienceModifier): marks the card as Modified (one modification per
/// card, shared with Gems). Amount is the modifier's value. These are put on cards in combat.
/// </summary>
public abstract class MadScienceModifier : CardModifier, ICustomModel, IModifiesCard
{
    public override bool ShouldReceiveCombatHooks => true;

    /// <summary>Text appended to the card ("Draw 1 card."), from card_modifiers.json with {Amount}.</summary>
    protected virtual bool HasText => true;

    public override void ModifyDescription(Creature? target, ref string description)
    {
        if (!HasText) return;
        var loc = GetLoc();
        loc.Add("Amount", Amount);
        description = description + "\n" + loc.GetFormattedText();
    }
}

/// <summary>Add [Amount] Block (Fortify).</summary>
public sealed class AddBlockModifier : MadScienceModifier
{
    protected override bool HasText => false;
    public override decimal ModifyBaseBlockAdditive(decimal originalBlock, ValueProp props) => Amount;
}

/// <summary>Add [Amount] damage (Honing Strike).</summary>
public sealed class AddDamageModifier : MadScienceModifier
{
    protected override bool HasText => false;
    public override decimal ModifyBaseDamageAdditive(decimal originalDamage, ValueProp props) => Amount;
}

/// <summary>Costs 0 this combat (Cheapen).</summary>
public sealed class CheapenModifier : MadScienceModifier
{
    protected override bool HasText => false;

    public override void OnInitialApplication()
    {
        Owner?.EnergyCost.SetThisCombat(0, reduceOnly: true);
    }
}

/// <summary>Draw [Amount] card(s) (Improve).</summary>
public sealed class DrawCardModifier : MadScienceModifier
{
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Owner is { } p) await CardPileCmd.Draw(choiceContext, Amount, p);
    }
}

/// <summary>Costs 1 more this combat; played again (Expand). StS1 replayed an unmodified copy; StS2 plays it an extra time.</summary>
public sealed class ExpandModifier : MadScienceModifier
{
    public override void OnInitialApplication()
    {
        Owner?.EnergyCost.AddThisCombat(1);
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        card == Owner ? playCount + 1 : playCount;
}

/// <summary>Channel [Amount] Frost (Winterize).</summary>
public sealed class FrostOrbModifier : MadScienceModifier
{
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Owner is not { } p) return;
        for (int i = 0; i < Amount; i++) await OrbCmd.Channel<FrostOrb>(choiceContext, p);
    }
}

/// <summary>Gain [Amount] Block (Enhance).</summary>
public sealed class GainBlockModifier : MadScienceModifier
{
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Owner is { } p) await CreatureCmd.GainBlock(p.Creature, Amount, ValueProp.Unpowered, null);
    }
}

/// <summary>Add a Cannonball to your hand; Amount 2 = upgraded (Piratify).</summary>
public sealed class GainCannonballModifier : MadScienceModifier
{
    public bool Upgraded => Amount >= 2;

    public override void ModifyDescription(Creature? target, ref string description)
    {
        description = description + "\n" + GetLoc(Upgraded ? "descriptionUpgraded" : "description").GetFormattedText();
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Owner is not { } p) return;
        var ball = Wiz.CreateCard<Cannonball>(p);
        if (Upgraded) Wiz.ForceUpgrade(ball);
        await Wiz.MakeInHand(ball, p);
    }
}

/// <summary>
/// Play the Absorbed card (Attach): when this card is played, play a copy of the absorbed card; when this card is
/// Exhausted, the absorbed card returns to your hand and the modification ends.
/// </summary>
public sealed class PlayCardModifier : MadScienceModifier
{
    public CardModel? Absorbed { get; set; }

    public override void ModifyDescription(Creature? target, ref string description)
    {
        if (Absorbed == null) return;
        var loc = GetLoc();
        loc.Add("Card", Absorbed.Title);
        description = description + "\n" + loc.GetFormattedText();
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Absorbed == null || Owner?.Owner is null) return;
        // VERIFY: auto-playing a clone of an exhausted card, then removing it (StS1 purgeOnUse).
        var copy = Absorbed.CreateClone();
        await CardCmd.AutoPlay(choiceContext, copy, null);
        await CardPileCmd.RemoveFromCombat(copy);
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card != Owner || Absorbed == null || Owner.Owner is not { } p) return;
        var back = Absorbed.CreateClone();
        Absorbed = null;
        CardModifier.RemoveModifier(card, this);
        await Wiz.MakeInHand(back, p);
    }
}

/// <summary>StS1 FindCardForAddModifierAction / SimpleAddModifierAction.</summary>
public static class MadScience
{
    /// <summary>Cards that can still be Modified.</summary>
    public static bool Unmodified(CardModel c) => !GemModifier.IsModified(c);

    public static bool Playable(CardModel c) => !c.Keywords.Contains(CardKeyword.Unplayable);

    /// <summary>
    /// Put a fresh modifier from <paramref name="make"/> on up to <paramref name="count"/> unmodified cards of
    /// <paramref name="pile"/> passing <paramref name="filter"/>, chosen at random or by the player.
    /// </summary>
    public static async Task Modify(PlayerChoiceContext ctx, CardModel source, IReadOnlyList<CardModel> pile, int count,
        bool random, Func<CardModel, bool> filter, Func<CardModifier> make, bool fromHand)
    {
        var player = source.Owner;
        var options = pile.Where(c => c != source && Unmodified(c) && filter(c)).ToList();
        if (options.Count == 0 || count <= 0) return;
        List<CardModel> chosen;
        if (random)
        {
            chosen = [];
            var rng = Wiz.CardRng(player);
            while (chosen.Count < count && options.Count > 0)
            {
                var c = rng.NextItem(options)!;
                options.Remove(c);
                chosen.Add(c);
            }
        }
        else
        {
            var prefs = new CardSelectorPrefs(new MegaCrit.Sts2.Core.Localization.LocString("packmaster_ui", "PACKMASTER-ADD_MODIFIER_UI.text0"),
                Math.Min(count, options.Count));
            chosen = fromHand
                ? (await CardSelectCmd.FromHand(ctx, player, prefs, options.Contains, source)).ToList()
                : (await CardSelectCmd.FromSimpleGrid(ctx, options, player, prefs)).ToList();
        }
        foreach (var c in chosen) CardModifier.AddModifier(c, make());
    }

    public static Func<CardModifier> New<T>(int amount = 1) where T : CardModifier => () =>
    {
        var mod = BaseLib.Extensions.ModelDbExtensions.CardModifier<T>(true);
        mod.Amount = amount;
        return mod;
    };
}
