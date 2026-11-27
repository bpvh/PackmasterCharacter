#!/usr/bin/env python3
"""Draw power icons for StS1 stances (StS1 showed stances as an aura, not a power icon).

    python3 tools/make_stance_icons.py

Writes Packmaster/images/stances/{Name}32.png and {Name}84.png (tracked in git). Add a stance to STANCES as
packs need it: (name, RGB colour).
"""
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

OUT = Path(__file__).resolve().parent.parent / "Packmaster" / "images" / "stances"
STANCES = [
    ("Aggression", (230, 40, 30)),
]


def icon(size: int, rgb) -> Image.Image:
    scale = 4
    s = size * scale
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    pad = s * 0.12
    w = max(scale, int(s * 0.09))
    # outer glow ring
    d.ellipse([pad, pad, s - pad, s - pad], outline=rgb + (255,), width=w)
    # inner flame-ish diamond
    c = s / 2
    r = s * 0.22
    d.polygon([(c, c - r * 1.4), (c + r, c), (c, c + r * 1.2), (c - r, c)], fill=rgb + (230,))
    glow = img.filter(ImageFilter.GaussianBlur(s * 0.04))
    out = Image.alpha_composite(glow, img)
    return out.resize((size, size), Image.LANCZOS)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for name, rgb in STANCES:
        for size in (32, 84):
            icon(size, rgb).save(OUT / f"{name}{size}.png")
            print(OUT / f"{name}{size}.png")


if __name__ == "__main__":
    main()
