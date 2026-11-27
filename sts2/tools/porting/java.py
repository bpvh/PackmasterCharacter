"""Minimal parsing of the StS1 Packmaster Java sources (cards and packs).

This is not a Java parser. The card sources follow a small set of conventions (see AbstractPackmasterCard and
the FILE TEMPLATE in src/main/java/thePackmaster/cards), so regexes plus brace matching are enough to pull out
the constructor, upp() and use() of nearly every card. Anything unusual is kept verbatim for manual porting.
"""
import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Optional

ROOT = Path(__file__).resolve().parents[3] / "src" / "main" / "java" / "thePackmaster"


def strip_comments(s: str) -> str:
    s = re.sub(r"/\*.*?\*/", "", s, flags=re.S)
    return re.sub(r"//[^\n]*", "", s)


def match_brace(s: str, open_idx: int) -> int:
    """Index just past the brace/paren that closes the one at open_idx (ignores string literals)."""
    pairs = {"{": "}", "(": ")"}
    opener = s[open_idx]
    closer = pairs[opener]
    depth, i, in_str = 0, open_idx, None
    while i < len(s):
        ch = s[i]
        if in_str:
            if ch == "\\":
                i += 2
                continue
            if ch == in_str:
                in_str = None
        elif ch in "\"'":
            in_str = ch
        elif ch == opener:
            depth += 1
        elif ch == closer:
            depth -= 1
            if depth == 0:
                return i + 1
        i += 1
    raise ValueError("unbalanced braces")


def method_body(src: str, signature_regex: str) -> Optional[str]:
    m = re.search(signature_regex, src)
    if not m:
        return None
    brace = src.find("{", m.end() - 1)
    end = match_brace(src, brace)
    return src[brace + 1:end - 1]


def split_statements(body: str):
    """Top-level statements of a method body (blocks such as if/for/anonymous classes stay whole)."""
    stmts, depth, cur, i, in_str = [], 0, [], 0, None
    while i < len(body):
        ch = body[i]
        cur.append(ch)
        if in_str:
            if ch == "\\" and i + 1 < len(body):
                cur.append(body[i + 1])
                i += 2
                continue
            if ch == in_str:
                in_str = None
        elif ch in "\"'":
            in_str = ch
        elif ch in "({":
            depth += 1
        elif ch in ")}":
            depth -= 1
            if depth == 0 and ch == "}":
                # end of a block statement, unless the block is part of an expression: new X() {...});
                rest = body[i + 1:].lstrip()
                if not rest.startswith((")", ";", ",", ".")):
                    stmts.append("".join(cur).strip())
                    cur = []
        elif ch == ";" and depth == 0:
            stmts.append("".join(cur).strip())
            cur = []
        i += 1
    tail = "".join(cur).strip()
    if tail:
        stmts.append(tail)
    return [s for s in stmts if s and s != ";"]


def norm(s: str) -> str:
    return re.sub(r"\s+", " ", s).strip()


# --------------------------------------------------------------------------------------------------------------


@dataclass
class JavaCard:
    path: Path
    cls: str
    package: str
    extends: str
    sts1_id: Optional[str]
    is_abstract: bool
    cost: Optional[str] = None
    type: Optional[str] = None
    rarity: Optional[str] = None
    target: Optional[str] = None
    values: dict = field(default_factory=dict)       # damage/block/magic/magic2/secondDamage/exhaustive/persist -> int
    upgrades: dict = field(default_factory=dict)     # same keys -> delta
    keywords: list = field(default_factory=list)     # Exhaust, Ethereal, Innate, Retain
    upgrade_keywords: dict = field(default_factory=dict)  # keyword -> "Add"/"Remove"
    upgrade_cost: Optional[int] = None               # new cost after upgrade (upgradeBaseCost)
    tags: list = field(default_factory=list)
    multi_damage: bool = False
    preview: Optional[str] = None                    # cardsToPreview class
    upgrade_preview: bool = False                    # cardsToPreview.upgrade() in upp()
    use_body: Optional[str] = None
    upp_rest: list = field(default_factory=list)     # untranslated upp() statements
    ctor_rest: list = field(default_factory=list)    # untranslated constructor statements
    other_members: str = ""                          # everything else in the class body, verbatim
    imports: list = field(default_factory=list)


