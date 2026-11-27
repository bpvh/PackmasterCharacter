# StS2 port: status and how to continue

Plan: [`../docs/sts2-port-plan.md`](../docs/sts2-port-plan.md). Build instructions: [`README.md`](README.md).

## Important: nothing here has run in the game yet

All C# was written in a cloud session without the game. It has been **compile-checked** against:

- BaseLib 3.4.8 (the real NuGet DLL, the version the manifest pins)
- GodotSharp 4.5.1 and Harmony 2.3.6 (NuGet)
- an **older** `sts2.dll` (assembly 0.1.0.0, early 2026), because no current one was available

So BaseLib calls are checked exactly, and game calls are checked against an old API. Where the old DLL and newer mods disagree, the code has an explicit branch:

```csharp
#if OLD_STS2_API      // defined only by tools/compile_check with -p:OldSts2Api=true
    old signature
#else                 // the real build: signature used by BaseLib 3.4.8 / newer mods
    current signature
#endif
```

The compile check passes with 0 errors and 0 warnings in that mode. The current-API branches can't be checked without a current `sts2.dll`. These are the places (all in `Util/Wiz.cs` and `Relics/HandyHaversack.cs`):

| Call | Old DLL | Current (used by the real build) |
|---|---|---|
| `PowerCmd.Apply<T>` (in `Wiz.ApplyPower`) | no choice context | `(ctx, target, amount, applier, source, silent)` (BaseLib 3.4.8 references this) |
| `CardPileCmd.AddGeneratedCardToCombat` (in `Wiz.MakeInHand` / `ShuffleIn`) | `(card, pile, bool addedByPlayer[, position])` | `(card, pile, player[, position])` (HeartMod, game 0.111) |
| first turn of combat (Handy Haversack) | `CombatState.RoundNumber` | `PlayerCombatState.TurnNumber` (HelloSpire, game 0.107.1) |

| `ModifyDamageAdditive` / `ModifyDamageMultiplicative` overrides | 5 parameters | trailing `CardPlay? cardPlay` (HeartMod, game 0.111) |

Other unverified signatures carry a `// VERIFY` comment.

Hook semantics checked by decompiling the reference DLL's `Hook` class (ilspycmd, scratch only, nothing committed): `ModifyDamageAdditive` returns the **delta** the game adds (return 0 when not applicable), `ModifyDamageMultiplicative` returns a **factor** (1 when not applicable), `ModifyHpLostAfterOsty` and `ModifyCardPlayCount` return the **new value**.

**First thing to do with the game installed:** `dotnet build` in `sts2/`. Expect a handful of signature fixes, mostly in `Util/Wiz.cs`. Then start a run and check the items under "Needs in-game verification" below.

## Done

