"""Convert StS1 (BaseMod/StSLib) description markup to StS2 loc markup.

StS1                                     StS2
!D! !B! !M! !${ModID}:m2! !stslib:ex!    {Damage:diff()} {Block:diff()} {Magic:diff()} {Magic2:diff()} {Exhaustive:diff()}
[E] / [E] [E]                            {energyPrefix:energyIcons(1)} / {energyPrefix:energyIcons(2)}
NL                                       newline
#yWord #bN #rWord #gWord #pWord          [gold]Word[/gold] [blue]N[/blue] [red].. [green].. [purple]..
*Word, [#EFC851]Word                     [gold]Word[/gold]
${ModID}:Plated_Armor, stslib:Retain     [gold]Plated Armor[/gold] (mod / StSLib keywords)
{@@}{!M!|1=|@=s}  (StSLib DynamicText)   {Magic:cond:=1?|s}
{@@}{!Upgrades!|>0=+}                    {IfUpgraded:show:+|}
DESCRIPTION + UPGRADE_DESCRIPTION        one string, differing part wrapped in {IfUpgraded:show:upgraded|base}

Plain keyword words (Block, Strength, ...) are auto-highlighted gold in StS1. StS2 needs explicit [gold] tags,
so known keyword names are wrapped too (see highlight_keywords).

Keyword-only lines that StS2 renders automatically from CardKeywords (Exhaust. Ethereal. Innate. Retain.
Unplayable.) are removed from card text by strip_keyword_lines; the card generator adds the keywords themselves.
"""
import re
from dataclasses import dataclass, field

from .ids import VAR_NAMES

# ---------------------------------------------------------------------------------------------------------
# Variables


def var_name(token: str) -> str:
    """StS1 dynamic variable key (without !) -> StS2 variable name."""
    if token in VAR_NAMES:
        return VAR_NAMES[token]
    key = token.split(":", 1)[-1]
    return key[:1].upper() + key[1:]


_VAR_RE = re.compile(r"!([A-Za-z0-9_$:{}]+?)!")


def _replace_vars(s: str) -> str:
    return _VAR_RE.sub(lambda m: "{" + var_name(m.group(1)) + ":diff()}", s)


# ---------------------------------------------------------------------------------------------------------
# StSLib DynamicText:  {!VAR!|cond=text|cond=text|@=default}


def _find_dynamic_blocks(s: str):
    """Yield (start, end) of top-level '{!VAR!|...}' blocks, honoring nested braces."""
    i = 0
    while True:
        start = s.find("{!", i)
        if start == -1:
            return
        depth, j = 0, start
        while j < len(s):
            if s[j] == "{":
                depth += 1
            elif s[j] == "}":
                depth -= 1
                if depth == 0:
                    break
            j += 1
        if j >= len(s):
            return
        yield start, j + 1
        i = j + 1


def _split_top(s: str, sep: str):
    parts, depth, cur = [], 0, []
    for ch in s:
        if ch == "{":
            depth += 1
        elif ch == "}":
            depth -= 1
        if ch == sep and depth == 0:
            parts.append("".join(cur))
            cur = []
        else:
            cur.append(ch)
    parts.append("".join(cur))
    return parts


_COND_RE = re.compile(r"^(>=|<=|>|<|=)?(-?\d+)$")


def _convert_dynamic_block(block: str, convert_inner) -> str:
    inner = block[1:-1]  # strip { }
    m = re.match(r"!([^!]+)!\|", inner)
    if not m:
        return block
    var = m.group(1)
    branches = _split_top(inner[m.end():], "|")
    conds, default = [], ""
    for br in branches:
        if "=" not in br:
            continue
        cond, text = br.split("=", 1)
        text = convert_inner(text)
        if cond == "@":
            default = text
            continue
        cm = _COND_RE.match(cond)
        if not cm:
            continue
        op, num = cm.group(1) or "=", cm.group(2)
        conds.append((op, num, text))

    if var == "Upgrades":
        # Times upgraded: StS2 cards upgrade once, so ">0", ">=1" and "1" all mean "upgraded".
        upgraded = next((t for op, n, t in conds if (op in (">", ">=") and int(n) + (op == ">") >= 1)
                         or (op == "=" and n == "1")), default)
        base = next((t for op, n, t in conds if op == "=" and n == "0"), default)
        return "{IfUpgraded:show:" + upgraded + "|" + base + "}"

    name = var_name(var)
    pieces = [f"{op}{num}?{text}" for op, num, text in conds]
    return "{" + name + ":cond:" + "|".join(pieces + [default]) + "}"


def _replace_dynamic(s: str, convert_inner) -> str:
    out, last = [], 0
    for start, end in _find_dynamic_blocks(s):
        out.append(s[last:start])
        out.append(_convert_dynamic_block(s[start:end], convert_inner))
        last = end
    out.append(s[last:])
    return "".join(out)


# ---------------------------------------------------------------------------------------------------------
# Highlighting