_VALUE_FIELDS = {
    "baseDamage": "damage", "damage": "damage",
    "baseBlock": "block", "block": "block",
    "baseMagicNumber": "magic", "magicNumber": "magic",
    "baseSecondMagic": "magic2", "secondMagic": "magic2",
    "baseSecondDamage": "secondDamage", "secondDamage": "secondDamage",
}
_UPGRADE_METHODS = {
    "upgradeDamage": "damage", "upgradeBlock": "block", "upgradeMagicNumber": "magic", "upMagic": "magic",
    "upMagic2": "magic2", "upSecondMagic": "magic2", "upgradeSecondMagicNumber": "magic2",
    "upgradeSecondMagic": "magic2", "upgradeSecondDamage": "secondDamage",
}
_KEYWORD_FIELDS = {"exhaust": "Exhaust", "isEthereal": "Ethereal", "isInnate": "Innate", "selfRetain": "Retain"}


def _constants(src: str) -> dict:
    consts = {m.group(1): m.group(2) for m in re.finditer(r"(?:static\s+)?(?:final\s+)?int\s+(\w+)\s*=\s*(-?\d+)\s*;", src)}
    # private static final CardType TYPE = CardType.SKILL;  ->  TYPE: "CardType.SKILL"
    for m in re.finditer(r"(?:static\s+)?(?:final\s+)?(?:AbstractCard\.)?(CardType|CardRarity|CardTarget)\s+(\w+)\s*=\s*([\w.]+)\s*;", src):
        consts[m.group(2)] = m.group(3)
    return consts


def _enum(expr: str, consts: dict) -> str:
    expr = expr.strip()
    return consts.get(expr, expr).split(".")[-1]


def _int(expr: str, consts: dict) -> Optional[int]:
    expr = expr.strip()
    if re.fullmatch(r"-?\d+", expr):
        return int(expr)
    if expr in consts:
        return int(consts[expr])
    return None


def parse_card(path: Path) -> Optional[JavaCard]:
    raw = path.read_text(encoding="utf-8", errors="ignore")
    src = strip_comments(raw)
    cm = re.search(r"public\s+(abstract\s+)?class\s+(\w+)(?:<[^>]*>)?\s+extends\s+(\w+)", src)
    if not cm:
        return None
    is_abstract, cls, extends = bool(cm.group(1)), cm.group(2), cm.group(3)
    pkg = re.search(r"package\s+([\w.]+);", src)
    idm = re.search(r'ID\s*=\s*makeID\(\s*"([^"]+)"\s*\)', src) or re.search(
        r'ID\s*=\s*SpireAnniversary5Mod\.makeID\(\s*"([^"]+)"\s*\)', src)
    card = JavaCard(path=path, cls=cls, package=pkg.group(1) if pkg else "", extends=extends,
                    sts1_id=idm.group(1) if idm else None, is_abstract=is_abstract,
                    imports=re.findall(r"import\s+([\w.]+);", src))
    consts = _constants(src)

    # constructor: public Cls() { super(ID, cost, CardType.X, CardRarity.Y, CardTarget.Z ...); ... }
    ctor = method_body(src, rf"public\s+{cls}\s*\(\s*\)\s*\{{")
    if ctor is not None:
        sm = re.search(r"super\s*\(", ctor)
        if sm:
            end = match_brace(ctor, sm.end() - 1)
            args = [a.strip() for a in _split_args(ctor[sm.end():end - 1])]
            if len(args) >= 5:
                card.cost = args[1]
                card.type = _enum(args[2], consts)
                card.rarity = _enum(args[3], consts)
                card.target = _enum(args[4], consts)
            ctor_after = ctor[end:]
        else:
            ctor_after = ctor
        for st in split_statements(ctor_after):
            if not _ctor_statement(card, norm(st), consts):
                card.ctor_rest.append(norm(st))
        cval = _int(card.cost, consts) if card.cost else None
        if cval is not None:
            card.cost = str(cval)

    upp = method_body(src, r"public\s+void\s+upp\s*\(\s*\)\s*\{")
    if upp is not None:
        for st in split_statements(upp):
            if not _upp_statement(card, norm(st), consts):
                card.upp_rest.append(norm(st))

    card.use_body = method_body(src, r"public\s+void\s+use\s*\(\s*AbstractPlayer\s+\w+\s*,\s*AbstractMonster\s+\w+\s*\)\s*\{")
    card.other_members = _other_members(src, cls)
    return card


def _split_args(s: str):
    out, depth, cur = [], 0, []
    for ch in s:
        if ch in "([{":
            depth += 1
        elif ch in ")]}":
            depth -= 1
        if ch == "," and depth == 0:
            out.append("".join(cur))
            cur = []
        else:
            cur.append(ch)
    out.append("".join(cur))
    return out


