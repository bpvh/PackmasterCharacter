"""Emit StS2 C# from parsed StS1 Java cards and packs (see java.py)."""
import re
import textwrap

from . import ids
from .java import JavaCard, JavaPack, norm, split_statements

ROOT_NS = "Packmaster.PackmasterCode"

TYPE = {"ATTACK": "Attack", "SKILL": "Skill", "POWER": "Power", "STATUS": "Status", "CURSE": "Curse"}
RARITY = {"BASIC": "Basic", "COMMON": "Common", "UNCOMMON": "Uncommon", "RARE": "Rare", "SPECIAL": "Token",
          "CURSE": "Curse", "STATUS": "Status"}
TARGET = {"ENEMY": "AnyEnemy", "ALL_ENEMY": "AllEnemies", "SELF": "Self", "NONE": "None",
          "SELF_AND_ENEMY": "AnyEnemy", "ALL": "AllEnemies"}
TAGS = {"STRIKE": "CardTag.Strike", "STARTER_DEFEND": "CardTag.Defend"}
IGNORED_TAGS = {"STARTER_STRIKE", "HEALING", "EMPTY"}

# StS1 vanilla power class -> StS2 vanilla power class (only names confirmed to exist in StS2).
VANILLA_POWERS = {
    "WeakPower": "WeakPower", "VulnerablePower": "VulnerablePower", "StrengthPower": "StrengthPower",
    "DexterityPower": "DexterityPower", "PoisonPower": "PoisonPower", "FrailPower": "FrailPower",
    "ThornsPower": "ThornsPower", "VigorPower": "VigorPower", "PlatedArmorPower": "PlatingPower",
    "IntangiblePlayerPower": "IntangiblePower", "IntangiblePower": "IntangiblePower", "BufferPower": "BufferPower",
    "RegenPower": "RegenPower", "ConstrictedPower": "ConstrictPower", "FocusPower": "FocusPower",
    "EnergizedPower": "EnergyNextTurnPower", "EnergizedBluePower": "EnergyNextTurnPower",
    "DrawCardNextTurnPower": "DrawCardsNextTurnPower",
}

VFX = {
    "SLASH_DIAGONAL": "vfx/vfx_attack_slash", "SLASH_HORIZONTAL": "vfx/vfx_attack_slash",
    "SLASH_VERTICAL": "vfx/vfx_attack_slash", "SLASH_HEAVY": "vfx/vfx_attack_slash",
    "BLUNT_LIGHT": "vfx/vfx_attack_blunt", "SMASH": "vfx/vfx_heavy_blunt", "BLUNT_HEAVY": "vfx/vfx_heavy_blunt",
}

VAR_DECL = {"magic": "Magic", "magic2": "Magic2", "secondDamage": "SecondDamage"}


def _rel(path) -> str:
    """Path relative to the repo root (src/main/java/...)."""
    posix = path.as_posix()
    return "src/" + posix.split("/src/", 1)[1] if "/src/" in posix else posix


def namespace_for_package(package: str) -> str:
    """thePackmaster.cards.strikepack -> Packmaster.PackmasterCode.Cards.Strikepack (first letter capitalized only,
    so it never equals a pack class name such as StrikesPack)."""
    parts = package.split(".")
    if len(parts) >= 3 and parts[1] == "cards":
        sub = [p[:1].upper() + p[1:] for p in parts[2:]]
        return ".".join([ROOT_NS, "Cards"] + sub)
    return ROOT_NS + ".Cards"


def cs_comment_block(text: str, indent: str = "    ") -> str:
    lines = textwrap.dedent(text.rstrip()).splitlines() or [""]
    return "\n".join(f"{indent}// {l}".rstrip() for l in lines)


# --------------------------------------------------------------------------------------------------------------
# use() translation


def amount(expr: str):
    """Java amount expression -> C# decimal expression, or None."""
    e = norm(expr).replace("this.", "")
    neg = e.startswith("-")
    e = e.lstrip("-").strip()
    table = {
        "magicNumber": 'DynamicVars["Magic"].BaseValue', "secondMagic": 'DynamicVars["Magic2"].BaseValue',
        "damage": "DynamicVars.Damage.BaseValue", "block": "DynamicVars.Block.BaseValue",
    }
    if e in table:
        out = table[e]
    elif re.fullmatch(r"\d+", e):
        out = f"{e}m"
    else:
        return None
    return ("-" + out) if neg else out


def _vfx_arg(fx: str) -> str:
    v = VFX.get(fx.split(".")[-1])
    return f', vfx: "{v}"' if v else ""


