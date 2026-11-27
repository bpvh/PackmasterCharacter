"""Naming rules shared by the loc converter and the card generator.

StS1 IDs look like "${ModID}:Rummage" (anniv5:Rummage). In StS2 a model's ID entry is derived from its C#
class name by StringHelper.Slugify and prefixed by BaseLib with the root namespace ("PACKMASTER-"), e.g.
class Rummage -> "PACKMASTER-RUMMAGE". Localization keys are "<entry>.title", "<entry>.description", ...

For plain CamelCase names (KiBlast -> KI_BLAST, HeartMod -> HEART_MOD, as seen in working mods) the slug
rule is unambiguous. For names with acronyms or digits ("PMBoosterBox", "Strike2") we can't be sure how
the game's Slugify splits them, so the generator emits [CustomID("PACKMASTER-...")] for those classes and
both tools use slug() below as the single source of truth.
"""
import re

MOD_PREFIX = "PACKMASTER-"
STS1_PREFIXES = ("${ModID}:", "anniv5:")

# Variable names used in StS2 loc strings and in generated C# cards. Keep both tools on these.
VAR_NAMES = {
    "D": "Damage",
    "B": "Block",
    "M": "Magic",                 # StS1 magicNumber
    "${ModID}:m2": "Magic2",      # SecondMagicNumber
    "anniv5:m2": "Magic2",
    "${ModID}:sd": "SecondDamage",
    "anniv5:sd": "SecondDamage",
    "stslib:ex": "Exhaustive",    # BaseLib ExhaustiveVar.Key
    "stslib:persist": "Persist",  # BaseLib PersistVar.Key
    "hoard": "Hoard",
}

_CSHARP_KEYWORDS = {
    "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
    "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
    "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
    "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
    "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
    "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
    "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
}


def strip_prefix(sts1_id: str) -> str:
    for p in STS1_PREFIXES:
        if sts1_id.startswith(p):
            return sts1_id[len(p):]
    return sts1_id


def class_name(sts1_id: str) -> str:
    """C# class name for an StS1 ID: prefix removed, non-identifier characters dropped, words capitalized."""
    name = strip_prefix(sts1_id)
    parts = re.split(r"[^0-9A-Za-z_]+", name)
    name = "".join(p[:1].upper() + p[1:] for p in parts if p)
    if not name:
        raise ValueError(f"cannot derive a class name from {sts1_id!r}")
    if name[0].isdigit():
        name = "_" + name
    if name in _CSHARP_KEYWORDS:
        name = "@" + name
    return name


def slug(name: str) -> str:
    """UPPER_SNAKE entry for a class name: HandyHaversack -> HANDY_HAVERSACK, PMBoosterBox -> PM_BOOSTER_BOX,
    Strike2 -> STRIKE_2. Matches StS2's Slugify for plain CamelCase; see module docstring for the rest."""
    name = name.lstrip("@")
    s = re.sub(r"([a-z])([A-Z])", r"\1_\2", name)
    s = re.sub(r"([A-Z]+)([A-Z][a-z])", r"\1_\2", s)
    s = re.sub(r"([A-Za-z])([0-9])", r"\1_\2", s)
    s = re.sub(r"([0-9])([A-Za-z])", r"\1_\2", s)
    s = re.sub(r"_+", "_", s).strip("_")
    return s.upper()


def needs_custom_id(name: str) -> bool:
    """True when the game's own Slugify might split the name differently than slug() does."""
    name = name.lstrip("@")
    return bool(re.search(r"[A-Z]{2}|[0-9]|_", name))


def entry(sts1_id_or_class: str, is_class: bool = False) -> str:
    """Full StS2 ID entry / loc key prefix, e.g. 'PACKMASTER-RUMMAGE'."""
    name = sts1_id_or_class if is_class else class_name(sts1_id_or_class)
    return MOD_PREFIX + slug(name)