_COLOR_PREFIX = {"y": "gold", "b": "blue", "r": "red", "g": "green", "p": "purple"}


def _replace_color_prefixes(s: str) -> str:
    # #yWord / #b12 / #rWord ... up to whitespace or punctuation (keep trailing .,;:!?)
    def sub(m):
        tag = _COLOR_PREFIX[m.group(1)]
        word = m.group(2)
        trail = ""
        while word and word[-1] in ".,;:!?)\"'":
            trail = word[-1] + trail
            word = word[:-1]
        return f"[{tag}]{word}[/{tag}]{trail}" if word else m.group(0)
    return re.sub(r"#([ybrgp])([^\s#\[]+)", sub, s)


def _replace_hex_colors(s: str) -> str:
    return re.sub(r"\[#[0-9A-Fa-f]{6}\]([^\s\[]+?)(?=[\s.,;:!?]|$)", r"[gold]\1[/gold]", s)


def _replace_star_highlight(s: str) -> str:
    # StS1 "*Word" highlights the word (card names etc). Keep trailing punctuation outside the tag.
    def sub(m):
        word, trail = m.group(1), ""
        while word and word[-1] in ".,;:!?)\"'":
            trail = word[-1] + trail
            word = word[:-1]
        return f"[gold]{word}[/gold]{trail}" if word else m.group(0)
    return re.sub(r"(?<![\w/])\*([^\s*\[{]+)", sub, s)


def _replace_prefixed_keywords(s: str, keywords_out: set) -> str:
    # ${ModID}:Plated_Armor / anniv5:Ruin / stslib:Temporary_HP -> [gold]Plated Armor[/gold]
    def sub(m):
        word, trail = m.group(2), ""
        while word and word[-1] in ".,;:!?)\"'":
            trail = word[-1] + trail
            word = word[:-1]
        if not word:
            return m.group(0)
        shown = word.replace("_", " ")
        keywords_out.add(shown)
        return f"[gold]{shown}[/gold]{trail}"
    return re.sub(r"(\$\{ModID\}|anniv5|stslib):([^\s\[{!]+)", sub, s)


# StS1 base-game keyword names (English). StS1 highlighted these automatically; StS2 needs explicit tags.
VANILLA_KEYWORDS_ENG = [
    "Artifact", "Block", "Burn", "Burns", "Calm", "Channel", "Channeled", "Confused", "Curse", "Curses", "Dark",
    "Dazed", "Dexterity", "Divinity", "Ethereal", "Evoke", "Evoked", "Exhaust", "Exhausted", "Exhausts",
    "Focus", "Frail", "Frost", "Innate", "Intangible", "Lightning", "Lock-On", "Mantra", "Metallicize",
    "Orb", "Orbs", "Plasma", "Plated Armor", "Poison", "Poisoned", "Regen", "Retain", "Retained", "Ritual",
    "Scry", "Shiv", "Shivs", "Slimed", "Stance", "Status", "Strength", "Temporary HP", "Thorns",
    "Unplayable", "Upgrade", "Upgraded", "Upgrades", "Vigor", "Void", "Vulnerable", "Weak", "Wound", "Wounds",
    "Wrath",
]

_PROTECTED_RE = re.compile(r"(\{[^{}]*(?:\{[^{}]*\}[^{}]*)*\}|\[(?:gold|blue|red|green|purple)\].*?\[/(?:gold|blue|red|green|purple)\]|\[[^\]]*\])")


def highlight_keywords(s: str, keywords) -> str:
    """Wrap keyword names in [gold] outside of {vars} and existing tags. Longest names first."""
    names = sorted({k for k in keywords if k}, key=len, reverse=True)
    if not names:
        return s
    pattern = re.compile(r"(?<![\w-])(" + "|".join(re.escape(n) for n in names) + r")(?![\w-])")
    parts = _PROTECTED_RE.split(s)
    for i in range(0, len(parts), 2):  # even indices are unprotected text
        parts[i] = pattern.sub(r"[gold]\1[/gold]", parts[i])
    return "".join(parts)


# ---------------------------------------------------------------------------------------------------------
# Lines

AUTO_KEYWORD_LINES_ENG = {
    "exhaust.": "Exhaust", "ethereal.": "Ethereal", "innate.": "Innate", "retain.": "Retain",
    "unplayable.": "Unplayable", "exhaust": "Exhaust", "ethereal": "Ethereal", "innate": "Innate",
    "retain": "Retain", "unplayable": "Unplayable",
}


def split_lines(s: str):
    return [p.strip() for p in re.split(r"\s*\bNL\b\s*", s)]


_KW_SENTENCE = r"(?:Exhaust|Ethereal|Innate|Retain|Unplayable)\."


