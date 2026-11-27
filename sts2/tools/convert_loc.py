#!/usr/bin/env python3
"""Convert the StS1 Packmaster localization to StS2 loc tables.

    python3 tools/convert_loc.py                 # all languages, merge into Packmaster/localization/
    python3 tools/convert_loc.py --lang eng      # one language
    python3 tools/convert_loc.py --report        # also write tools/out/loc_report.json (review list)
    python3 tools/convert_loc.py --check         # exit 1 if the output would change (for CI)

Input:  ../src/main/resources/anniv5Resources/localization/<lang>/{,<pack>/}*strings.json
Output: Packmaster/localization/<lang>/<table>.json

  Cardstrings     -> cards.json            <ENTRY>.title / .description (+ .extended<N> for EXTENDED_DESCRIPTION)
  Powerstrings    -> powers.json           .title / .description / .smartDescription (+ .fragment<N>)
  Relicstrings    -> relics.json           .title / .description / .flavor (+ .fragment<N>)
  Potionstrings   -> potions.json          .title / .description (+ .fragment<N>)
  Keywordstrings  -> static_hover_tips.json  StS1 keywords are tooltips, i.e. StS2 StaticHoverTips
  Orbstrings      -> orbs.json             .title / .description (+ .fragment<N>)
  Stancestrings   -> powers.json           stances become powers in StS2
  UIstrings       -> packmaster_ui.json    <ENTRY>.text<N>; a custom table for code-side strings

Hand-written entries win: a key that already exists in the output file and is not listed in
tools/loc_generated_keys.json (keys this tool wrote before) is never overwritten. Delete a key from the
output to have it regenerated.

StS1 powers, relics and potions assemble their text in code from DESCRIPTIONS fragments, so their
description is a best guess (fragments joined around {Amount}); every fragment is also kept as
.fragment<N> and listed in the report for manual review.
"""
import argparse
import re
import json
import sys
from collections import OrderedDict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from porting import ids  # noqa: E402
from porting.loc import (VANILLA_KEYWORDS_ENG, convert_inline, convert_text, highlight_keywords,  # noqa: E402
                         merge_upgrade, tidy)

HERE = Path(__file__).resolve().parent
STS2 = HERE.parent
SRC = STS2.parent / "src" / "main" / "resources" / "anniv5Resources" / "localization"
DST = STS2 / "Packmaster" / "localization"
GENERATED_KEYS = HERE / "loc_generated_keys.json"
REPORT = HERE / "out" / "loc_report.json"

LANGS = ["eng", "deu", "kor", "rus", "zhs"]


def load_strings(lang: str, name: str) -> "OrderedDict[str, dict]":
    """Merge <lang>/<name> and <lang>/*/<name>, pack files after the root file."""
    root = SRC / lang
    out = OrderedDict()
    files = [root / name] + sorted(root.glob(f"*/{name}"))
    for f in files:
        if f.exists():
            data = json.loads(f.read_text(encoding="utf-8-sig"), object_pairs_hook=OrderedDict)
            if isinstance(data, list):  # Keywordstrings is a list
                folder = "root" if f.parent == root else f.parent.name
                for i, item in enumerate(data):
                    out[f"{folder}#{i}"] = item
            else:
                out.update(data)
    return out


def keyword_names(lang: str):
    names = set()
    for item in load_strings(lang, "Keywordstrings.json").values():
        names.update(n for n in item.get("NAMES", []) if n)
        if item.get("PROPER_NAME"):
            names.add(item["PROPER_NAME"])
    if lang == "eng":
        names.update(VANILLA_KEYWORDS_ENG)
    # Very short or lowercase-only names cause false positives inside normal words of other languages.
    return {n for n in names if len(n) >= 3}


def owned_of(owned_all: dict, key: str) -> dict:
    """Generated-key record for one table. Older records were plain key lists (treated as unedited)."""
    owned = owned_all.get(key, {})
    if isinstance(owned, list) or any(len(str(v)) != 10 for v in owned.values()):
        # older formats: a key list, or full values; convert to hashes of the current file values
        path = DST / f"{key}.json"
        data = json.loads(path.read_text(encoding="utf-8-sig") or "{}") if path.exists() else {}
        keys = owned if isinstance(owned, list) else [k for k, v in owned.items() if data.get(k) == v]
        owned = {k: value_hash(data[k]) for k in keys if k in data}
    return owned


def java_format_to_amount(s: str) -> str:
    """StS1 code filled '{0}' / '%s' / '%d' placeholders with String.format; StS2 SmartFormat would treat '{0}'
    as a positional argument, so map them to {Amount} (reviewed per power anyway)."""
    import re
    return re.sub(r"\{\d+\}|%[sd]", "{Amount}", s)


