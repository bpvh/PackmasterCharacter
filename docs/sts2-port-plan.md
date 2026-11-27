# The Packmaster: Slay the Spire 2 port plan

Scope: port the functional content of The Packmaster to Slay the Spire 2 (StS2). That means the character, the starter cards and relic, all booster packs and their cards, powers, orbs, potions and relics, the pack draft and card-pool system, the pack-related relics, potions, event and rewards, and the character animation.

Out of scope: hats, skins (including the Packmistress skin), the hat unlock system, the main-menu "expansion packs" button and FTUE, and victory confetti or glow cosmetics. The cards in the separate `Packmaster-Expansion-Packs` repo are also out of scope. This plan only leaves room for them (see Phase 8).

---

## 1. What StS2 character mods look like

### Toolchain

| | StS1 (this repo) | StS2 |
|---|---|---|
| Engine | libGDX (Java) | Godot 4.5 ("MegaDot" fork), C# / .NET 9 |
| Loader | ModTheSpire + BaseMod + StSLib | Built-in mod loader (`mods/<ModId>/` with `.dll`, `.pck`, `.json` manifest) |
| Common library | BaseMod / StSLib | **BaseLib** (`Alchyr.Sts2.BaseLib` on NuGet; also a required runtime mod) |
| Patching | SpirePatch | Harmony (`harmony.PatchAll`), plus BaseLib helpers for patching `async` methods |
| Assets | Loose files in the jar | Godot resources packed into a `.pck` (exported with MegaDot/Godot 4.5.1, or `BSchneppe.StS2.PckPacker` for simple assets) |
| Build | Maven | `dotnet build`, referencing `sts2.dll` and `0Harmony.dll` from the local game install |

The standard starting point is Alchyr's template (`dotnet new` from `Alchyr/ModTemplate-StS2`, "Character" option). It produces:

```
PackmasterSts2/
  Packmaster.csproj            Godot.NET.Sdk, refs sts2.dll + BaseLib NuGet
  Packmaster.json              manifest: id, version, min_game_version, deps [BaseLib]
  project.godot, export_presets.cfg
  PackmasterCode/              all C#
    MainFile.cs                [ModInitializer] -> Harmony.PatchAll
    Character/                 CharacterModel, CardPool, RelicPool, PotionPool
    Cards/ Powers/ Relics/ Potions/ ...
  Packmaster/                  all Godot assets (res://Packmaster/...)
    localization/eng/cards.json, powers.json, relics.json, characters.json, card_keywords.json, ...
    images/ scenes/ animations/
```

I read the template, BaseLib's source, and three existing character mods: STS2-Buu (stances, a secondary resource, 100+ cards), HeartMod (a Spine-animated character) and HelloSpire (a minimal character).

### Core API shapes (from those mods)

Cards are classes with constructor metadata, declarative dynamic vars, and an `async` play method:

```csharp
[Pool(typeof(PackmasterCardPool))]
public sealed class Strike() : PackmasterCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this)
            .Targeting(play.Target!).WithHitFx("vfx/vfx_attack_slash").Execute(ctx);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
```

Powers are `CustomPowerModel` subclasses with `Type`/`StackType` and hook overrides such as `AfterCardPlayed`, `AfterSideTurnStart`, `AfterTurnEnd`, `ModifyDamageMultiplicative` and `BeforeDamageReceived`. Commands like `PowerCmd.Apply<T>()` are awaited directly. There is no action queue.

Localization uses flat keys such as `"PACKMASTER-STRIKE.title"` and `".description": "Deal {Damage:diff()} damage."` with SmartFormat (plurals, conditionals) and BBCode-like tags (`[gold]Block[/gold]`).

Character visuals come from a `CustomVisualPath` scene containing a `SpineSprite` (spine-godot, Spine **4.2** runtime) or a plain Godot animation. `SetupCustomAnimationStates` maps the game's Idle/Hit/Attack/Cast/Dead/Relaxed states onto whatever animations exist. Missing ones fall back to Idle.

BaseLib already provides most of the StSLib-style plumbing Packmaster depends on:

