#!/usr/bin/env python3
"""Copy StS1 Packmaster art into the StS2 Godot asset folders.

The StS1 resources (../src/main/resources/anniv5Resources) stay the single source of truth,
so the ~120 MB of card art isn't duplicated in git. Run this before building the .pck:

    python3 tools/sync_assets.py            # copy everything (skips unchanged files)
    python3 tools/sync_assets.py --clean    # remove previously synced files first

Mapping (StS1 -> StS2, relative to Packmaster/images):
    cards/{Name}.png      (250x190) -> card_portraits/{Name}.png
    cards/{Name}_p.png    (500x380) -> card_portraits/big/{Name}.png
    powers/{Name}32.png / {Name}84.png -> powers/ (same names)
    relics/{Name}.png, {Name}Outline.png -> relics/ ; {Name}.png also -> relics/big/
    potions are drawn procedurally in StS1, so there is nothing to copy.
    char/mainChar/orb/*  -> charui/energy/ (energy counter layers, used in Phase 4)
    charSelect/charButton.png -> charui/char_select_packmaster(.png|_locked.png), character_icon_packmaster.png,
                                 map_marker_packmaster.png (resized; these are tracked in git)

Synced files are listed in .synced_assets so --clean only removes what this script created;
the template placeholders (card.png, power.png, relic.png, ...) are never touched.
"""
import argparse
import filecmp
import shutil
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
STS2 = HERE.parent
SRC = STS2.parent / "src" / "main" / "resources" / "anniv5Resources" / "images"
DST = STS2 / "Packmaster" / "images"
MANIFEST = STS2 / ".synced_assets"


def plan():
    """Yield (src, dst) pairs."""
    cards = SRC / "cards"
    for f in sorted(cards.glob("*.png")):
        if f.stem.endswith("_p"):
            yield f, DST / "card_portraits" / "big" / f"{f.stem[:-2]}.png"
        else:
            yield f, DST / "card_portraits" / f.name
    for f in sorted((SRC / "powers").glob("*.png")):
        yield f, DST / "powers" / f.name
    for f in sorted((SRC / "relics").glob("*.png")):
        yield f, DST / "relics" / f.name
        if not f.stem.endswith("Outline"):
            yield f, DST / "relics" / "big" / f.name
    for f in sorted((SRC / "char" / "mainChar" / "orb").glob("*.png")):
        yield f, DST / "charui" / "energy" / f.name
    # Gems pack rest site option (StS1 EnhanceBonfireOption)
    rest = DST / "ui" / "rest_site"
    yield SRC / "ui" / "scrapcampfire.png", rest / "option_packmaster_enhance.png"
    yield SRC / "ui" / "scrapcampfiredisabled.png", rest / "option_packmaster_enhance_disabled.png"


def make_character_ui(button: Path, out_dir: Path, dry_run: bool):
    """Derive the character-select icon (132x195, as in the template), its locked variant,
    and the 128x128 character icon / map marker from the StS1 200x200 char select button."""
    from PIL import Image, ImageEnhance
    im = Image.open(button).convert("RGBA")
    w, h = im.size
    tw, th = 132, 195
    crop_w = round(h * tw / th)
    left = (w - crop_w) // 2
    select = im.crop((left, 0, left + crop_w, h)).resize((tw, th), Image.LANCZOS)
    r, g, b, a = select.split()
    grey = ImageEnhance.Brightness(Image.merge("RGB", (r, g, b)).convert("L").convert("RGB")).enhance(0.35)
    locked = Image.merge("RGBA", (*grey.split(), a))
    icon = im.resize((128, 128), Image.LANCZOS)
    outputs = {
        "char_select_packmaster.png": select,
        "char_select_packmaster_locked.png": locked,
        "character_icon_packmaster.png": icon,
        "map_marker_packmaster.png": icon,
    }
    if not dry_run:
        out_dir.mkdir(parents=True, exist_ok=True)
        for name, img in outputs.items():
            img.save(out_dir / name)
    return list(outputs)


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--clean", action="store_true", help="delete previously synced files first")
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args(argv)

    if not SRC.is_dir():
        print(f"StS1 resources not found at {SRC}", file=sys.stderr)
        return 1

    if args.clean and MANIFEST.exists():
        for line in MANIFEST.read_text().splitlines():
            p = STS2 / line
            if p.is_file() and not args.dry_run:
                p.unlink()
        if not args.dry_run:
            MANIFEST.unlink()

    written, skipped = [], 0
    for src, dst in plan():
        rel = dst.relative_to(STS2).as_posix()
        written.append(rel)
        if dst.exists() and filecmp.cmp(src, dst, shallow=False):
            skipped += 1
            continue
        if not args.dry_run:
            dst.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(src, dst)

    # Character UI images are generated, small, and tracked in git (so a fresh clone runs without syncing).
    button = SRC / "charSelect" / "charButton.png"
    if button.exists():
        try:
            make_character_ui(button, DST / "charui", args.dry_run)
        except ImportError:
            print("Pillow not installed; skipping character UI images", file=sys.stderr)

    if not args.dry_run:
        MANIFEST.write_text("\n".join(sorted(set(written))) + "\n")
    print(f"{len(written)} files mapped, {skipped} unchanged{' (dry run)' if args.dry_run else ''}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