| Area | State |
|---|---|
| Project | Alchyr character template (BaseLib), manifest pinned to BaseLib 3.4.8 / game 0.111.0 |
| Character | `ThePackmaster`: 75 HP, 99 gold, starter deck 4 Strike / 4 Defend / Rummage / Cardistry, Handy Haversack. Visuals are still base-game placeholders (`PlaceholderCharacterModel`) |
| Starter cards | Strike, Defend, Rummage, Cardistry (+ `CardistryPower`, a reimplementation of StS1 Rebound) |
| Relics | Handy Haversack (+ runs the pack draft, see below) |
| Pack framework | `Packs/`: `PackmasterPack`, `PackRegistry` (reflection, other mods can add packs), `PackRunState` (per-player, saved with the run via BaseLib `SavedSpireField`; add/remove/ban for Banishing Decree), `PackDraft`, `PackmasterConfig` (custom draft slots, All Packs, multiple None, packs per choice, disabled packs, skip draft) |
| Pool filter | `Patches/PackPoolPatches.cs`: postfix on `CardPoolModel.GetUnlockedCards` so rewards, shops and random-card effects only use the run's packs; player context from `CardCreationOptions.GetPossibleCards` |
| Packs ported (enabled) | 18 of 60: **Core Set**, **Strikes**, **Alignment**, **Prismatic**, **Arcana**, **Quiet**, **Bellord**, **Even/Odd**, **Rimworld**, **Ring of Pain**, **Blacksmiths** (upgrades), **Aggression**, **Board Game**, **Gems** (Jeweler's Kit), and the vanilla packs **Ironclad**, **Silent**, **Defect**, **Poison** |
| Vanilla packs | StS1 packs made of base-game cards use the StS2 base-game cards (`tools/vanilla_cards.json`, read by `gen_cards.py --pack-defs`). The pool filter adds them to the Packmaster's pool for the selected packs, respecting their own pool's unlocks. Bulwark, Status, All For One and Watcher stay disabled until their missing cards (StS2 doesn't have them) get substitutes or Packmaster reimplementations |
| Shared mechanics | Temporary HP (`TempHpPower`, via `ModifyHpLostAfterOsty`), StS1 keyword tooltips (`Util/PmTips.cs`, generated), pack relics (`Relics/PackRelic.cs`: only offered when the pack is in a Packmaster's run), multi-level upgrades (`MaxUpgradeLevel`, Ring of Pain Evolve, Blacksmiths), card modifiers (BaseLib `CardModifier`, Rimworld On Fire) |
| Rest site | `RestSite/EnhanceRestSiteOption.cs`: Enhance (socket a Gem for 25 Gold) as a free action; added by `PackmasterRunHooks` (a BaseLib run-hook singleton) when the deck has a Gem |
| Occult | `Mechanics/Occult.cs`: Occult cards are playable despite Unplayable/hooks/energy; played that way they Exhaust and spend at most your energy |
| Stances | `Stances/PackmasterStance.cs`: a stance is a single-instance buff power; `Stance.Enter<T>` / `Stance.Exit` run exit/enter effects and `IStanceListener` hooks, and track "changed stance this turn". No stance power = StS1 Neutral. Icons are drawn by `tools/make_stance_icons.py` (tracked) |
| Pack definitions | All 60 packs generated in `Packs/Definitions/`; only finished packs are `Enabled` |
| Localization | All 5 StS1 languages (eng, deu, kor, rus, zhs) converted to StS2 tables by `tools/convert_loc.py` |
| Art | `tools/sync_assets.py` copies card/power/relic art from `../src` into `Packmaster/images` (gitignored, run before building the .pck); character select / icon images are generated and tracked |

## Tools (`sts2/tools`, Python 3, tests: `python3 -m unittest discover -s tools/tests -t tools`)

- **`convert_loc.py`** converts the StS1 loc for all languages. Hand edits are never overwritten: it tracks a hash of what it generated (`loc_generated_keys.json`) and only regenerates untouched keys. `--report` writes a review list to `tools/out/loc_report.json`: power and relic text StS1 assembled in code from fragments, keyword lines stripped from cards, keyword translation alignment.
- **`gen_cards.py --pack <PackId>`** generates C# for a pack's cards. Constructor values, upgrades, keywords and tags are always generated. Simple `use()` statements (attack, block, draw, energy, vanilla powers) are translated; everything else becomes `// TODO port:` lines, with the Java kept as comments. It never overwrites existing files without `--force`. `--pack-defs` refreshes the pack definitions: a pack is enabled once all its cards exist and none still carries the `Status: skeleton` header. `--list` shows per-pack status.
- **`check_loc.py`** checks that every C# card, power and relic has its English title and description, and that card text only uses variables the card declares. Run it after porting a pack; `--all` includes skeletons.
- **`sync_assets.py`** copies art (see above).
- **`compile_check/`** is a csproj that type-checks `PackmasterCode/` without Godot or the game, given paths to `sts2.dll`, BaseLib, GodotSharp and Harmony.

## Porting a pack (the loop)

1. `python3 tools/gen_cards.py --pack StrikesPack` writes skeletons to `PackmasterCode/Cards/<javapackage>/`.
2. For each card, port the `// TODO port:` lines from the Java comments, using `Util/Wiz.cs` and BaseLib `CommonActions`. Delete the auto-generated header when the card is done.
3. Port the pack's powers into `Powers/<javapackage>/` on `PackmasterPower`, and any orbs, relics and potions.
4. Fix the loc (`Packmaster/localization/eng/*.json`): card text uses the vars the card declares (`Damage`, `Block`, `Magic`, `Magic2`, ...). Power text from StS1 fragments needs rewriting, and the report lists them.
5. `python3 tools/gen_cards.py --pack-defs` enables the pack.
6. Compile-check, then test in game.

Conventions:
- StS1 `magicNumber` is var `Magic` and `secondMagic` is `Magic2`.
- Card art is looked up by the StS1 ID name (`ArtId`).
- Loc keys are `PACKMASTER-<UPPER_SNAKE class name>`.
- Name power classes after the StS1 `POWER_ID`, so converted loc lines up. If they can't be (the ID equals a card class, e.g. Animosity), add the entry to `tools/loc_renames.json` and rerun `convert_loc.py`: every language gets the renamed keys. `check_loc.py` finds missing ones.
- "Until end of turn" stat debuffs use BaseLib `CustomTemporaryPowerModelWrapper<Card, StrengthPower>` with `InvertInternalPowerAmount`.
- Replaying a card uses `ModifyCardPlayCount` / `AfterModifyingCardPlayCount` (vanilla Burst pattern).
- "When drawn" cards override `AfterCardDrawn` and check `card == this`.
- End-of-turn hand effects use `BeforeFlush`.

## Needs in-game verification

- **Pack draft timing.** It runs at the start of the first turn of the first combat (from the starter relic), using the game's card-select grid with one preview card per pack. StS1 drafted before Neow. Moving it earlier needs a confirmed run-start/Neow hook. Until the draft runs, the pool already contains the fixed and random packs.
- **Pool filter coverage:** card rewards, shops, events and transforms. If a source bypasses `GetUnlockedCards`, patch it too.
- **Card-select grid with preview cards** (`ToMutable()` copies of canonical preview cards; may need `CombatState.CreateCard`).
- **Co-op:** each Packmaster resolves packs from (run seed, NetId) and uses the default slots; draft picks go through `PlayerChoiceContext`.
- **SavedSpireField save/load** of pack state mid-run.
- **CardistryPower** moves the played card to the top of the draw pile in `AfterCardPlayed`; check the card has already reached the discard pile at that point.
- **Dice** (`Powers/Boardgamepack/DicePower.cs`): the card that uses Dice is chosen in `BeforeCardPlayed` and the Dice it saw are removed in `AfterCardPlayed`, so Dice rolled by the card itself are kept (StS1 order). Check that block/damage previews include Dice.
- **Chance** cards (Go to Jail) auto-play from `AfterCardDrawn` although Unplayable; **Skip Next Turn** ends the turn from `AfterPlayerTurnStart`.
- **Enhance**: OnSelect returns false after socketing so other options stay (StS1 free action); check the rest site UI and co-op sync. Socketed gems are BaseLib card modifiers (saved with the card); Retain/Persist are re-applied on load.
- **Occult** patches `CardModel.CanPlay(out, out)` and the private `CardModel.SpendEnergy`.
- **MayhemFormPower** has no hand-refresh hook in StS2, so it checks after turn-start draw, play, discard and exhaust.

## Design decisions taken (review)

- **Choice Cut** (Prismatic) offered Uppercut, Doom and Gloom, Predator and Reach Heaven. Doom and Gloom and Reach Heaven don't exist in StS2, so it offers Uppercut, Predator and Sweeping Beam.
- "Different color" (Prismatic) means a card outside the Packmaster's card pool, i.e. other characters' and colorless cards, as in StS1.
- **Penalty Strike**, **Quincunx**, **Lie** and **Decapitate** use StS2 temporary-Strength powers instead of StS1's -Strength plus GainStrengthPower pair; Inner Fury's temporary Strength is the same wrapper without inversion.
- **Vindicate** applies the vanilla StS2 Rage power (whenever you play an Attack this turn, gain Block), the same effect as StS1 Rage.
- **Vanilla pack substitutes** for StS1 cards StS2 doesn't have: Double Tap -> One-Two Punch, Choke -> Strangle, Sneaky Strike -> Flick-Flack, Doom and Gloom -> Null, Recursion -> Darkness, Catalyst -> Bubble Bubble, Crippling Cloud -> Haze, Corpse Explosion -> Corrosive Wave (reasons in `tools/vanilla_cards.json`). Retained cards use their StS2 numbers, not StS1's.
- **Aggression** multiplies damage with the `Move` flag (attacks), like StS1 NORMAL damage.

## Not started

- Phase 4: character animation (the StS1 skeleton is a DragonBones export with meshes and IK; it needs Spine 4.2 or a Godot rebuild), energy counter from the bag orb layers, rest-site and merchant scenes.
- 42 packs (see `gen_cards.py --list`). Pack-related relics (Booster Box, Booster Pack, Collection, Banishing Decree, Collector Badge, Gem of Unity, Bag of Holding as an Ancient upgrade), potions, Black Market Dealer event, top-bar packs button, pack summary star ratings in the draft.