def translate_statement(st: str, card: JavaCard, need):
    """One Java statement -> list of C# lines, or None if not understood. `need` collects usings/flags."""
    s = norm(st).rstrip(";").strip()
    s = re.sub(r"^(Wiz\.|this\.)", "", s)
    s = re.sub(r"^AbstractDungeon\.actionManager\.addToBottom\((.*)\)$", r"atb(\1)", s)
    s = re.sub(r"^addToBot\((.*)\)$", r"atb(\1)", s)

    m = re.fullmatch(r"(?:dmg|dmgTop|altDmg)\(\s*m\s*,\s*([\w.]+)\s*\)", s)
    if m:
        return [f"await CommonActions.CardAttack(this, play{_vfx_arg(m.group(1))}).Execute(ctx);"]
    m = re.fullmatch(r"(?:allDmg|allDmgTop)\(\s*([\w.]+)\s*\)", s)
    if m:
        need.add("all_enemies")
        return [f"await CommonActions.CardAttack(this, play{_vfx_arg(m.group(1))}).Execute(ctx);"]
    m = re.fullmatch(r"atb\(new DamageAction\(\s*m\s*,\s*new DamageInfo\(\s*p\s*,\s*(?:this\.)?damage\s*,[^)]*\)\s*(?:,\s*([\w.]+))?\s*\)\)", s)
    if m:
        return [f"await CommonActions.CardAttack(this, play{_vfx_arg(m.group(1) or '')}).Execute(ctx);"]
    m = re.fullmatch(r"doDmg\(\s*m\s*,\s*(?:this\.)?damage\s*(?:,\s*([\w.]+))?\s*\)", s)
    if m:
        return [f"await CommonActions.CardAttack(this, play{_vfx_arg(m.group(1) or '')}).Execute(ctx);"]
    m = re.fullmatch(r"atb\(new DamageAllEnemiesAction\(\s*p\s*,\s*(?:this\.)?multiDamage\s*,[^,]*(?:,\s*([\w.]+))?\s*\)\)", s)
    if m:
        need.add("all_enemies")
        return [f"await CommonActions.CardAttack(this, play{_vfx_arg(m.group(1) or '')}).Execute(ctx);"]
    if re.fullmatch(r"blck\(\s*\)|atb\(new GainBlockAction\(\s*p\s*,\s*(?:p\s*,\s*)?(?:this\.)?block\s*\)\)", s):
        return ["await CommonActions.CardBlock(this, play);"]
    m = re.fullmatch(r"atb\(new DrawCardAction\(\s*(?:p\s*,\s*)?([^,()]+)\s*\)\)", s)
    if m and amount(m.group(1)):
        return [f"await CardPileCmd.Draw(ctx, {amount(m.group(1))}, Owner);"]
    m = re.fullmatch(r"atb\(new GainEnergyAction\(\s*([^,()]+)\s*\)\)", s)
    if m and amount(m.group(1)):
        return [f"await PlayerCmd.GainEnergy({amount(m.group(1))}, Owner);"]

    m = re.fullmatch(r"atb\(new ChannelAction\(new (?:[\w.]+\.)?(Lightning|Frost|Dark|Plasma|Glass)\(\s*\)\)\)", s)
    if m:
        need.add("orbs")
        return [f"await OrbCmd.Channel<{m.group(1)}Orb>(ctx, Owner);"]
    m = re.fullmatch(r"atb\(new DiscardAction\(\s*p\s*,\s*p\s*,\s*([^,()]+)\s*,\s*false\s*\)\)", s)
    if m and amount(m.group(1)):
        need.add("wiz")
        return [f"await Wiz.ChooseAndDiscard(ctx, this, (int){amount(m.group(1))});"]
    m = re.fullmatch(r"atb\(new AttackDamageRandomEnemyAction\(\s*this\s*(?:,\s*([\w.]+))?\s*\)\)", s)
    if m:
        need.add("wiz")
        return [f"await Wiz.AttackRandomEnemy(ctx, this, play{_vfx_arg(m.group(1) or '')});"]
    m = re.fullmatch(r"for \(int i = 0; i < (?:this\.)?magicNumber; i\+\+\) \{ atb\(new AttackDamageRandomEnemyAction\(\s*this\s*(?:,\s*([\w.]+))?\s*\)\); \}", s)
    if m:
        need.add("wiz")
        return ['for (int i = 0; i < DynamicVars["Magic"].IntValue; i++)',
                f"    await Wiz.AttackRandomEnemy(ctx, this, play{_vfx_arg(m.group(1) or '')});"]

    # Powers on the target / self / all enemies.
    power_re = r"new (\w+)\(\s*(\w+)\s*,\s*(-?[\w.]+)\s*(?:,\s*(?:true|false))?\s*\)"
    m = re.fullmatch(rf"applyToEnemy(?:Top)?\(\s*m\s*,\s*{power_re}\s*\)", s) or \
        re.fullmatch(rf"atb\(new ApplyPowerAction\(\s*m\s*,\s*p\s*,\s*{power_re}\s*(?:,\s*-?[\w.]+)?\s*\)\)", s)
    if m:
        return _apply(m.group(1), m.group(3), "play.Target!", need)
    m = re.fullmatch(rf"applyToSelf(?:Top)?\(\s*{power_re}\s*\)", s) or \
        re.fullmatch(rf"atb\(new ApplyPowerAction\(\s*p\s*,\s*p\s*,\s*{power_re}\s*(?:,\s*-?[\w.]+)?\s*\)\)", s)
    if m:
        return _apply(m.group(1), m.group(3), "Owner.Creature", need)
    m = re.fullmatch(rf"forAllMonstersLiving\(\s*(\w+)\s*->\s*applyToEnemy\(\s*\w+\s*,\s*{power_re}\s*\)\s*\)", s)
    if m:
        return _apply(m.group(2), m.group(4), "CombatState!.HittableEnemies", need)
    return None


