# The Packmaster for Slay the Spire 2

A port of the StS1 character mod in the parent folder (`../src`) to Slay the Spire 2. It is built on [BaseLib](https://github.com/Alchyr/BaseLib-StS2). The full plan is in [`../docs/sts2-port-plan.md`](../docs/sts2-port-plan.md), and progress is tracked in [`PORTING_STATUS.md`](PORTING_STATUS.md).

## Requirements

- .NET 9 SDK
- Slay the Spire 2 installed (the build references `sts2.dll` and `0Harmony.dll` from the game folder)
- BaseLib installed in the game's `mods/` folder (download `BaseLib.dll`, `.pck` and `.json` from its releases)
- To export the `.pck` (assets): MegaDot or Godot **4.5.1** Mono. A newer Godot produces a `.pck` the game won't load.

## Build

1. Copy `Directory.Build.props.example` to `Directory.Build.props` and set `GodotPath`. If your game isn't in the default Steam library, also set `Sts2Path`. The copy is gitignored.
2. `dotnet build` compiles the DLL and copies `Packmaster.dll` and `Packmaster.json` into `<game>/mods/Packmaster/`.
3. `dotnet publish` additionally runs a headless Godot export of `Packmaster.pck` into the same folder.
4. Launch the game with mods enabled. The Packmaster appears on character select.

Game logs are at `%APPDATA%\SlayTheSpire2\logs\godot.log` on Windows and `~/.local/share/SlayTheSpire2/logs/` on Linux.

## Layout

```
Packmaster.csproj, Packmaster.json, project.godot, export_presets.cfg
PackmasterCode/          C# (namespace Packmaster.PackmasterCode)
  MainFile.cs            mod initializer
  Character/             character model and card/relic/potion pools
  Packs/                 pack framework: PackmasterPack, PackRegistry, run state, pool filter
  Cards/ Powers/ Relics/ Potions/ Util/ Patches/
Packmaster/              Godot assets (res://Packmaster/...)
  localization/<lang>/   StS2 loc tables
  images/                card portraits, powers, relics, character UI
tools/                   Python porting tools (loc converter, card skeleton generator) and their tests
```