| Packmaster uses (StS1) | BaseLib / StS2 equivalent |
|---|---|
| `CardModifierManager` (67 files) | `BaseLib.Abstracts.CardModifier` |
| `SpireField` (14 files), save wrapper | `SpireField<,>`, `SavedSpireField<,>` (persisted in run save) |
| `ScryAction` | `BaseLib.Commands.ScryCmd`, `IAfterScryed` hook |
| `AbstractOrb` / `channelOrb` | `CustomOrbModel` (StS2 has orbs natively via Defect) |
| Custom reward types | `CustomReward` |
| Campfire option (`EnhanceBonfireOption`) | `CustomRestSiteOption` |
| Custom card piles | `CustomPile` |
| Dynamic variables (`!anniv5:m2!`) | `DynamicVar` subclasses, `DynamicVarSource` |
| Keywords | `card_keywords.json` + `BaseLibKeywords`, auto-tooltips |
| Boss relic that replaces the starter (Bag of Holding) | Ancient options (`CustomAncientModel`, `AncientOption`) |
| SpireConfig | `BaseLib.Config` (in-game mod config UI) |
| Network-safe custom state | `CustomMessage` / `CustomTargetedMessage` (StS2 has co-op multiplayer) |
| `selectScreen`/grid select | `BaseLibCardSelectCmd`, `MultiPileCardSelect` |

### Differences that change the design

1. **No action queue.** Every `addToBot`/`addToTop` chain becomes sequential `await` code. Most cards get simpler. The exceptions are "do this after the queue empties" tricks and actions that inspect or reorder the queue, which need rethinking case by case.
2. **Co-op multiplayer.** Up to 4 players, each possibly a Packmaster with different packs. All randomness must use the run or player RNG streams (36 files use `cardRandomRng` today). All UI choices must go through `PlayerChoiceContext`, and global statics (`SpireAnniversary5Mod.currentPoolPacks`, `combatExhausts`, `cardsRippedThisTurn`, ...) must become per-player state.
3. **No Watcher and no stances.** StS2 characters are Ironclad, Silent, Regent, Necrobinder and Defect. Stances (used by Aggression, Hermit and Utility) must be rebuilt as powers, the way STS2-Buu does it. Watcher pack cards don't exist in the base game.
4. **Vanilla card identities changed.** Packs built from StS1 base-game cards (Ironclad, Silent, Defect, Watcher, Poison, Orb, Bulwark, All For One, Status) can't just reference IDs. Many of those cards were renamed, reworked or removed in StS2.
5. **No end-of-act boss relic swap.** Starter-relic upgrades come from Ancients instead.
6. **VFX and SFX.** 70 libGDX `AbstractGameEffect` classes and 74 `CardCrawlGame.sound` call sites have to become Godot scenes or particles, or be swapped for StS2's built-in hit VFX and FMOD events.

### Prior art: `mojimoon/Sts2Packmaster` (not used)

