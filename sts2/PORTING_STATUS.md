# StS2 port: status and how to continue

Plan: [`../docs/sts2-port-plan.md`](../docs/sts2-port-plan.md). Build instructions: [`README.md`](README.md).

## Important: nothing here has run in the game yet

All C# was written in a cloud session without the game. It has been **compile-checked** against:

- BaseLib 3.4.8 (the real NuGet DLL, the version the manifest pins)
- GodotSharp 4.5.1 and Harmony 2.3.6 (NuGet)
- an **older** `sts2.dll` (assembly 0.1.0.0, early 2026), because no current one was available

So BaseLib calls are checked exactly, and game calls are checked against an old API. The remaining compile errors against that old DLL are all known version drift, where newer mods use the newer form:

| File | Call | Old DLL | Newer mods (Oct/Nov 2026) |
|---|---|---|---|
| `Relics/HandyHaversack.cs` | `Owner.PlayerCombatState.TurnNumber` | missing (old builds used `CombatState.RoundNumber`) | `PlayerCombatState.TurnNumber` (HelloSpire, game 0.107.1) |
| `Util/Wiz.cs` (`MakeInHand`, `ShuffleIn`) | `CardPileCmd.AddGeneratedCardToCombat(card, pile, player[, position])` | `(card, pile, bool addedByPlayer, position)` | `(card, pile, player)` (HeartMod, game 0.111) |

**First thing to do with the game installed:** `dotnet build` in `sts2/`. Expect a handful of signature fixes. Where possible, game calls go through BaseLib's `CommonActions` or `Util/Wiz.cs`, so each fix should be in one place. Then start a run and check the items under "Needs in-game verification" below.

## Done

| Area | State |
|---|---|
| Project | Alchyr character template (BaseLib), manifest pinned to BaseLib 3.4.8 / game 0.111.0 |
| Character | `ThePackmaster`: 75 HP, 99 gold, starter deck 4 Strike / 4 Defend / Rummage / Cardistry, Handy Haversack. Visuals are still base-game placeholders (`PlaceholderCharacterModel`) |
| Starter cards | Strike, Defend, Rummage, Cardistry (+ `CardistryPower`, a reimplementation of StS1 Rebound) |
| Relics | Handy Haversack (+ runs the pack draft, see below) |
| Pack framework | `Packs/`: `PackmasterPack`, `PackRegistry` (reflection, other mods can add packs), `PackRunState` (per-player, saved with the run via BaseLib `SavedSpireField`; add/remove/ban for Banishing Decree), `PackDraft`, `PackmasterConfig` (custom draft slots, All Packs, multiple None, packs per choice, disabled packs, skip draft) |
| Pool filter | `Patches/PackPoolPatches.cs`: postfix on `CardPoolModel.GetUnlockedCards` so rewards, shops and random-card effects only use the run's packs; player context from `CardCreationOptions.GetPossibleCards` |
| Packs ported | **Core Set** (10 cards + Mayhem Form power), enabled |
| Pack definitions | All 60 packs generated in `Packs/Definitions/`; only finished packs are `Enabled` |
| Localization | All 5 StS1 languages (eng, deu, kor, rus, zhs) converted to StS2 tables by `tools/convert_loc.py` |
| Art | `tools/sync_assets.py` copies card/power/relic art from `../src` into `Packmaster/images` (gitignored, run before building the .pck); character select / icon images are generated and tracked |

## Tools (`sts2/tools`, Python 3, tests: `python3 -m unittest discover -s tools/tests -t tools`)

- **`convert_loc.py`** converts the StS1 loc for all languages. Hand edits are never overwritten: it tracks a hash of what it generated (`loc_generated_keys.json`) and only regenerates untouched keys. `--report` writes a review list to `tools/out/loc_report.json`: power and relic text StS1 assembled in code from fragments, keyword lines stripped from cards, keyword translation alignment.
- **`gen_cards.py --pack <PackId>`** generates C# for a pack's cards. Constructor values, upgrades, keywords and tags are always generated. Simple `use()` statements (attack, block, draw, energy, vanilla powers) are translated; everything else becomes `// TODO port:` lines, with the Java kept as comments. It never overwrites existing files without `--force`. `--pack-defs` refreshes the pack definitions: a pack is enabled once all its cards exist and none still carries the `Status: skeleton` header. `--list` shows per-pack status.
- **`sync_assets.py`** copies art (see above).
- **`compile_check/`** is a csproj that type-checks `PackmasterCode/` without Godot or the game, given paths to `sts2.dll`, BaseLib, GodotSharp and Harmony.

## Porting a pack (the loop)

1. `python3 tools/gen_cards.py --pack StrikesPack` writes skeletons to `PackmasterCode/Cards/<javapackage>/`.
2. For each card, port the `// TODO port:` lines from the Java comments, using `Util/Wiz.cs` and BaseLib `CommonActions`. Delete the auto-generated header when the card is done.
3. Port the pack's powers into `Powers/<javapackage>/` on `PackmasterPower`, and any orbs, relics and potions.
4. Fix the loc (`Packmaster/localization/eng/*.json`): card text uses the vars the card declares (`Damage`, `Block`, `Magic`, `Magic2`, ...). Power text from StS1 fragments needs rewriting, and the report lists them.
5. `python3 tools/gen_cards.py --pack-defs` enables the pack.
6. Compile-check, then test in game.

Conventions: StS1 `magicNumber` is var `Magic` and `secondMagic` is `Magic2`. Card art is looked up by the StS1 ID name (`ArtId`). Loc keys are `PACKMASTER-<UPPER_SNAKE class name>`.

## Needs in-game verification

- **Pack draft timing.** It runs at the start of the first turn of the first combat (from the starter relic), using the game's card-select grid with one preview card per pack. StS1 drafted before Neow. Moving it earlier needs a confirmed run-start/Neow hook. Until the draft runs, the pool already contains the fixed and random packs.
- **Pool filter coverage:** card rewards, shops, events and transforms. If a source bypasses `GetUnlockedCards`, patch it too.
- **Card-select grid with preview cards** (`ToMutable()` copies of canonical preview cards; may need `CombatState.CreateCard`).
- **Co-op:** each Packmaster resolves packs from (run seed, NetId) and uses the default slots; draft picks go through `PlayerChoiceContext`.
- **SavedSpireField save/load** of pack state mid-run.
- **CardistryPower** moves the played card to the top of the draw pile in `AfterCardPlayed`; check the card has already reached the discard pile at that point.
- **MayhemFormPower** has no hand-refresh hook in StS2, so it checks after turn-start draw, play, discard and exhaust.

## Not started

- Phase 4: character animation (the StS1 skeleton is a DragonBones export with meshes and IK; it needs Spine 4.2 or a Godot rebuild), energy counter from the bag orb layers, rest-site and merchant scenes.
- 59 packs (see `gen_cards.py --list`), pack-related relics (Booster Box, Booster Pack, Collection, Banishing Decree, Collector Badge, Gem of Unity, Bag of Holding as an Ancient upgrade), potions, Black Market Dealer event, Enhance rest-site option, top-bar packs button, pack summary star ratings in the draft.
