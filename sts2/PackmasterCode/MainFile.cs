using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace Packmaster.PackmasterCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Packmaster"; // Used for resource file paths (res://Packmaster/...)
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        // Config first so values are loaded before anything reads them.
        BaseLib.Config.ModConfigRegistry.Register(ModId, new Packs.PackmasterConfig());

        var assembly = Assembly.GetExecutingAssembly();

        // Code-side strings converted from StS1 UIstrings (tools/convert_loc.py).
        CustomLocTableManager.Register("packmaster_ui.json");

        Harmony harmony = new(ModId);
        harmony.PatchAll(assembly);
    }
}
