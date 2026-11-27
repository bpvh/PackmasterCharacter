using System.Reflection;

namespace Packmaster.PackmasterCode.Packs;

/// <summary>
/// All known packs. Built lazily on first use (after ModelDb is initialized) by scanning every loaded assembly
/// for non-abstract PackmasterPack subclasses, so packs from other mods register themselves automatically.
/// Replaces StS1 SpireAnniversary5Mod.allPacks / packsByID / cardParentMap.
/// </summary>
public static class PackRegistry
{
    private static List<PackmasterPack>? _all;
    private static Dictionary<string, PackmasterPack> _byId = new();
    private static Dictionary<Type, List<PackmasterPack>> _byCardType = new();

    /// <summary>Every registered pack, including disabled (not yet ported) ones, ordered by PackId.</summary>
    public static IReadOnlyList<PackmasterPack> All
    {
        get
        {
            EnsureLoaded();
            return _all!;
        }
    }

    /// <summary>Packs that can be drafted.</summary>
    public static IEnumerable<PackmasterPack> Enabled => All.Where(p => p.Enabled && PackmasterConfig.IsPackAllowed(p.PackId));

    public static PackmasterPack? Get(string packId)
    {
        EnsureLoaded();
        return _byId.GetValueOrDefault(packId);
    }

    /// <summary>Packs containing this card (usually one; a few StS1 cards appear in more than one pack).</summary>
    public static IReadOnlyList<PackmasterPack> PacksOf(CardModel card)
    {
        EnsureLoaded();
        return _byCardType.TryGetValue(card.GetType(), out var packs) ? packs : [];
    }

    public static bool IsPackCard(CardModel card) => PacksOf(card).Count > 0;

    /// <summary>Register a pack explicitly (normally unnecessary; subclasses are discovered).</summary>
    public static void Register(PackmasterPack pack)
    {
        EnsureLoaded();
        Add(pack);
        _all!.Sort((a, b) => string.CompareOrdinal(a.PackId, b.PackId));
    }

    /// <summary>Canonical CardModel for a card class.</summary>
    public static CardModel CardFor(Type cardType) => ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));

    private static void EnsureLoaded()
    {
        if (_all != null) return;
        _all = [];
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (type.IsAbstract || !type.IsSubclassOf(typeof(PackmasterPack))) continue;
                if (type.GetConstructor(Type.EmptyTypes) == null) continue;
                try
                {
                    Add((PackmasterPack)Activator.CreateInstance(type)!);
                }
                catch (Exception e)
                {
                    MainFile.Logger.Error($"Could not create pack {type.FullName}: {e}");
                }
            }
        }

        _all.Sort((a, b) => string.CompareOrdinal(a.PackId, b.PackId));
        MainFile.Logger.Info($"Registered {_all.Count} packs ({_all.Count(p => p.Enabled)} enabled).");
    }

    private static void Add(PackmasterPack pack)
    {
        if (_byId.ContainsKey(pack.PackId))
        {
            MainFile.Logger.Error($"Duplicate pack id {pack.PackId}; ignoring {pack.GetType().FullName}");
            return;
        }

        _all!.Add(pack);
        _byId[pack.PackId] = pack;
        foreach (var cardType in pack.CardTypes)
        {
            if (!_byCardType.TryGetValue(cardType, out var list)) _byCardType[cardType] = list = [];
            list.Add(pack);
        }
    }
}