def strip_keyword_lines(lines, auto_map=AUTO_KEYWORD_LINES_ENG):
    """Remove auto-rendered keywords ('Exhaust.') that are a whole line, or a whole sentence at the start or
    end of a line ('Choose a card. Exhaust.'). Returns (lines, removed keywords, removed line indices).
    Only whole-line removals are reported as indices (they can be mirrored in other languages)."""
    kept, removed, idx = [], [], []
    for i, line in enumerate(lines):
        key = re.sub(r"\s+", " ", line).strip().lower()
        if key in auto_map:
            removed.append(auto_map[key])
            idx.append(i)
            continue
        while True:
            m = re.search(r"(?<=[.!?])\s+(" + _KW_SENTENCE + r")\s*$", line)
            if not m:
                break
            removed.append(m.group(1)[:-1])
            line = line[:m.start()]
        while True:
            m = re.match(r"^(" + _KW_SENTENCE + r")\s+(?=\S)", line)
            if not m:
                break
            removed.append(m.group(1)[:-1])
            line = line[m.end():]
        kept.append(line)
    return kept, removed, idx


# ---------------------------------------------------------------------------------------------------------
# Whole-string conversion


@dataclass
class ConvertResult:
    text: str
    keywords: set = field(default_factory=set)       # mod/StSLib keywords referenced by prefix
    removed_keywords: list = field(default_factory=list)  # auto keyword lines that were stripped
    removed_line_indices: list = field(default_factory=list)


def convert_inline(s: str, keywords_out: set) -> str:
    """Convert one fragment (no NL handling, no keyword auto-highlight)."""
    s = s.replace("{@@}", "")
    s = _replace_dynamic(s, lambda inner: convert_inline(inner, keywords_out))
    s = _replace_vars(s)
    s = re.sub(r"\[E\](?:\s*\[E\])*", lambda m: "{energyPrefix:energyIcons(%d)}" % m.group(0).count("[E]"), s)
    s = _replace_hex_colors(s)
    s = _replace_prefixed_keywords(s, keywords_out)
    s = _replace_color_prefixes(s)
    s = _replace_star_highlight(s)
    s = s.replace(" ", " ")
    return s


def tidy(s: str) -> str:
    s = re.sub(r"[ \t]+", " ", s)
    s = re.sub(r" ?\n ?", "\n", s)
    s = re.sub(r" ([.,;:!?])", r"\1", s)
    return s.strip()


def convert_text(s: str, keyword_names=(), strip_auto_keywords=False, strip_indices=None) -> ConvertResult:
    """Convert a full StS1 description.

    strip_auto_keywords: remove lines like 'Exhaust.' (English detection).
    strip_indices: remove these line indices instead (used for other languages, mirroring English).
    """
    if s is None:
        return ConvertResult("")
    kw = set()
    lines = split_lines(s)
    removed, idx = [], []
    if strip_indices is not None:
        idx = [i for i in strip_indices if i < len(lines)]
        lines = [l for i, l in enumerate(lines) if i not in set(idx)]
    elif strip_auto_keywords:
        lines, removed, idx = strip_keyword_lines(lines)
    lines = [convert_inline(l, kw) for l in lines]
    text = "\n".join(lines)
    text = highlight_keywords(text, list(keyword_names))
    return ConvertResult(tidy(text), kw, removed, idx)


# ---------------------------------------------------------------------------------------------------------
# Upgrade merge


def _tokens(s: str):
    """Split into word tokens, keeping {...} blocks and [tag]..[/tag] spans atomic."""
    toks, i = [], 0
    while i < len(s):
        if s[i] == "{":
            depth, j = 0, i
            while j < len(s):
                depth += {"{": 1, "}": -1}.get(s[j], 0)
                if depth == 0:
                    break
                j += 1
            toks.append(s[i:j + 1])
            i = j + 1
        elif s[i].isspace():
            j = i
            while j < len(s) and s[j].isspace():
                j += 1
            toks.append(s[i:j])
            i = j
        else:
            j = i
            while j < len(s) and not s[j].isspace() and s[j] != "{":
                j += 1
            toks.append(s[i:j])
            i = j
    return toks


def merge_upgrade(base: str, upgraded: str) -> str:
    """Combine base and upgraded text into one string with {IfUpgraded:show:upg|base} around the difference."""
    if upgraded is None or upgraded == base:
        return base
    a, b = _tokens(base), _tokens(upgraded)
    pre = 0
    while pre < len(a) and pre < len(b) and a[pre] == b[pre]:
        pre += 1
    suf = 0
    while suf < len(a) - pre and suf < len(b) - pre and a[-1 - suf] == b[-1 - suf]:
        suf += 1
    # Don't start or end the conditional in the middle of whitespace handling: move leading/trailing spaces out.
    mid_a, mid_b = a[pre:len(a) - suf], b[pre:len(b) - suf]
    head, tail = "".join(a[:pre]), "".join(a[len(a) - suf:]) if suf else ""
    ma, mb = "".join(mid_a), "".join(mid_b)
    if "|" in ma or "|" in mb:
        # A '|' inside the branches would break the IfUpgraded split; fall back to whole-string branches.
        return "{IfUpgraded:show:" + upgraded + "|" + base + "}"
    return head + "{IfUpgraded:show:" + mb + "|" + ma + "}" + tail
