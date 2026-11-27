#!/usr/bin/env python3
"""Check that every C# model in PackmasterCode has its English loc, and that card text only uses variables the
card declares. Run after porting a pack:

    python3 tools/check_loc.py            # report problems, exit 1 if any
    python3 tools/check_loc.py --all      # also report cards in disabled packs (skeletons)

Checks
  * cards (classes deriving from PackmasterCard / a pack base card): <ENTRY>.title and .description in cards.json
  * powers (PackmasterPower, CustomTemporaryPowerModelWrapper are skipped: BaseLib generates their text):
    .title and .description in powers.json
  * relics (PackmasterRelic subclasses): .title, .description in relics.json
  * {Var:...} in a card description must be one of the card's declared vars (WithDamage -> Damage, WithBlock ->
    Block, WithVar("X") -> X, WithCards -> Cards, WithEnergy -> Energy, WithCalculatedDamage -> CalculatedDamage +
    ExtraDamage, ...). IfUpgraded and energyPrefix are always allowed.

The C# side is read with regexes (class name, base class, [CustomID], With* calls), which is enough for the
conventions the generator and the ported code follow.
"""
import argparse
import json
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from porting import ids  # noqa: E402

HERE = Path(__file__).resolve().parent
STS2 = HERE.parent
CODE = STS2 / "PackmasterCode"
LOC = STS2 / "Packmaster" / "localization" / "eng"

ALWAYS = {"IfUpgraded", "energyPrefix"}
WITH_VARS = {
    "WithDamage": ["Damage"], "WithBlock": ["Block"], "WithCards": ["Cards"], "WithEnergy": ["Energy"],
    "WithHeal": ["Heal"], "WithCalculatedDamage": ["CalculatedDamage", "ExtraDamage", "CalculationBase"],
    "WithCalculatedBlock": ["CalculatedBlock", "CalculationExtra", "CalculationBase"],
}


def load(table):
    path = LOC / f"{table}.json"
    return json.loads(path.read_text(encoding="utf-8-sig")) if path.exists() else {}


def classes():
    for f in CODE.rglob("*.cs"):
        text = f.read_text(encoding="utf-8", errors="ignore")
        for m in re.finditer(r"(?:\[[^\]]*CustomID\(\"([^\"]+)\"\)\]\s*)?public\s+(?:sealed\s+)?class\s+(\w+)\s*(?:\([^)]*\))?\s*:\s*([\w.:<>, ]+)", text):
            custom, name, base = m.group(1), m.group(2), m.group(3).strip()
            body_start = text.find("{", m.end())
            yield f, name, base.split(",")[0].strip(), custom, text[body_start:], text


GLOBAL_CONSTS = {}  # const string Name = "Value" from every file (base classes define var names)


def declared_vars(body: str):
    out = set(ALWAYS)
    for method, names in WITH_VARS.items():
        if re.search(rf"\b{method}\(", body):
            out.update(names)
    out.update(re.findall(r'WithVar\(\s*"(\w+)"', body))
    consts = {**GLOBAL_CONSTS, **dict(re.findall(r'const\s+string\s+(\w+)\s*=\s*"(\w+)"', body))}
    out.update(consts[c] for c in re.findall(r'WithVar\(\s*(\w+)\s*,', body) if c in consts)
    out.update(re.findall(r'WithPower<(\w+)>\(\s*"(\w+)"', body) and [n for _, n in re.findall(r'WithPower<(\w+)>\(\s*"(\w+)"', body)] or [])
    out.update(p for p in re.findall(r"WithPower<(\w+)>\(\s*\d", body))
    if "new ExhaustiveVar(" in body:
        out.add("Exhaustive")
    if "new PersistVar(" in body:
        out.add("Persist")
    return out


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--all", action="store_true")
    args = ap.parse_args(argv)

    cards, powers, relics = load("cards"), load("powers"), load("relics")
    for f in CODE.rglob("*.cs"):
        GLOBAL_CONSTS.update(re.findall(r'const\s+string\s+(\w+)\s*=\s*"(\w+)"', f.read_text(errors="ignore")))
    problems = []
    for path, name, base, custom, body, text in classes():
        rel = path.relative_to(STS2)
        if "abstract class" in text.split(f"class {name}")[0][-40:]:
            continue
        entry = custom or ids.entry(name, is_class=True)
        if "Status: skeleton" in text and not args.all:
            continue
        is_card = "Cards/" in str(rel) and "Card" not in name[-4:] or base.endswith("Card") or base.endswith("PackPreviewCard")
        if base.endswith("PackPreviewCard") or base == "PackmasterPack":
            continue
        if "Powers/" in str(rel):
            if "CustomTemporaryPowerModelWrapper" in base:
                continue
            for suffix in ("title", "description"):
                if f"{entry}.{suffix}" not in powers:
                    problems.append(f"{rel}: missing powers.json {entry}.{suffix}")
        elif "Relics/" in str(rel):
            if base in ("PackmasterRelic", "CustomRelicModel") and name == "PackDraftRelic":
                continue
            for suffix in ("title", "description"):
                if f"{entry}.{suffix}" not in relics:
                    problems.append(f"{rel}: missing relics.json {entry}.{suffix}")
        elif "Cards/" in str(rel) and is_card:
            for suffix in ("title", "description"):
                if f"{entry}.{suffix}" not in cards:
                    problems.append(f"{rel}: missing cards.json {entry}.{suffix}")
            desc = cards.get(f"{entry}.description", "")
            used = set(re.findall(r"\{(\w+)[:}]", desc))
            unknown = used - declared_vars(body)
            if unknown:
                problems.append(f"{rel}: description uses undeclared var(s) {sorted(unknown)}: {desc[:120]!r}")
            if "SelectionScreenPrompt" in body and f"{entry}.selectionScreenPrompt" not in cards:
                problems.append(f"{rel}: uses SelectionScreenPrompt but cards.json has no {entry}.selectionScreenPrompt")
    for p in problems:
        print(p)
    print(f"{len(problems)} problem(s)")
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
