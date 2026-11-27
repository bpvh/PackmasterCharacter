#!/usr/bin/env python3
"""Generate StS2 C# for Packmaster packs from the StS1 Java sources.

    python3 tools/gen_cards.py --list                       # packs, card counts, how many translate fully
    python3 tools/gen_cards.py --pack StrikesPack           # write that pack's cards into PackmasterCode/Cards/
    python3 tools/gen_cards.py --pack StrikesPack --dry-run # print what would be written
    python3 tools/gen_cards.py --pack-defs                  # (re)write every pack definition + preview card + loc
    python3 tools/gen_cards.py --report                     # write tools/out/cards_report.json

What it generates
  * Cards: one C# class per StS1 card, on BaseLib's ConstructedCardModel (via PackmasterCard). Cost, type,
    rarity, target, damage/block/magic numbers, upgrades, keywords and tags come from the Java constructor and
    upp(). Common use() statements (attack, block, draw, energy, vanilla powers) are translated; anything else
    becomes a "// TODO port:" line, with the full Java use() and other members kept as comments below.
    Each file's header says "Status: ported" (everything translated) or "skeleton" (needs hand porting).
  * Pack base cards (AbstractStrikePackCard, ...) as empty abstract classes with the Java members as comments.
  * Pack definitions (Packs/Definitions/<Pack>.cs): PackmasterPack subclass + its PackPreviewCard. CardTypes lists
    the pack's cards that exist in the project; Enabled is true only when all of them do, unless the file already
    says otherwise (hand edits win: Enabled is preserved).

Existing card files are never overwritten unless --force is given, so hand-ported cards are safe.
Loc for cards comes from tools/convert_loc.py; this script only adds the preview-card titles/descriptions.
"""
import argparse
import json
import re
import sys
from collections import OrderedDict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from porting import emit, ids, java  # noqa: E402

HERE = Path(__file__).resolve().parent
STS2 = HERE.parent
CODE = STS2 / "PackmasterCode"
LOC = STS2 / "Packmaster" / "localization"
REPORT = HERE / "out" / "cards_report.json"
SRC_LOC = STS2.parent / "src" / "main" / "resources" / "anniv5Resources" / "localization"


def load_packs():
    packs = []
    for p in sorted((java.ROOT / "packs").glob("*.java")):
        pk = java.parse_pack(p)
        if pk:
            packs.append(pk)
    return packs


def eng_descriptions():
    sys.path.insert(0, str(HERE))
    import convert_loc  # noqa: E402
    return convert_loc.load_strings("eng", "Cardstrings.json")


def existing_classes():
    """class name -> (namespace, path) for every C# class in the project."""
    out = {}
    for f in CODE.rglob("*.cs"):
        text = f.read_text(encoding="utf-8", errors="ignore")
        ns = re.search(r"^namespace\s+([\w.]+)\s*;", text, re.M)
        for m in re.finditer(r"\bclass\s+(\w+)", text):
            out.setdefault(m.group(1), (ns.group(1) if ns else "", f))
    return out


def plan_pack(pack, idx, strings):
    """Yield (kind, java_card, cs_path, cs_source, status) for a pack's cards and their base classes."""
    seen_bases = set()
    results = []
    for name in pack.card_classes:
        if name in pack.vanilla_cards:
            results.append(("vanilla", name, None, None, "vanilla"))
            continue
        path = java.resolve_class(name, pack.imports, idx)
        if not path:
            results.append(("missing", name, None, None, "missing"))
            continue
        card = java.parse_card(path)
        if not card or card.cost is None:
            results.append(("unparsed", name, path, None, "unparsed"))
            continue
        # base class chain
        base_cs, chain = base_for(card, idx)
        for base in chain:
            if base.cls in seen_bases:
                continue
            seen_bases.add(base.cls)
            parent_cs, _ = base_for(base, idx)
            ns = emit.namespace_for_package(base.package)
            src = emit.abstract_card_class(base, parent_cs, ns)
            results.append(("base", base, cs_path(ns, base.cls), src, "base"))
        ns = emit.namespace_for_package(card.package)
        eng = (strings.get(f"${{ModID}}:{card.sts1_id}") or {}).get("DESCRIPTION", "") if card.sts1_id else ""
        eng = eng.replace(" NL ", "\n")
        src, status = emit.card_class(card, base_cs, ns, eng)
        results.append(("card", card, cs_path(ns, ids.class_name(card.sts1_id) if card.sts1_id else card.cls), src, status))
    return results


def base_for(card, idx):
    """(C# base class reference, [abstract Java ancestors to generate, outermost last])."""
    chain = []
    cur = card
    while cur.extends not in ("AbstractPackmasterCard", "CustomCard", "AbstractCard"):
        path = java.resolve_class(cur.extends, {i.split(".")[-1]: i for i in cur.imports}, idx)
        parent = java.parse_card(path) if path else None
        if not parent:
            break
        chain.append(parent)
        cur = parent
    if card.extends in ("AbstractPackmasterCard", "CustomCard", "AbstractCard") or not chain:
        return "global::Packmaster.PackmasterCode.Cards.PackmasterCard", chain
    parent = chain[0]
    return f"global::{emit.namespace_for_package(parent.package)}.{parent.cls}", chain


def cs_path(namespace: str, cls: str) -> Path:
    rel = namespace.replace(emit.ROOT_NS + ".", "").split(".")
    return CODE.joinpath(*rel, f"{cls}.cs")


