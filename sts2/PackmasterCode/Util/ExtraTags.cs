using HarmonyLib;

namespace Packmaster.PackmasterCode.Util;

/// <summary>
/// Tags added to a card instance at runtime (StS1 cards did this with card modifiers, e.g. AddStrikeTagModifier).
/// StS2 computes CardModel.Tags from the class, so a getter postfix appends these. Copied when the card is
/// cloned; not saved, so use it for "this combat" effects.
/// </summary>
public static class ExtraTags
{
    private static readonly SpireField<CardModel, HashSet<CardTag>> Field =
        new SpireField<CardModel, HashSet<CardTag>>(() => [])
            .CopyOnClone((_, dst, tags) => Field!.Set(dst, [.. tags ?? []]));

    private static HashSet<CardTag> Get(CardModel card)
    {
        var tags = Field.Get(card);
        if (tags == null) Field.Set(card, tags = []);
        return tags;
    }

    public static void Add(CardModel card, CardTag tag) => Get(card).Add(tag);

    public static bool Has(CardModel card, CardTag tag) => card.Tags.Contains(tag);

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.Tags), MethodType.Getter)]
    private static class TagsPatch
    {
        private static void Postfix(CardModel __instance, ref IEnumerable<CardTag> __result)
        {
            var extra = Field.Get(__instance);
            if (extra is { Count: > 0 }) __result = __result.Union(extra).ToList();
        }
    }
}