A StS2 Packmaster-style library already exists: PackmasterLib (https://github.com/mojimoon/Sts2Packmaster), Harmony-only and targeting game v0.111.0. **We're not depending on it or using its code.** The repo has no license, so its code is all rights reserved, and we want everything on BaseLib.

Its public docs are still useful as evidence of how the game works. Facts about the game aren't copyrightable, but its code is, so we write our own implementation and copy nothing.
- **Pool filtering:** a prefix on `CardCreationOptions.GetPossibleCards(player)` reportedly covers card rewards, shops, events, potions, random-card effects and transforms (`CardFactory.CreateForReward` goes through it).
- **Run-start hook and saving:** a hidden run `ModifierModel` passed into `RunState.CreateForNewRun`. Its `[SavedProperty]` fields persist in the run save and sync in multiplayer. `OnRunCreated`/`OnRunLoaded` lifecycle, and `GenerateNeowOption` for a pre-Neow step.
- **Draft picks:** `CardSelectCmd.FromSimpleGrid` with a blocking choice context is network-synced.
- **Card library:** pool tabs are hard-coded, and custom sorts can be added to `NCardGrid.SortingAlgorithms`.

We'll confirm each of these against `sts2.dll` ourselves in Phase 2.

---

## 2. Inventory of what gets ported

| Area | Count (Java files) | Notes |
|---|---|---|
| Packs | 60 | Plus `AbstractCardPack` and `PackPreviewCard` |
| Cards | ~670 | 14 starter/system cards, the rest in packs (10-19 per pack; Monster Hunter has 34) |
| Powers | 159 | |
| Actions | 115 | Most disappear as standalone classes. They become helper methods or inline awaits |
| Card modifiers | 54 | Become BaseLib `CardModifier` |
| Patches | 100 | Each needs review. Many exist only to work around StS1 engine limits |
| Orbs | 20 | Summons, Weapons, Dragonwrath and Content Creator packs, plus `PackmasterOrb` and `Oblivion` |
| Relics | 14 | 7 core (`HandyHaversack`, `BagOfHolding`, `BanishingDecree`, `CollectorBadge`, `GemOfUnity`, `PMBoosterBox`, `PMBoosterPack`, `PMCollection`) plus pack relics |
| Potions | 9 | 4 core plus pack potions |
| Events | 1 | `BlackMarketDealerEvent` |
| Rewards | 3 | |
| Stances | 2 | Become powers |
| VFX | 70 | |
| Localization | 5 languages | eng, deu, kor, rus, zhs. StS2 uses the same codes |
| Assets | ~3,200 resource files | Card art, power and relic icons, pack art, audio, shaders |

**Character data to carry over:** 75 HP, 99 gold, 3 energy, 5 orb slots (only relevant if orbs are used). Starting deck is 4 Strike, 4 Defend, Rummage, Cardistry. Starting relic is Handy Haversack ("At the start of each combat, gain [E] and draw 1 card"), upgraded by Bag of Holding.

**Animation:** `PackmasterAnim.json`/`.atlas`/`.png` is a DragonBones export in Spine JSON form with no Spine version tag. It has 18 bones, 16 slots and only two animations, `Idle` and `Hit`.

---

## 3. Target architecture

### 3.1 Repository layout

Recommendation: a new C# project in a top-level `sts2/` folder on this branch. Later it can be split into its own repo, e.g. `PackmasterCharacter-StS2`. The Java mod stays untouched, so card logic and strings can be read side by side during the port.

```
sts2/
  Packmaster.csproj, Packmaster.json, project.godot
  PackmasterCode/
    MainFile.cs
    Character/        Packmaster.cs, PackmasterCardPool.cs, RelicPool, PotionPool
    Core/             PackRegistry, PackmasterPack (base), PackSelection (per-player state), RunPool filter patch
    Cards/            PackmasterCard (base), Starter/, <PackName>/...
    Powers/           PackmasterPower (base), <PackName>/...
    Stances/          PackmasterStancePower (base) + stance powers
    Orbs/  Relics/  Potions/  Events/  Rewards/  RestSite/
    Util/             Wiz.cs (C# facade mirroring the Java Wiz helpers), Rng helpers
    UI/               PackSetupScreen (Godot), PackFilterMenu, CurrentRunPacksTopBarButton
    Patches/
  Packmaster/
    localization/<lang>/*.json
    images/cards/<pack>/..., images/powers/..., images/relics/..., images/packs/...
    animations/packmaster/ (Spine 4.2 .skel/.atlas + SkeletonData .tres)
    scenes/ (creature visuals, rest site, merchant, char select bg, energy counter)
  tools/
    convert_loc.py     StS1 loc JSON -> StS2 loc tables
    gen_cards.py       Java card constructor/upgrade -> C# skeletons
```

### 3.2 The pack system (the core of the character)

> **Implementation notes (all our own code, on BaseLib):**
> - **Selection storage:** either a BaseLib `SavedSpireField` on the player or a hidden run `ModifierModel` with `[SavedProperty]` fields. Prefer the modifier if it gives us multiplayer sync for free; decide in the Phase 2 spike.
> - **Mutation:** include add/remove/ban pack operations from day one, for Banishing Decree, Booster Box and similar.
> - **Filter point:** `CardCreationOptions.GetPossibleCards` (to confirm), falling back to `CardPoolModel.GetUnlockedCards`.

- **`PackmasterPack` base class** (replaces `AbstractCardPack`): `Id`, localized name, description, author and credits. It also holds `IReadOnlyList<Type> Cards`, a `PackSummary` (offense/defense/support/frontload/scaling ratings plus tags) and optional `PackPotions`. The hat fields are dropped.
- **`PackRegistry`**: discovers all `PackmasterPack` subclasses by reflection at init, including ones from other assemblies so a future expansion mod works. It builds `card type -> pack` and `pack -> cards` maps, replacing `cardParentMap` and `cardClassParentMap`, and exposes a public `Register(pack)` API.
- **Per-player run state**: `SavedSpireField<Player, List<string>> SelectedPacks`, plus a banned-packs set for Banishing Decree. It is saved with the run and synced for co-op. This replaces the static `currentPoolPacks`.
- **Card-pool filtering**: all pack cards sit in `PackmasterCardPool`. A Harmony postfix on `CardPoolModel.GetUnlockedCards(...)` (BaseLib's `CommonActions` uses it to generate random class cards) filters the result to the owning player's selected packs. I need to check which reward, shop and transform paths call this against `sts2.dll` (decompile with ILSpy) in Phase 2. If some path bypasses it, patch the card-reward generator directly. Random-card effects inside cards ("add a random card from your packs") go through the same filtered query.
- **Pack draft at run start**: same rules as the original. The default custom-draft config is Core Set, then 3 random packs, then 3 choices of 1 from 3, for `PACKS_PER_RUN = 7` and `PACKS_PER_CHOICE = 3`. It also keeps the "custom draft" config, "all packs mode", "allow multiple NONE" and the pack filter menu. Implementation:
  - A Godot `Control` screen showing pack preview cards (built from `PackPreviewCard`'s design: pack art in a card frame, summary ratings as stars, hover to preview the pack's cards).
  - Triggered once when a Packmaster player's run starts, before the first map node. Hook point to confirm in Phase 2: either the run-start/Neow-equivalent flow, or a one-shot "Pack Setup" choice at the first room. In co-op each Packmaster player drafts on their own client. Results are sent with `CustomMessage` and applied on all peers before the run proceeds.
- **Compendium**: group or filter Packmaster cards by pack in the card library (nice to have; Phase 7).

### 3.3 Card base class and translation aids

- `PackmasterCard : CustomCardModel` with `[Pool(typeof(PackmasterCardPool))]`. It handles the image path from the pack folder (`images/cards/<pack>/<card>.png`; the existing 500x380 and 250x190 art works because StS2 scales it), a `Pack` lookup, and common helpers.
- **`Wiz.cs` facade**: 470 Java files call `Wiz.*`. Recreating the same helper names in C# (`Wiz.ApplyToEnemy`, `Wiz.ApplyToSelf`, `Wiz.Draw`, `Wiz.MakeInHand`, `Wiz.ForAllMonstersLiving`, `Wiz.Shuffle`, ...) as awaitable wrappers over StS2 commands turns most card bodies into near-mechanical translations.
- **`tools/gen_cards.py`**: Java cards follow a rigid pattern (`super(ID, cost, type, rarity, target)`, `baseDamage = X`, `upp() { upgradeDamage(Y); }`, `tags.add(...)`, `exhaust = true`). The script parses these and emits C# skeletons with `CanonicalVars`, `OnUpgrade`, keywords and a `// TODO port use()` block containing the original Java body as a comment. That leaves only the `use()` logic for manual or assisted translation.
- **`tools/convert_loc.py`**: converts `Cardstrings.json`, `Powerstrings.json` and the other loc files, both root and per-pack, into StS2 tables. It maps `!D!` to `{Damage:diff()}`, `!B!` to `{Block:diff()}`, `!M!` and `!anniv5:m2!` to named vars, `NL` to newline, `[E]` to the energy icon tag, and `#y`/`*` keyword markup to `[gold]`. It also rewrites the `{@@}{!x!|@=card|>1=cards}` plural syntax into SmartFormat plurals and the `${ModID}:` prefixes into `PACKMASTER-` keys, for all 5 languages. Output then gets reviewed by hand.

### 3.4 Shared mechanics to build once, before packs

| Mechanic | Used by | Plan |
|---|---|---|
| Stances | Aggression, Hermit, Utility (plus "Calm/Wrath"-referencing cards in Watcher pack) | `PackmasterStancePower` (single-instance power, mutually exclusive, `OnEnter`/`OnExit`, optional aura scene). A `StanceCmd.Change<T>()`, following the STS2-Buu pattern |
| Scry | Dimension Gate, Warden, Shaman | BaseLib `ScryCmd` |
| Orbs (custom) | Summons (7), Weapons (9), Dragonwrath, Content Creator | `CustomOrbModel`. Check the 5-slot default and the Packmaster orb slot count |
| Card modifiers | 54 classes across many packs | BaseLib `CardModifier` |
| Temp HP | several packs | StS2 has no StSLib TempHP. Implement as a power with `BeforeDamageReceived`, or use an equivalent if StS2 has one |
| "Infest", "Ruin", "Conjure Pack", "Pack Rip" system cards | cross-pack | Port with the starter cards in Phase 3 |
| Discard hook, global turn-start hook, scry callback patches | various | Mostly replaced by StS2 hooks (`AfterCardDiscarded`, `AfterSideTurnStart`, `IAfterScryed`) |

---

## 4. Character and animation

1. **Model**: `Packmaster : CustomCharacterModel`. Start from `PlaceholderCharacterModel` so it's playable early, then override every asset path. Set `NameColor` and the card-pool shader color from `characterColor` (0.76, 0.65, 0.52).
2. **Spine animation**: StS2 runs spine-godot 4.2. The current skeleton is a DragonBones-exported JSON. Options, in order of preference:
   1. Import `PackmasterAnim.json` into the Spine 4.2 editor (Spine can import older JSON; DragonBones' Spine export targets 3.x), fix anything that breaks, then export `.skel` and `.atlas` and wrap them in a `SpineSkeletonDataResource` `.tres` like HeartMod does. **Needs a Spine Essential/Pro license.**
   2. Re-export from the original DragonBones project, if the authors still have it, targeting a newer Spine format.
   3. Rebuild the two animations as a Godot-native animation (Skeleton2D/AnimationPlayer with the same atlas parts). BaseLib supports non-Spine visuals. No license needed, but the most manual.
3. **Animation states**: `SetupAnimationState(controller, idleName: "Idle", hitName: "Hit")`. Attack, cast and dead fall back to idle, the same as StS1 where only Hit was ever triggered (`ThePackmaster.damage()`). Adding attack, cast and death animations later is optional polish.
4. **Other scenes StS2 requires per character**: rest-site pose, merchant pose, character-select background and icon (from the existing select-button template), map marker, multiplayer icon outline, and the energy counter (the 11-layer bag orb becomes `CustomEnergyCounter` layer images). StS2 also asks for rock-paper-scissors arm textures and a "Yummy Cookie" relic icon. Placeholders are acceptable until art exists.
5. Corpse, shoulder images and cutscene panels: StS2 handles death and ending differently. Map these where an equivalent exists, otherwise drop them.

---

## 5. Porting packs: tiers and order

Port packs in tiers. Each pack ships as one unit: cards, powers, orbs, patches, loc and art, tested in-game. Then mark it as enabled in the registry, so partially-ported builds are always playable with the packs done so far.

**Tier 0 (required first)**: starter cards (Strike, Defend, Rummage, Cardistry), system cards (PackRip, ConjurePack, Vexed, OnInfest, modal choice card) and **Core Set**, which is in every default draft.

**Tier 1 (self-contained, few patches, standard mechanics; good for proving the Wiz facade and generator):**
Alignment, Basics, Bellord, Biting Cold, Board Game, Batter, Claw, Energy and Echo, Even/Odd, Farmer, Go With the Flow, High Energy, Mad Science, Pinnacle, Pixie, Prismatic, Quiet, Replicators, Strikes, Upgrades, Woman in Blue, Gems, Legacy, Bard Inspire, Anomaly, Arcana, Creativity, Colorless, Goddess of Explosions, Ring of Pain, Rimworld, Transmutation, Thievery

**Tier 2 (needs a shared mechanic from 3.4 or heavier custom patching):**
- Stances: Aggression, Hermit, Utility
- Scry: Dimension Gate, Warden, Shaman
- Orbs: Summons, Weapons, Dragonwrath, Content Creator (also 5 patches)
- Heavy patching: Psychic (7 patches), Warlock (7), Rip (5), Distortion, Oddities, Energy/Echo, Core Set (3)
- Large: Marisa (19 cards, 9 powers), Monster Hunter (34 cards, 7 powers)

**Tier 3 (packs made of StS1 base-game cards; need a design decision):**
Ironclad, Silent, Defect, Watcher, Poison, Orb, Bulwark, All For One, Status

- For **Ironclad, Silent and Defect**, these characters exist in StS2. Proposal: rebuild each pack from the closest StS2 base-game cards. Where a card no longer exists, port the StS1 version as a mod card.
- For **Watcher** (no StS2 equivalent), either port its 10 cards as mod cards on the shared stance and scry systems, or replace the pack with a Regent or Necrobinder pack built from StS2 cards.
- **Poison, Orb, Bulwark, All For One and Status** are themed mixes of StS1 base cards. Same approach: StS2 equivalent if one exists, mod-card reimplementation otherwise.

Each pack follows this checklist:
1. Run `gen_cards.py` for the pack and port each `use()` body.
2. Port the powers, then the orbs, actions and modifiers, which become helpers.
3. Review each patch: drop it, replace it with a StS2 hook, or port it to Harmony.
4. Convert loc, then review the wording against the StS2 keyword style.
5. Copy and rename art; use placeholders where missing.
6. Test in-game: debug config to force the pack into the draft, and the console to spawn each card. Check upgrade text, multiplayer (2 clients) and save/load mid-combat.
7. Enable it in the registry.

---

## 6. Relics, potions, events and UI

| Item | Plan |
|---|---|
| Handy Haversack | Starter relic, straight port |
| Bag of Holding | Was a boss relic replacing Haversack. Becomes an Ancient option (BaseLib `AncientOption`) that upgrades the starter relic, matching StS2's starter-upgrade convention |
| PM Booster Box / Booster Pack / Collection, Banishing Decree | Pack-interacting relics: depend on `PackRegistry` and per-player selection. Need grid or card selection through `BaseLibCardSelectCmd` and `CustomReward` (`PMBoosterBoxCardReward`, `SingleCardReward`) |
| Collector Badge, Gem of Unity | Track "packs played this turn/combat" with per-player state and the `AfterCardPlayed` hook |
| Pack relics (Biting Cold, Marisa, Pixie, Quiet, Summons) | Ported with their pack |
| Booster Brew, Modder's Delight, Pack in a Jar, Smithing Oil, pack potions | `CustomPotionModel`. Pack-exclusive potions are gated in the potion pool by selected packs, replacing `PotionPoolPatch` |
| Black Market Dealer event | `CustomEventModel` |
| Enhance bonfire option | `CustomRestSiteOption` |
| Current-run cards top-panel button | Godot top-bar button that opens the run's pack and card list |
| Pack filter menu, pack summaries/ratings | In the setup screen and mod config |
| Run history packs | Store selected packs in run history (nice to have) |
| Dropped | Hats, skins, skin selection UI, main-menu expansion button, FTUE, hat-unlock patches, metrics patches, font patches, Exordium title patch |

---

## 7. Phases and milestones

| Phase | Deliverable | Exit criteria |
|---|---|---|
| **0. Setup** | `sts2/` project from Alchyr's character template, BaseLib dependency, build and copy to `mods/`, `.pck` export | Empty character shows on character select |
| **1. Character skeleton** | Packmaster model with placeholder visuals, the 4 starter cards, Handy Haversack, card/relic/potion pools, loc for the character | Can start and finish a run with the starter deck |
| **2. Pack framework** | Our own `PackmasterPack` base class and `PackRegistry` with a public registration API. Per-player saved selection with add/remove/ban. Pool filter patch, after confirming the hook in `sts2.dll`. Run-start draft screen with multiplayer-synced picks. Config: custom draft slots, all packs, allow multiple None, pack filter. Top-bar packs button. Port Core Set | Rewards, shops and random-card effects only offer cards from drafted packs. Survives save/quit/resume. Works with 2 Packmasters in co-op |
| **3. Shared infrastructure** | `Wiz.cs`, `gen_cards.py`, `convert_loc.py`, system cards (Pack Rip, Conjure Pack, ...), stance system, temp HP, common modifiers | Generator and converter output compiles. System cards work |
| **4. Animation and character art** | Spine 4.2 conversion (or Godot-native fallback), energy counter, rest/merchant/char-select scenes, icons | No placeholder base-game art remains on the character |
| **5. Tier 1 packs** | ~33 packs | Each passes the per-pack checklist |
| **6. Tier 2 packs** | Stance, scry, orb and heavy-patch packs | Same |
| **7. Tier 3 packs, relics, potions, event, UI** | Base-card packs (after the design decision), the rest of section 6, compendium grouping | Feature parity with the StS1 mod minus cosmetics |
| **8. Polish and extensibility** | deu/kor/rus/zhs loc review, balance pass against StS2 numbers, public pack API documented for an Expansion Packs port | Release candidate |

Phases 4 and 5 can run in parallel once Phase 3 lands.

---

## 8. Risks and open questions

1. **Build environment**: compiling needs `sts2.dll` from a local StS2 install, and in-game testing needs the game. I can write and refactor code in a cloud session, but builds and playtests have to run on a machine with the game installed, or in CI with the DLL supplied privately. It can't be committed.
2. **Early Access churn**: StS2 and BaseLib are still changing; example mods target game 0.107-0.111 and BaseLib 3.3-3.4.7. Pin versions in the manifest and expect API breaks between game patches.
3. **Card-pool filtering hook**: the single most important technical spike, done at the start of Phase 2. Prior art suggests `CardCreationOptions.GetPossibleCards`; confirm it covers rewards, shops, events and transforms.
4. **Multiplayer**: StS1 Packmaster is single-player and leans on global statics. Decide whether the first release supports co-op fully, or only single-player with co-op later. Recommendation: build per-player state from day one, because retrofitting is much harder, and treat co-op testing as release-blocking only from Phase 7.
5. **Spine license and source files**: converting the animation cleanly needs the Spine editor, or the original DragonBones project.
6. **Base-game-card packs (Tier 3)**: the design choice (StS2 equivalents, reimplemented StS1 cards, or replacement packs) is yours to make, and it affects how "Packmaster" those packs feel.
7. **Credits and licensing**: the repo is MIT, but packs are by many contributors and the art comes from many sources. Keep author and credit strings per pack, and check that art redistribution is acceptable for a new release.
8. **Balance**: StS2 rebalanced enemy HP and base cards. Port numbers 1:1 first and balance afterwards (Phase 8), so port bugs and design changes don't get mixed up.

---

## Sources

- Prior art, unlicensed, reference only (no code used): https://github.com/mojimoon/Sts2Packmaster
- Alchyr ModTemplate-StS2: https://github.com/Alchyr/ModTemplate-StS2
- BaseLib-StS2: https://github.com/Alchyr/BaseLib-StS2 (wiki: https://alchyr.github.io/BaseLib-Wiki/)
- STS2-Buu (stances, large card pool): https://github.com/harsh2204/STS2-Buu
- HeartMod (Spine character visuals): https://github.com/oadvheahawcq/HeartMod
- HelloSpire (minimal character): https://github.com/r0zar/HelloSpire
- StS2 modding tutorial: https://github.com/fresh-milkshake/Modding-Tutorial
- ModSmith docs: https://cpimhoff.github.io/Sts2-ModSmith/docs/setup/guided.html
- Hermit StS2 port (precedent for legacy character ports): https://www.nexusmods.com/slaythespire2/mods/320