def write(path: Path, text: str, force: bool, dry: bool):
    if path.exists() and not force:
        return "kept"
    if dry:
        print(f"--- {path.relative_to(STS2)}\n{text}")
        return "dry"
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")
    return "written"


def pack_defs(packs, dry):
    classes = existing_classes()
    defs_dir = CODE / "Packs" / "Definitions"
    written = []
    for pk in packs:
        types, missing = [], []
        for name in pk.card_classes:
            if name in pk.vanilla_cards:
                missing.append(f"vanilla {name}")
                continue
            path = java.resolve_class(name, pk.imports, java.index_classes())
            card = java.parse_card(path) if path else None
            cls = ids.class_name(card.sts1_id) if card and card.sts1_id else name
            hit = classes.get(cls)
            if hit and "Cards" in hit[0]:
                types.append(f"global::{hit[0]}.{cls}")
            else:
                missing.append(cls)
        target = defs_dir / f"{ids.class_name(pk.pack_id)}.cs"
        enabled = bool(types) and not missing
        if target.exists():
            m = re.search(r"Enabled\s*=>\s*(true|false)", target.read_text())
            if m and m.group(1) == "false" and enabled:
                pass  # becomes enabled once every card exists
            elif m:
                enabled = enabled and m.group(1) == "true" or (m.group(1) == "true" and not missing)
        src = emit.pack_class(pk, types, enabled, missing)
        if dry:
            print(f"--- {target.relative_to(STS2)}\n{src}")
        else:
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_text(src, encoding="utf-8")
        written.append(target)
    if not dry:
        preview_loc(packs)
    return written


def preview_loc(packs):
    """PACKMASTER-<PACK>_PREVIEW.title/.description in cards.json, from the pack's UIstrings (name, desc, author)."""
    import convert_loc  # noqa: E402
    from porting.loc import convert_inline, tidy
    for lang in convert_loc.LANGS:
        ui = convert_loc.load_strings(lang, "UIstrings.json")
        eng_ui = convert_loc.load_strings("eng", "UIstrings.json")
        gen = OrderedDict()
        for pk in packs:
            item = ui.get(f"${{ModID}}:{pk.pack_id}") or eng_ui.get(f"${{ModID}}:{pk.pack_id}")
            if not item:
                continue
            text = item.get("TEXT") or []
            entry = ids.entry(pk.pack_id) + "_PREVIEW"
            if len(text) > 0:
                gen[f"{entry}.title"] = text[0]
            if len(text) > 1:
                desc = tidy(convert_inline(text[1].replace(" NL ", "\n"), set()))
                if len(text) > 2 and text[2]:
                    desc += f"\n[gray]{text[2]}[/gray]"
                gen[f"{entry}.description"] = desc
        path = LOC / lang / "cards.json"
        owned_all = json.loads(convert_loc.GENERATED_KEYS.read_text()) if convert_loc.GENERATED_KEYS.exists() else {}
        key = f"{lang}/cards"
        merged, wrote = convert_loc.merge_table(path, gen, set(owned_all.get(key, [])))
        owned_all[key] = sorted(set(owned_all.get(key, [])) | wrote)
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(json.dumps(merged, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        convert_loc.GENERATED_KEYS.write_text(json.dumps(owned_all, indent=1, sort_keys=True) + "\n")


def find_pack(packs, name):
    key = name.lower().replace("_", "")
    for pk in packs:
        if key in (pk.cls.lower(), pk.pack_id.lower()):
            return pk
    folder_hits = [pk for pk in packs if any(
        (java.resolve_class(c, pk.imports, {}) or Path()).parent.name.lower() == key for c in pk.card_classes)]
    return folder_hits[0] if folder_hits else None


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--pack", action="append", help="pack class/ID (StrikesPack) to generate cards for")
    ap.add_argument("--pack-defs", action="store_true", help="write all pack definitions and preview loc")
    ap.add_argument("--list", action="store_true")
    ap.add_argument("--report", action="store_true")
    ap.add_argument("--force", action="store_true", help="overwrite existing card files")
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args(argv)

    packs = load_packs()
    idx = java.index_classes()
    strings = eng_descriptions()

    if args.list or args.report:
        report = OrderedDict()
        for pk in packs:
            rows = plan_pack(pk, idx, strings)
            counts = {}
            for kind, _, _, _, status in rows:
                if kind != "base":
                    counts[status] = counts.get(status, 0) + 1
            report[pk.pack_id] = counts
            if args.list:
                print(f"{pk.pack_id:28} " + ", ".join(f"{k}: {v}" for k, v in sorted(counts.items())))
        if args.report:
            REPORT.parent.mkdir(parents=True, exist_ok=True)
            totals = {}
            for c in report.values():
                for k, v in c.items():
                    totals[k] = totals.get(k, 0) + v
            REPORT.write_text(json.dumps({"totals": totals, "packs": report}, indent=1) + "\n")
            print("totals:", totals)

    for name in args.pack or []:
        pk = find_pack(packs, name)
        if not pk:
            print(f"unknown pack {name}", file=sys.stderr)
            return 1
        for kind, card, path, src, status in plan_pack(pk, idx, strings):
            if src is None:
                print(f"  {kind:8} {card}")
                continue
            res = write(path, src, args.force, args.dry_run)
            print(f"  {status:8} {res:8} {path.relative_to(STS2)}")

    if args.pack_defs or args.pack:
        pack_defs(packs if args.pack_defs else [find_pack(packs, n) for n in args.pack], args.dry_run)
        print("pack definitions updated")
    return 0


if __name__ == "__main__":
    sys.exit(main())