# Constructor/upgrade statements that only affect StS1 rendering and have no StS2 meaning.
_IGNORED = [
    r"showEvokeValue\s*=\s*\w+", r"showEvokeOrbCount\s*=\s*\w+", r"FlavorText\..*", r"initializeDescription\(\s*\)",
    r"rawDescription\s*[+]?=.*", r"(?:this\.)?magicNumber\s*=\s*(?:this\.)?baseMagicNumber",
    r"(?:this\.)?secondMagic\s*=\s*(?:this\.)?baseSecondMagic", r"AnimatedCardsPatch\..*", r"loadCardImage\(.*\)",
    r"glowColor\s*=.*", r"name\s*=.*",
]


def _ignored(st: str) -> bool:
    st = re.sub(r"^this\.", "", st.rstrip(";").strip())
    return any(re.fullmatch(p, st) for p in _IGNORED)


def _ctor_statement(card: JavaCard, st: str, consts) -> bool:
    if _ignored(st):
        return True
    st = re.sub(r"^this\.", "", st.rstrip(";").strip())
    m = re.fullmatch(r"((?:this\.)?\w+\s*=\s*)+(-?\w+)", st)
    if m:
        names = [n.replace("this.", "").strip() for n in re.findall(r"((?:this\.)?\w+)\s*=", st)]
        value = st.split("=")[-1].strip()
        if all(n in _VALUE_FIELDS for n in names):
            v = _int(value, consts)
            if v is None:
                return False
            card.values[_VALUE_FIELDS[names[0]]] = v
            return True
        if len(names) == 1 and names[0] in _KEYWORD_FIELDS and value in ("true", "false"):
            kw = _KEYWORD_FIELDS[names[0]]
            if value == "true" and kw not in card.keywords:
                card.keywords.append(kw)
            return True
        if len(names) == 1 and names[0] == "isMultiDamage" and value == "true":
            card.multi_damage = True
            return True
    m = re.fullmatch(r"(?:this\.)?tags\.add\(\s*(?:AbstractCard\.)?(?:CardTags\.)?([\w.]+)\s*\)", st)
    if m:
        card.tags.append(m.group(1).split(".")[-1])
        return True
    m = re.fullmatch(r"(?:this\.)?cardsToPreview\s*=\s*new\s+(\w+)\(\s*\)", st)
    if m:
        card.preview = m.group(1)
        return True
    m = re.fullmatch(r"ExhaustiveVariable\.setBaseValue\(\s*this\s*,\s*(\w+)\s*\)", st)
    if m and _int(m.group(1), consts) is not None:
        card.values["exhaustive"] = _int(m.group(1), consts)
        return True
    m = re.fullmatch(r"PersistFields\.setBaseValue\(\s*this\s*,\s*(\w+)\s*\)", st)
    if m and _int(m.group(1), consts) is not None:
        card.values["persist"] = _int(m.group(1), consts)
        return True
    return False


def _upp_statement(card: JavaCard, st: str, consts) -> bool:
    if _ignored(st):
        return True
    st = re.sub(r"^this\.", "", st.rstrip(";").strip())
    if re.fullmatch(r"(?:this\.)?cardsToPreview\.upgrade\(\s*\)", st):
        card.upgrade_preview = True
        return True
    m = re.fullmatch(r"(\w+)\(\s*(-?\w+)\s*\)", st)
    if m and m.group(1) in _UPGRADE_METHODS:
        v = _int(m.group(2), consts)
        if v is None:
            return False
        key = _UPGRADE_METHODS[m.group(1)]
        card.upgrades[key] = card.upgrades.get(key, 0) + v
        return True
    if m and m.group(1) == "upgradeBaseCost":
        v = _int(m.group(2), consts)
        if v is None:
            return False
        card.upgrade_cost = v
        return True
    m = re.fullmatch(r"(\w+)\s*=\s*(true|false)", st)
    if m and m.group(1) in _KEYWORD_FIELDS:
        kw = _KEYWORD_FIELDS[m.group(1)]
        has = kw in card.keywords
        if m.group(2) == "true" and not has:
            card.upgrade_keywords[kw] = "Add"
        elif m.group(2) == "false" and has:
            card.upgrade_keywords[kw] = "Remove"
        return True
    if re.fullmatch(r"uDesc\(\s*\)|initializeDescription\(\s*\)|upgradeName\(\s*\)", st):
        return True
    if re.fullmatch(r"rawDescription\s*=\s*.+", st):
        return True  # upgraded text comes from the converted loc ({IfUpgraded:...})
    return False