def _apply(java_power: str, amt_expr: str, target: str, need):
    power = VANILLA_POWERS.get(java_power)
    amt = amount(amt_expr)
    if not power or not amt:
        return None
    need.add("vanilla_powers")
    # BaseLib's CommonActions wraps PowerCmd.Apply, whose signature changed between game versions.
    if target.endswith("HittableEnemies"):
        return [f"foreach (var enemy in {target}.ToList())",
                f"    await CommonActions.Apply<{power}>(ctx, enemy, this, {amt});"]
    return [f"await CommonActions.Apply<{power}>(ctx, {target}, this, {amt});"]


def translate_use(card: JavaCard):
    """Returns (lines, fully_translated, needs). Untranslated statements become TODO comments in place."""
    need = set()
    if not card.use_body or not card.use_body.strip():
        return [], True, need
    out, ok = [], True
    for st in split_statements(card.use_body):
        cs = translate_statement(st, card, need)
        if cs is None:
            ok = False
            out.append("// TODO port: " + norm(st)[:200])
        else:
            out.extend(cs)
    return out, ok, need


# --------------------------------------------------------------------------------------------------------------


def card_class(card: JavaCard, base_class: str, namespace: str, eng_text: str = "") -> str:
    """C# source for a concrete card."""
    lines, full, need = translate_use(card)
    cls = ids.class_name(card.sts1_id) if card.sts1_id else card.cls
    usings = []
    if "vanilla_powers" in need:
        usings.append("using MegaCrit.Sts2.Core.Models.Powers;")
    if "orbs" in need:
        usings.append("using MegaCrit.Sts2.Core.Models.Orbs;")
    if "wiz" in need:
        usings.append("using Packmaster.PackmasterCode.Util;")
    if "exhaustive" in card.values or "persist" in card.values:
        usings.append("using BaseLib.Cards.Variables;")

    cost = card.cost or "0"
    x_cost = cost == "-1"
    unplayable = cost == "-2"
    cost_val = "0" if (x_cost or unplayable or not re.fullmatch(r"-?\d+", cost)) else cost
    ctype = TYPE.get(card.type or "", "Skill")
    rarity = RARITY.get(card.rarity or "", "Common")
    target = TARGET.get(card.target or "", "Self")
    if "all_enemies" in need or card.multi_damage:
        target = "AllEnemies"

    body = []
    v, u = card.values, card.upgrades
    if "damage" in v:
        body.append(f"WithDamage({v['damage']}{', ' + str(u['damage']) if u.get('damage') else ''});")
    if "block" in v:
        body.append(f"WithBlock({v['block']}{', ' + str(u['block']) if u.get('block') else ''});")
    for key, name in VAR_DECL.items():
        if key in v:
            body.append(f'WithVar("{name}", {v[key]}{", " + str(u[key]) if u.get(key) else ""});')
    if "exhaustive" in v:
        body.append(f"WithVar(new ExhaustiveVar({v['exhaustive']}));")
    if "persist" in v:
        body.append(f"WithVar(new PersistVar({v['persist']}));")
    for kw in card.keywords:
        mode = card.upgrade_keywords.get(kw)
        body.append(f"WithKeyword(CardKeyword.{kw}{', UpgradeType.Remove' if mode == 'Remove' else ''});")
    for kw, mode in card.upgrade_keywords.items():
        if mode == "Add":
            body.append(f"WithKeyword(CardKeyword.{kw}, UpgradeType.Add);")
    if unplayable:
        body.append("WithKeyword(CardKeyword.Unplayable);")
    if card.upgrade_cost is not None and re.fullmatch(r"-?\d+", cost) and not x_cost:
        delta = card.upgrade_cost - int(cost)
        if delta:
            body.append(f"WithCostUpgradeBy({delta});")
    tags = [TAGS[t] for t in card.tags if t in TAGS]
    if tags:
        body.append(f"WithTags({', '.join(tags)});")
    todo_ctor = [t for t in card.tags if t not in TAGS and t not in IGNORED_TAGS]
    for t in todo_ctor:
        body.append(f"// TODO port tag: {t}")
    if card.preview:
        body.append(f"// TODO tooltip for cardsToPreview: {card.preview}{' (upgraded with this card)' if card.upgrade_preview else ''}")
    for st in card.ctor_rest:
        body.append("// TODO port (constructor): " + st[:200])

    upgrade_todo = [st for st in card.upp_rest]
    status = "ported" if (full and not card.ctor_rest and not card.upp_rest and not todo_ctor and not card.preview
                          and not card.other_members.strip() and not x_cost) else "skeleton"

    out = []
    out.append("// <auto-generated> by tools/gen_cards.py from " + _rel(card.path))
    out.append(f"// Status: {status}. Review against the Java source, then delete this header.")
    out.append("// </auto-generated>")
    out.extend(usings)
    if usings:
        out.append("")
    out.append(f"namespace {namespace};")
    out.append("")
    out.append("/// <summary>")
    for l in (eng_text or "").splitlines():
        out.append("/// " + l.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;"))
    out.append("/// </summary>")
    if ids.needs_custom_id(cls):
        out.append(f'[BaseLib.Utils.Attributes.CustomID("{ids.entry(cls, is_class=True)}")]')
    out.append(f"public sealed class {cls} : {base_class}")
    out.append("{")
    out.append(f"    public {cls}() : base({cost_val}, CardType.{ctype}, CardRarity.{rarity}, TargetType.{target})")
    out.append("    {")
    for b in body:
        out.append("        " + b)
    out.append("    }")
    if x_cost:
        out.append("")
        out.append("    protected override bool HasEnergyCostX => true;")
    out.append("")
    out.append("    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)")
    out.append("    {")
    if not lines:
        out.append("        await Task.CompletedTask;")
    for l in lines:
        out.append("        " + l)
    if lines and all(l.startswith("//") for l in lines):
        out.append("        await Task.CompletedTask;")
    out.append("    }")
    if upgrade_todo:
        out.append("")
        out.append("    protected override void OnUpgrade()")
        out.append("    {")
        for st in upgrade_todo:
            out.append("        // TODO port: " + st[:200])
        out.append("    }")
    if card.use_body and not full:
        out.append("")
        out.append("    // StS1 use():")
        out.append(cs_comment_block(card.use_body.strip("\n"), "    "))
    if card.other_members.strip():
        out.append("")
        out.append("    // Other StS1 members (port as needed):")
        out.append(cs_comment_block(card.other_members, "    "))
    out.append("}")
    return "\n".join(out) + "\n", status


def abstract_card_class(card: JavaCard, base_class: str, namespace: str) -> str:
    out = [
        "// <auto-generated> by tools/gen_cards.py from " + _rel(card.path),
        "// Pack base card. StS1 used it for the pack frame and shared helpers; port helpers below as cards need them.",
        "// </auto-generated>",
        f"namespace {namespace};",
        "",
        f"public abstract class {card.cls}(int cost, CardType type, CardRarity rarity, TargetType target)",
        f"    : {base_class}(cost, type, rarity, target)",
        "{",
    ]
    if card.other_members.strip():
        out.append("    // StS1 members:")
        out.append(cs_comment_block(card.other_members, "    "))
    out.append("}")
    return "\n".join(out) + "\n"


def pack_class(pack: JavaPack, card_types: list, enabled: bool, missing: list) -> str:
    summary = pack.summary or (0, 0, 0, 0, 0)
    tags = "".join(f", PackTag.{t}" for t in pack.tags)
    cls = ids.class_name(pack.pack_id)
    out = [
        "// <auto-generated> by tools/gen_cards.py from " + _rel(pack.path),
        "// </auto-generated>",
        f"namespace {ROOT_NS}.Packs.Definitions;",
        "",
        f"public sealed class {cls} : PackmasterPack",
        "{",
        f'    public override string PackId => "{pack.pack_id}";',
        f"    public override bool Enabled => {'true' if enabled else 'false'};",
        f"    public override Type PreviewCardType => typeof({cls}Preview);",
        f"    public override PackSummary Summary => new({', '.join(str(x) for x in summary)}{tags});",
    ]
    if card_types:
        out.append("    public override IReadOnlyList<Type> CardTypes =>")
        out.append("    [")
        for t in card_types:
            out.append(f"        typeof({t}),")
        out.append("    ];")
    else:
        out.append("    public override IReadOnlyList<Type> CardTypes => [];")
    if missing:
        out.append(f"    // Not ported yet ({len(missing)}): " + ", ".join(missing))
    out.append("}")
    out.append("")
    out.append(f"public sealed class {cls}Preview : PackPreviewCard")
    out.append("{")
    out.append(f'    public override string PackId => "{pack.pack_id}";')
    out.append("}")
    return "\n".join(out) + "\n"
