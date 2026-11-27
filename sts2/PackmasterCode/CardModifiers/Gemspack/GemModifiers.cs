// Ported from src/main/java/thePackmaster/cardmodifiers/gemspack/*.java
using BaseLib.Patches.Features;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Cards.Gemspack;
using Packmaster.PackmasterCode.Mechanics;
using Packmaster.PackmasterCode.Powers.Boardgamepack;
using Packmaster.PackmasterCode.Powers.Gemspack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.CardModifiers.Gemspack;

/// <summary>
/// StS1 ISCARDMODIFIED: a permanent modification of a deck card. A card holds at most one (Gems, Mad Science).
/// </summary>
public interface IModifiesCard;

/// <summary>
/// A Gem socketed into a card at a rest site (StS1 AbstractMadScienceModifier subclasses). Saved with the card by
/// BaseLib. Play effects run after the card's own effects; static effects (keywords) are re-applied whenever the
/// modifier is attached, copied or loaded, because StS2 doesn't save card keywords.
/// </summary>
public abstract class GemModifier : CardModifier, ICustomModel, IModifiesCard
{
    public override bool ShouldReceiveCombatHooks => true;

    /// <summary>Text goes before the card text (Retain-like keywords) instead of after it.</summary>
    protected virtual bool TextFirst => false;

    /// <summary>The effect of the gem, shared with the gem card's own play.</summary>
    public virtual Task Effect(PlayerChoiceContext ctx, Player player) => Task.CompletedTask;

    protected virtual void ApplyStatic(CardModel card) { }

    public override void OnInitialApplication()
    {
        if (Owner != null) ApplyStatic(Owner);
    }

    public override void LoadSaveData(ModifierSave save)
    {
        if (Owner != null) ApplyStatic(Owner);
    }

    public override void ModifyDescription(Creature? target, ref string description)
    {
        var text = GetLoc().GetFormattedText();
        if (string.IsNullOrEmpty(text)) return;
        description = TextFirst ? text + "\n" + description : description + "\n" + text;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Owner is { } player) await Effect(choiceContext, player);
    }

    public static bool IsModified(CardModel card) => Wiz.Modifiers(card).Any(m => m is IModifiesCard);
}

/// <summary>Onyx: reduce your debuffs by 1 (StS1 ReduceDebuffsAction).</summary>
public sealed class CleanseGemModifier : GemModifier
{
    public override Task Effect(PlayerChoiceContext ctx, Player player) => ReduceDebuffs(player.Creature, 1);

    public static async Task ReduceDebuffs(Creature c, int by)
    {
        foreach (var power in c.Powers.Where(p => p.Type == PowerType.Debuff).ToList())
        {
            if (power.Amount > 0)
            {
                if (power.Amount <= by) await PowerCmd.Remove(power);
                else await Wiz.ModifyPowerAmount(power, -by);
            }
            else if (power.Amount < 0)
            {
                if (-power.Amount <= by) await PowerCmd.Remove(power);
                else await Wiz.ModifyPowerAmount(power, by);
            }
        }
    }
}

/// <summary>Tourmaline: roll a D6.</summary>
public sealed class DiceGemModifier : GemModifier
{
    public override async Task Effect(PlayerChoiceContext ctx, Player player) => await Dice.Roll(ctx, player, 6);
}

/// <summary>Opal: draw 1 card.</summary>
public sealed class DrawGemModifier : GemModifier
{
    public override Task Effect(PlayerChoiceContext ctx, Player player) => CardPileCmd.Draw(ctx, 1, player);
}

/// <summary>Citrine: gain 1 energy next turn (StS1 Energized).</summary>
public sealed class EnergyGemModifier : GemModifier
{
    public override async Task Effect(PlayerChoiceContext ctx, Player player) =>
        await Wiz.ApplyPower<EnergyNextTurnPower>(ctx, player.Creature, 1, player.Creature);
}

/// <summary>Aquamarine: channel 1 Frost.</summary>
public sealed class FrostGemModifier : GemModifier
{
    public override async Task Effect(PlayerChoiceContext ctx, Player player) => await OrbCmd.Channel<FrostOrb>(ctx, player);
}

/// <summary>Fragmented Gem: channel 1 Lightning.</summary>
public sealed class LightningGemModifier : GemModifier
{
    public override async Task Effect(PlayerChoiceContext ctx, Player player) => await OrbCmd.Channel<LightningOrb>(ctx, player);
}

/// <summary>Ruby: gain 2 temporary Strength.</summary>
public sealed class TempStrengthGemModifier : GemModifier
{
    public const int Strength = 2;

    public override async Task Effect(PlayerChoiceContext ctx, Player player) =>
        await Wiz.ApplyPower<TempStrengthGemPower>(ctx, player.Creature, Strength, player.Creature);
}

/// <summary>Amber: the card gains Retain.</summary>
public sealed class RetainGemModifier : GemModifier
{
    protected override void ApplyStatic(CardModel card)
    {
        if (!card.Keywords.Contains(CardKeyword.Retain)) card.AddKeyword(CardKeyword.Retain);
    }

    // Retain renders as a keyword; no extra text.
    public override void ModifyDescription(Creature? target, ref string description) { }
}

/// <summary>Lapis Lazuli: the card is Occult.</summary>
public sealed class OccultGemModifier : GemModifier, IGrantsOccult
{
    protected override bool TextFirst => true;
}

/// <summary>Garnet: the card has Persist 2 (BaseLib Persist: returns to hand the first 2 times it is played each turn).</summary>
public sealed class PersistGemModifier : GemModifier
{
    public const int Persist = 2;

    // BaseLib reads Persist from a card's PersistVar; a modifier can't add one, so extend IsPersist.
    [HarmonyPatch(typeof(PersistPatch), nameof(PersistPatch.IsPersist))]
    private static class IsPersistPatch
    {
        private static void Postfix(CardModel card, ref bool __result)
        {
            if (__result || !Wiz.Modifiers(card).Any(m => m is PersistGemModifier)) return;
            __result = BaseLib.Cards.Variables.PersistVar.PersistCount(card, Persist) > 0;
        }
    }
}