def _other_members(src: str, cls: str) -> str:
    """Class body minus ID field, constructor, use() and upp(), for reference comments."""
    cm = re.search(rf"class\s+{cls}\b[^{{]*\{{", src)
    if not cm:
        return ""
    body = src[cm.end():match_brace(src, cm.end() - 1) - 1]
    for sig in (rf"public\s+{cls}\s*\(\s*\)\s*\{{", r"public\s+void\s+use\s*\([^)]*\)\s*\{", r"public\s+void\s+upp\s*\(\s*\)\s*\{"):
        m = re.search(sig, body)
        if m:
            start = m.start()
            # include preceding annotations
            ann = re.search(r"(\s*@\w+\s*)+$", body[:start])
            if ann:
                start = ann.start()
            end = match_brace(body, body.find("{", m.end() - 1))
            body = body[:start] + body[end:]
    body = re.sub(r"public\s+(final\s+)?static\s+(final\s+)?String\s+ID\s*=[^;]*;", "", body)
    # Constants already folded into the parsed values.
    body = re.sub(r"(?:(?:private|public|protected)\s+)?(?:static\s+)?(?:final\s+)?(?:static\s+)?int\s+\w+\s*=\s*-?\d+\s*;", "", body)
    body = re.sub(r"(?:(?:private|public|protected)\s+)?(?:static\s+)?(?:final\s+)?(?:static\s+)?(?:AbstractCard\.)?"
                  r"(?:CardType|CardRarity|CardTarget)\s+\w+\s*=\s*[\w.]+\s*;", "", body)
    return body.strip()


# --------------------------------------------------------------------------------------------------------------


@dataclass
class JavaPack:
    path: Path
    cls: str
    pack_id: str
    summary: Optional[tuple]
    tags: list
    card_classes: list          # simple class names, in order
    vanilla_cards: list         # card classes imported from com.megacrit (StS1 base game)
    potion_classes: list
    imports: dict               # simple name -> full import


def parse_pack(path: Path) -> Optional[JavaPack]:
    src = strip_comments(path.read_text(encoding="utf-8", errors="ignore"))
    cm = re.search(r"public\s+class\s+(\w+)\s+extends\s+AbstractCardPack", src)
    if not cm:
        return None
    idm = re.search(r'makeID\(\s*"([^"]+)"\s*\)', src)
    imports = {i.split(".")[-1]: i for i in re.findall(r"import\s+([\w.]+);", src)}
    sm = re.search(r"new\s+PackSummary\(([^)]*)\)", src)
    summary, tags = None, []
    if sm:
        args = [a.strip() for a in sm.group(1).split(",")]
        nums = [a for a in args if re.fullmatch(r"-?\d+", a)]
        summary = tuple(int(n) for n in nums[:5]) if len(nums) >= 5 else None
        tags = [a.split(".")[-1] for a in args if "Tags." in a]
    body = method_body(src, r"public\s+ArrayList<String>\s+getCards\s*\(\s*\)\s*\{") or ""
    cards = re.findall(r"\.add\(\s*(\w+)\.ID\s*\)", body)
    vanilla = [c for c in cards if imports.get(c, "").startswith("com.megacrit")]
    pot_body = method_body(src, r"public\s+ArrayList<String>\s+getPackPotions\s*\(\s*\)\s*\{") or ""
    potions = re.findall(r"\.add\(\s*(\w+)\.(?:POTION_)?ID\s*\)", pot_body)
    return JavaPack(path=path, cls=cm.group(1), pack_id=idm.group(1) if idm else cm.group(1), summary=summary,
                    tags=tags, card_classes=cards, vanilla_cards=vanilla, potion_classes=potions, imports=imports)


def index_classes(root: Path = ROOT) -> dict:
    """Simple class name -> list of paths, for every .java file under thePackmaster."""
    idx = {}
    for p in root.rglob("*.java"):
        idx.setdefault(p.stem, []).append(p)
    return idx


def resolve_class(name: str, imports: dict, idx: dict) -> Optional[Path]:
    full = imports.get(name)
    if full and full.startswith("thePackmaster."):
        p = ROOT.parent / (full.replace(".", "/") + ".java")
        if p.exists():
            return p
    paths = idx.get(name, [])
    return paths[0] if len(paths) == 1 else (paths[0] if paths else None)