class Converter:
    def __init__(self, report):
        self.report = report
        # English card line-strip decisions, reused for other languages: entry -> (desc idx, upg idx)
        self.strip_plan = {}

    # ---- cards
    def cards(self, lang, kw):
        out = OrderedDict()
        for sid, card in load_strings(lang, "Cardstrings.json").items():
            entry = ids.entry(sid)
            if "NAME" in card:
                out[f"{entry}.title"] = card["NAME"]
            desc, upg = card.get("DESCRIPTION"), card.get("UPGRADE_DESCRIPTION")
            if desc is not None:
                if lang == "eng":
                    d = convert_text(desc, kw, strip_auto_keywords=True)
                    u = convert_text(upg, kw, strip_auto_keywords=True) if upg else None
                    self.strip_plan[entry] = (d.removed_line_indices, u.removed_line_indices if u else [])
                    removed = sorted(set(d.removed_keywords) | set(u.removed_keywords if u else []))
                    if removed or (u and set(u.removed_keywords) != set(d.removed_keywords)):
                        self.report.setdefault("card_keywords_from_text", {})[entry] = {
                            "base": d.removed_keywords, "upgraded": u.removed_keywords if u else None}
                else:
                    di, ui = self.strip_plan.get(entry, ([], []))
                    d = convert_text(desc, kw, strip_indices=di)
                    u = convert_text(upg, kw, strip_indices=ui) if upg else None
                text = merge_upgrade(d.text, u.text) if u else d.text
                out[f"{entry}.description"] = text
                for k in d.keywords | (u.keywords if u else set()):
                    self.report.setdefault("keywords_used", {}).setdefault(k, set()).add(entry)
            for i, ext in enumerate(card.get("EXTENDED_DESCRIPTION") or []):
                out[f"{entry}.extended{i}"] = convert_text(ext, kw).text
        return out

    # ---- powers / relics / potions / orbs: text assembled from fragments in StS1 code
    def fragments(self, lang, kw, filename, kind, title_key="NAME", flavor=False):
        out = OrderedDict()
        for sid, item in load_strings(lang, filename).items():
            entry = ids.entry(sid)
            if title_key in item:
                out[f"{entry}.title"] = item[title_key]
            frags = item.get("DESCRIPTIONS") or item.get("DESCRIPTION") or []
            if isinstance(frags, str):
                frags = [frags]
            # A fragment ending in a color prefix ("gain #b") means StS1 code appended the number right after it.
            blue_next = [bool(re.search(r"#[by]\s*$", f)) for f in frags]
            frags = [re.sub(r"\s*#[by]\s*$", "", f) for f in frags]
            conv = [tidy(highlight_keywords(convert_inline(java_format_to_amount(f).replace(" NL ", "\n"), set()), kw))
                    for f in frags]
            if conv:
                if kind == "relic" or len(conv) == 1:
                    guess = conv[0]
                else:
                    amount = "[blue]{Amount}[/blue]" if blue_next[0] else "{Amount}"
                    guess = tidy(f" {amount} ".join(conv[:2]) + (" " + " ".join(conv[2:]) if len(conv) > 2 else ""))
                out[f"{entry}.description"] = guess
                if kind == "power":
                    out[f"{entry}.smartDescription"] = guess
                if len(conv) > 1:
                    for i, f in enumerate(conv):
                        out[f"{entry}.fragment{i}"] = f
                    if lang == "eng":
                        self.report.setdefault("review_fragments", {})[entry] = conv
            if flavor and item.get("FLAVOR"):
                out[f"{entry}.flavor"] = item["FLAVOR"]
        return out

    def keywords(self, lang):
        """Keywordstrings are lists; entries line up across languages, so IDs come from the English entry
        at the same (file, index)."""
        out = OrderedDict()
        if not hasattr(self, "keyword_entries"):
            self.keyword_entries = {}
            for key, item in load_strings("eng", "Keywordstrings.json").items():
                name = item.get("PROPER_NAME") or (item.get("NAMES") or [""])[0]
                if name:
                    self.keyword_entries[key] = ids.MOD_PREFIX + ids.slug(ids.class_name(name))
        items = load_strings(lang, "Keywordstrings.json")
        # StS1 matched keywords by per-language name, so there is no shared ID. Align each file's list with
        # the English one by position; if a translation has extra entries (zhs prepends some), align from the end.
        by_folder = {}
        for key in items:
            by_folder.setdefault(key.split("#")[0], []).append(key)
        eng_by_folder = {}
        for key in self.keyword_entries:
            eng_by_folder.setdefault(key.split("#")[0], []).append(key)
        aligned = {}
        for folder, keys in by_folder.items():
            eng_keys = eng_by_folder.get(folder, [])
            if len(keys) != len(eng_keys) and lang != "eng":
                self.report.setdefault("keyword_alignment_from_end", []).append(
                    f"{lang}/{folder}: {len(keys)} entries vs {len(eng_keys)} in English")
                offset = len(keys) - len(eng_keys)
                for i, key in enumerate(keys):
                    j = i - offset
                    if 0 <= j < len(eng_keys):
                        aligned[key] = eng_keys[j]
            else:
                aligned.update(zip(keys, eng_keys))
        for key, item in items.items():
            name = item.get("PROPER_NAME") or (item.get("NAMES") or [""])[0]
            entry = self.keyword_entries.get(aligned.get(key, ""))
            if not name or not entry:
                if lang != "eng":
                    self.report.setdefault("unmatched_keywords", []).append(f"{lang}:{key}:{name}")
                continue
            out[f"{entry}.title"] = name
            out[f"{entry}.description"] = tidy(convert_inline(item.get("DESCRIPTION", "").replace(" NL ", "\n"), set()))
        return out

    def ui(self, lang):
        out = OrderedDict()
        for sid, item in load_strings(lang, "UIstrings.json").items():
            entry = ids.entry(sid)
            for i, t in enumerate(item.get("TEXT") or []):
                out[f"{entry}.text{i}"] = t
            for k, v in (item.get("TEXT_DICT") or {}).items():
                out[f"{entry}.{k}"] = v
        return out


def merge_table(path: Path, generated: "OrderedDict[str, str]", owned: dict):
    """Merge generated entries into a loc file.

    owned maps key -> the value this tool wrote last time. A key is regenerated only if it is new, or its
    current value is still exactly what we generated (i.e. nobody edited it by hand). Returns (merged, owned').
    """
    existing = OrderedDict()
    if path.exists():
        existing = json.loads(path.read_text(encoding="utf-8-sig") or "{}", object_pairs_hook=OrderedDict)
    merged = OrderedDict(existing)
    new_owned = {}
    for k, v in generated.items():
        if k in existing and owned.get(k) != value_hash(existing[k]):
            continue  # hand-written or hand-edited: keep
        merged[k] = v
        new_owned[k] = value_hash(v)
    return merged, new_owned


def value_hash(v) -> str:
    import hashlib
    return hashlib.sha1(str(v).encode("utf-8")).hexdigest()[:10]


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--lang", action="append", choices=LANGS)
    ap.add_argument("--report", action="store_true")
    ap.add_argument("--check", action="store_true", help="don't write; exit 1 if anything would change")
    ap.add_argument("--clean", action="store_true", help="first remove every key this tool generated before")
    args = ap.parse_args(argv)

    if args.clean and GENERATED_KEYS.exists():
        for key, owned in json.loads(GENERATED_KEYS.read_text()).items():
            path = DST / f"{key}.json"
            if not path.exists():
                continue
            data = json.loads(path.read_text(encoding="utf-8-sig") or "{}", object_pairs_hook=OrderedDict)
            owned = owned_of({key: owned}, key)
            for k, h in owned.items():
                if k in data and value_hash(data[k]) == h:
                    data.pop(k, None)  # hand-edited values survive --clean too
            if data:
                path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
            else:
                path.unlink()
        GENERATED_KEYS.unlink()

    langs = args.lang or LANGS
    if "eng" not in langs:
        langs = ["eng"] + langs  # English decides which keyword lines to strip
    owned_all = json.loads(GENERATED_KEYS.read_text()) if GENERATED_KEYS.exists() else {}
    report = {}
    conv = Converter(report)
    changed = []
    new_owned = dict(owned_all)

    for lang in langs:
        if not (SRC / lang).is_dir():
            continue
        kw = keyword_names(lang)
        tables = OrderedDict()
        tables["cards"] = conv.cards(lang, kw)
        powers = conv.fragments(lang, kw, "Powerstrings.json", "power")
        powers.update(conv.fragments(lang, kw, "Stancestrings.json", "power"))
        tables["powers"] = powers
        tables["relics"] = conv.fragments(lang, kw, "Relicstrings.json", "relic", flavor=True)
        tables["potions"] = conv.fragments(lang, kw, "Potionstrings.json", "potion")
        tables["orbs"] = conv.fragments(lang, kw, "Orbstrings.json", "orb")
        tables["static_hover_tips"] = conv.keywords(lang)
        tables["packmaster_ui"] = conv.ui(lang)

        for table, gen in tables.items():
            if not gen:
                continue
            path = DST / lang / f"{table}.json"
            key = f"{lang}/{table}"
            prev = owned_of(owned_all, key)
            merged, written = merge_table(path, gen, prev)
            # keep records of keys other tools (gen_cards.py) generated in this table
            kept = {k: h for k, h in prev.items() if k not in gen and k in merged}
            new_owned[key] = dict(sorted({**kept, **written}.items()))
            text = json.dumps(merged, ensure_ascii=False, indent=2) + "\n"
            old = path.read_text(encoding="utf-8-sig") if path.exists() else None
            if old != text:
                changed.append(str(path.relative_to(STS2)))
                if not args.check:
                    path.parent.mkdir(parents=True, exist_ok=True)
                    path.write_text(text, encoding="utf-8")

    if not args.check:
        GENERATED_KEYS.write_text(json.dumps(new_owned, indent=1, sort_keys=True) + "\n")
    if args.report:
        REPORT.parent.mkdir(parents=True, exist_ok=True)
        serial = {k: ({kk: sorted(vv) if isinstance(vv, set) else vv for kk, vv in v.items()} if isinstance(v, dict) else v)
                  for k, v in report.items()}
        REPORT.write_text(json.dumps(serial, ensure_ascii=False, indent=1, sort_keys=True) + "\n")
    print(("would change: " if args.check else "updated: ") + (", ".join(changed) if changed else "nothing"))
    return 1 if (args.check and changed) else 0


if __name__ == "__main__":
    sys.exit(main())
