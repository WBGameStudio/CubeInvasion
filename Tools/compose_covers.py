#!/usr/bin/env python3
"""Builds the Playgama store covers from transparent renders of the game's own models.

Layers come from the Unity menu-less batch method PlaygamaCoverLayers.Run (Documentation/Playgama/Covers/layers).
Needs Pillow and numpy. Usage: python3 Tools/compose_covers.py
"""
import math, os, random
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont, ImageChops

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
COVERS = os.path.join(ROOT, "Documentation/Playgama/Covers")
LAYERS = os.path.join(COVERS, "layers")
FONT = os.path.join(ROOT, "Assets/CubeInvasion/Art/UI/Reference/CubeRounded-Bold.ttf")
SS = 2  # Supersampling; everything is drawn at twice the final size.

NAVY = (16, 14, 58)
CYAN = (46, 232, 255)
HOT = (255, 62, 110)
GOLD = (255, 205, 60)


def burst_background(w, h, cx, cy):
    """Hot core, magenta middle, deep violet edge, with sun rays: readable at thumbnail size."""
    y, x = np.mgrid[0:h, 0:w].astype(np.float32)
    dx, dy = x - cx, y - cy
    d = np.sqrt(dx * dx + dy * dy) / math.hypot(max(cx, w - cx), max(cy, h - cy))
    stops = [(0.0, (255, 214, 92)), (0.16, (255, 120, 70)), (0.38, (232, 40, 130)), (0.68, (112, 28, 190)), (1.0, (28, 14, 92))]
    img = np.zeros((h, w, 3), np.float32)
    for c in range(3):
        img[..., c] = np.interp(d, [s[0] for s in stops], [s[1][c] for s in stops])
    angle = np.arctan2(dy, dx)
    rays = (np.sin(angle * 14) > 0.35).astype(np.float32) * np.clip(1.15 - d * 1.05, 0, 1) * 0.16
    img = img + rays[..., None] * 255
    return Image.fromarray(np.clip(img, 0, 255).astype(np.uint8)).convert("RGBA")


def add_glow(base, mask, color, radius, strength=1.0):
    glow = mask.filter(ImageFilter.GaussianBlur(radius))
    glow = glow.point(lambda v: min(255, int(v * strength)))
    layer = Image.new("RGBA", base.size, color + (0,))
    layer.putalpha(glow)
    base.alpha_composite(layer)


def load_actor(name, height, flip=False, angle=0.0, rim=(255, 255, 255), rim_px=0.022, tint=None):
    """Crop a render, scale it to height, give it a sticker outline so it pops from the busy scene."""
    im = Image.open(os.path.join(LAYERS, name + ".png")).convert("RGBA")
    im = im.crop(im.getbbox())
    s = height / im.height
    im = im.resize((max(1, int(im.width * s)), int(height)), Image.LANCZOS)
    if tint:
        r, g, b, a = im.split()
        rgb = Image.merge("RGB", (r, g, b))
        rgb = Image.blend(rgb, Image.new("RGB", rgb.size, tint[0]), tint[1])
        im = Image.merge("RGBA", (*rgb.split(), a))
    if flip:
        im = im.transpose(Image.FLIP_LEFT_RIGHT)
    pad = int(height * rim_px * 2.4) + 4
    canvas = Image.new("RGBA", (im.width + pad * 2, im.height + pad * 2), (0, 0, 0, 0))
    canvas.paste(im, (pad, pad))
    alpha = canvas.getchannel("A")
    grown = alpha.filter(ImageFilter.GaussianBlur(height * rim_px * 0.75)).point(lambda v: 255 if v > 26 else 0)
    grown = grown.filter(ImageFilter.GaussianBlur(1.5 * SS))
    outline = Image.new("RGBA", canvas.size, rim + (0,))
    outline.putalpha(grown)
    outline.alpha_composite(canvas)
    return outline.rotate(angle, resample=Image.BICUBIC, expand=True)


def place(base, actor, cx, cy, shadow=True):
    x, y = int(cx - actor.width / 2), int(cy - actor.height / 2)
    if shadow:
        shade = Image.new("RGBA", base.size, (0, 0, 0, 0))
        tone = Image.new("RGBA", actor.size, NAVY + (0,))
        tone.putalpha(actor.getchannel("A").point(lambda v: int(v * 0.5)))
        shade.paste(tone, (x, y + int(actor.height * 0.035)))
        base.alpha_composite(shade.filter(ImageFilter.GaussianBlur(actor.height * 0.018)))
    paste_clipped(base, actor, x, y)


def paste_clipped(base, im, x, y):
    layer = Image.new("RGBA", base.size, (0, 0, 0, 0))
    layer.paste(im, (x, y))
    base.alpha_composite(layer)


def iso_platform(base, cx, cy, half_w, depth):
    """The floating cube: tile top with a cyan edge and dark sides, cracked chunks falling off the front corner."""
    half_h = half_w * 0.56
    top = [(cx, cy - half_h), (cx + half_w, cy), (cx, cy + half_h), (cx - half_w, cy)]
    d = ImageDraw.Draw(base)
    d.polygon([top[3], top[2], (cx, cy + half_h + depth), (cx - half_w, cy + depth)], fill=(38, 34, 112))
    d.polygon([top[1], top[2], (cx, cy + half_h + depth), (cx + half_w, cy + depth)], fill=(24, 22, 84))
    d.polygon(top, fill=(214, 226, 248))
    n = 5
    for i in range(1, n):
        t = i / n
        a = (cx - half_w + half_w * t, cy - half_h * t)
        b = (cx + half_w * t, cy + half_h - half_h * t)
        d.line([a, b], fill=(150, 168, 214), width=max(2, int(half_w * 0.006)))
        a = (cx + half_w - half_w * t, cy - half_h * t)
        b = (cx - half_w * t, cy + half_h - half_h * t)
        d.line([a, b], fill=(150, 168, 214), width=max(2, int(half_w * 0.006)))
    edge = max(4, int(half_w * 0.016))
    d.line(top + [top[0]], fill=CYAN, width=edge, joint="curve")
    # Panels on the sides read as machinery rather than a flat block.
    for side, sx in ((-1, -half_w), (1, half_w)):
        for i in range(1, 4):
            t = i / 4
            px = cx + sx * t
            py = cy + half_h * (1 - t)
            d.line([(px, py + depth * 0.14), (px, py + depth * 0.86)], fill=(70, 66, 160), width=max(2, int(half_w * 0.007)))
    # Falling chunks: the cube is collapsing, which is the game's hook.
    rng = random.Random(7)
    for i in range(9):
        t = rng.uniform(-0.75, 0.75)
        s = half_w * rng.uniform(0.03, 0.075)
        px = cx + half_w * t
        py = cy + half_h * (1 - abs(t)) + depth * rng.uniform(1.15, 2.3)
        cube(d, px, py, s)


def cube(d, x, y, s, top=(214, 226, 248), left=(62, 58, 150), right=(38, 34, 112)):
    h = s * 0.56
    d.polygon([(x, y - h), (x + s, y), (x, y + h), (x - s, y)], fill=top)
    d.polygon([(x - s, y), (x, y + h), (x, y + h + s), (x - s, y + s)], fill=left)
    d.polygon([(x + s, y), (x, y + h), (x, y + h + s), (x + s, y + s)], fill=right)


def laser(base, a, b, width, color=CYAN):
    mask = Image.new("L", base.size, 0)
    d = ImageDraw.Draw(mask)
    d.line([a, b], fill=255, width=int(width))
    add_glow(base, mask, color, width * 1.5, 1.8)
    core = Image.new("RGBA", base.size, color + (0,))
    core.putalpha(mask)
    base.alpha_composite(core)
    inner = Image.new("L", base.size, 0)
    ImageDraw.Draw(inner).line([a, b], fill=255, width=max(2, int(width * 0.45)))
    white = Image.new("RGBA", base.size, (255, 255, 255, 0))
    white.putalpha(inner)
    base.alpha_composite(white)


def star(cx, cy, outer, inner, points, spin=0.0, jitter=0.0, seed=1):
    rng = random.Random(seed)
    out = []
    for i in range(points * 2):
        r = outer if i % 2 == 0 else inner
        r *= 1 + rng.uniform(-jitter, jitter)
        a = spin + math.pi * i / points
        out.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
    return out


def blast(base, cx, cy, size, seed=3):
    mask = Image.new("L", base.size, 0)
    ImageDraw.Draw(mask).polygon(star(cx, cy, size, size * 0.56, 11, 0.2, 0.18, seed), fill=255)
    add_glow(base, mask, (255, 150, 40), size * 0.24, 1.5)
    d = ImageDraw.Draw(base)
    d.polygon(star(cx, cy, size, size * 0.56, 11, 0.2, 0.18, seed), fill=(255, 92, 42), outline=NAVY)
    d.polygon(star(cx, cy, size * 0.74, size * 0.42, 11, 0.45, 0.14, seed + 1), fill=(255, 176, 42))
    d.polygon(star(cx, cy, size * 0.46, size * 0.24, 9, 0.1, 0.1, seed + 2), fill=(255, 246, 190))
    rng = random.Random(seed + 5)
    for _ in range(8):
        a = rng.uniform(0, math.tau)
        r = size * rng.uniform(1.05, 1.55)
        cube(d, cx + math.cos(a) * r, cy + math.sin(a) * r, size * rng.uniform(0.05, 0.1), (86, 92, 128), (52, 56, 92), (34, 36, 70))


def coin(base, cx, cy, r, squash=1.0):
    d = ImageDraw.Draw(base)
    w, h = r * squash, r
    d.ellipse([cx - w - r * 0.14, cy - h - r * 0.14, cx + w + r * 0.14, cy + h + r * 0.14], fill=NAVY)
    d.ellipse([cx - w, cy - h, cx + w, cy + h], fill=(255, 176, 34))
    d.ellipse([cx - w * 0.74, cy - h * 0.74, cx + w * 0.74, cy + h * 0.74], fill=GOLD)
    d.rectangle([cx - w * 0.14, cy - h * 0.42, cx + w * 0.14, cy + h * 0.42], fill=(255, 150, 28))


def gradient_text_layer(text, font, top, bottom):
    x0, y0, x1, y1 = font.getbbox(text)
    w, h = x1 - x0, y1 - y0
    mask = Image.new("L", (w + 40, h + 40), 0)
    ImageDraw.Draw(mask).text((20 - x0, 20 - y0), text, font=font, fill=255)
    ramp = np.linspace(0, 1, mask.height)[..., None]
    rgb = (1 - ramp) * np.array(top, np.float32) + ramp * np.array(bottom, np.float32)
    rgb = np.repeat(rgb[:, None, :], mask.width, axis=1).astype(np.uint8)
    fill = Image.fromarray(rgb).convert("RGBA")
    fill.putalpha(mask)
    return fill, mask


def logo(width, stacked=True):
    """Chunky two-tone wordmark: gold CUBE, cyan INVASION, navy extrusion and a white rim."""
    lines = [("CUBE", (255, 240, 120), (255, 150, 30)), ("INVASION", (190, 250, 255), (30, 170, 255))]
    base_size = int(width * (0.33 if stacked else 0.15))
    rendered = []
    for i, (text, top, bottom) in enumerate(lines):
        # Both lines are fitted to the same width, so the short word gets the big, poster-like size.
        size = base_size
        font = ImageFont.truetype(FONT, size)
        x0, _, x1, _ = font.getbbox(text)
        target = width * 0.88 if stacked else width * (0.34 if i == 0 else 0.62)
        size = int(size * target / (x1 - x0))
        if stacked and i == 0:
            size = int(size * 0.72)
        font = ImageFont.truetype(FONT, size)
        rendered.append(gradient_text_layer(text, font, top, bottom))
    gap = int(width * 0.012)
    if stacked:
        total_w = max(r[0].width for r in rendered)
        total_h = sum(r[0].height for r in rendered) - gap
    else:
        total_w = sum(r[0].width for r in rendered)
        total_h = max(r[0].height for r in rendered)
    pad = int(width * 0.09)
    sheet_mask = Image.new("L", (total_w + pad * 2, total_h + pad * 2), 0)
    sheet_fill = Image.new("RGBA", sheet_mask.size, (0, 0, 0, 0))
    x, y = pad, pad
    for fill, mask in rendered:
        ox = pad + (total_w - fill.width) // 2 if stacked else x
        oy = y if stacked else pad + (total_h - fill.height) // 2
        sheet_mask.paste(ImageChops.lighter(sheet_mask.crop((ox, oy, ox + mask.width, oy + mask.height)), mask), (ox, oy))
        sheet_fill.alpha_composite(fill, (ox, oy))
        x += fill.width
        y += fill.height - gap
    stroke = int(width * 0.012) + 2
    rim = sheet_mask.filter(ImageFilter.GaussianBlur(stroke * 0.8)).point(lambda v: 255 if v > 22 else 0).filter(ImageFilter.GaussianBlur(1))
    heavy = sheet_mask.filter(ImageFilter.GaussianBlur(stroke * 1.9)).point(lambda v: 255 if v > 18 else 0).filter(ImageFilter.GaussianBlur(1))
    out = Image.new("RGBA", sheet_mask.size, (0, 0, 0, 0))
    depth = int(width * 0.03)
    shade = Image.new("RGBA", sheet_mask.size, NAVY + (255,))
    for i in range(depth, -1, -1):
        layer = Image.new("RGBA", sheet_mask.size, (0, 0, 0, 0))
        layer.paste(shade, (0, i), heavy)
        out.alpha_composite(layer)
    white = Image.new("RGBA", sheet_mask.size, (255, 255, 255, 255))
    layer = Image.new("RGBA", sheet_mask.size, (0, 0, 0, 0))
    layer.paste(white, (0, 0), rim)
    out.alpha_composite(layer)
    out.alpha_composite(sheet_fill)
    return out.rotate(3.5, resample=Image.BICUBIC, expand=True)


def scene(w, h, spec):
    W, H = w * SS, h * SS
    u = min(W, H)
    hero_x, hero_y = W * spec["hero"][0], H * spec["hero"][1]
    base = burst_background(W, H, int(hero_x), int(hero_y))
    px, py = W * spec["platform"][0], H * spec["platform"][1]
    iso_platform(base, px, py, u * spec["platform"][2], u * spec["platform"][3])

    # Far enemies first, so the action layers front to back.
    actors = []
    for name, ex, ey, height, flip, angle in spec["enemies"]:
        actors.append((ey, name, W * ex, H * ey, u * height, flip, angle))
    actors.sort(key=lambda a: a[0])

    hero = load_actor("hero", u * spec["hero"][2], flip=spec.get("hero_flip", True), angle=spec.get("hero_angle", 0), rim=(255, 255, 255), rim_px=0.026)
    # The cannon hand sits on the side the render was flipped to; lasers leave from there.
    gun = (hero_x + spec["gun"][0] * u * spec["hero"][2], hero_y + spec["gun"][1] * u * spec["hero"][2])

    for _, name, x, y, height, flip, angle in actors:
        enemy = load_actor(name, height, flip=flip, angle=angle, rim=(255, 74, 104), rim_px=0.03, tint=((70, 40, 110), 0.12))
        place(base, enemy, x, y)

    for target in spec["targets"]:
        tx, ty = W * target[0], H * target[1]
        laser(base, gun, (tx, ty), u * 0.016)
        blast(base, tx, ty, u * target[2], seed=int(tx) % 97 + 2)

    for cx, cy, r, squash in spec["coins"]:
        coin(base, W * cx, H * cy, u * r, squash)

    # Hero glow lifts the character off the burst.
    glow_mask = Image.new("L", base.size, 0)
    glow_mask.paste(hero.getchannel("A"), (int(hero_x - hero.width / 2), int(hero_y - hero.height / 2)))
    add_glow(base, glow_mask, (255, 255, 220), u * 0.04, 1.1)
    place(base, hero, hero_x, hero_y)
    muzzle = Image.new("L", base.size, 0)
    ImageDraw.Draw(muzzle).polygon(star(gun[0], gun[1], u * 0.06, u * 0.02, 8, 0.2), fill=255)
    add_glow(base, muzzle, CYAN, u * 0.024, 1.8)
    ImageDraw.Draw(base).polygon(star(gun[0], gun[1], u * 0.052, u * 0.018, 8, 0.2), fill=(240, 255, 255))

    mark = logo(int(W * spec["logo"][2]), spec.get("stacked", True))
    lx, ly = int(W * spec["logo"][0] - mark.width / 2), int(H * spec["logo"][1] - mark.height / 2)
    shade = Image.new("RGBA", base.size, (0, 0, 0, 0))
    shade.paste(Image.new("RGBA", mark.size, (20, 6, 70, 150)), (lx, ly + int(u * 0.012)), mark.getchannel("A"))
    base.alpha_composite(shade.filter(ImageFilter.GaussianBlur(u * 0.016)))
    paste_clipped(base, mark, lx, ly)

    # Vignette keeps the eye in the middle.
    vy, vx = np.mgrid[0:H, 0:W].astype(np.float32)
    v = np.sqrt(((vx - W / 2) / (W / 2)) ** 2 + ((vy - H / 2) / (H / 2)) ** 2)
    alpha = np.clip((v - 0.95) * 0.55, 0, 0.45) * 255
    vignette = Image.new("RGBA", base.size, (16, 6, 60, 0))
    vignette.putalpha(Image.fromarray(alpha.astype(np.uint8)))
    base.alpha_composite(vignette)
    return base.convert("RGB").resize((w, h), Image.LANCZOS)


SPECS = {
    "cover_landscape_1920x1080.png": (1920, 1080, {
        "hero": (0.635, 0.55, 0.68), "gun": (-0.42, -0.02), "hero_flip": False,
        "platform": (0.64, 0.74, 0.60, 0.17),
        "enemies": [
            ("crusher", 0.885, 0.27, 0.33, True, -6), ("sprinter", 0.91, 0.75, 0.31, True, 8),
            ("gunner", 0.37, 0.80, 0.25, False, 4), ("grunt", 0.49, 0.90, 0.20, False, -5),
        ],
        "targets": [(0.30, 0.74, 0.11)],
        "coins": [(0.50, 0.18, 0.034, 0.7), (0.82, 0.13, 0.03, 1.0), (0.96, 0.60, 0.028, 0.5), (0.27, 0.93, 0.03, 0.8)],
        "logo": (0.255, 0.30, 0.47), "stacked": True,
    }),
    "cover_square_800x800.png": (800, 800, {
        "hero": (0.50, 0.665, 0.50), "gun": (-0.42, -0.02), "hero_flip": False,
        "platform": (0.50, 0.84, 0.50, 0.11),
        "enemies": [("crusher", 0.87, 0.54, 0.30, True, -6), ("sprinter", 0.83, 0.86, 0.22, True, 8), ("grunt", 0.27, 0.91, 0.17, False, -5)],
        "targets": [(0.115, 0.62, 0.105)],
        "coins": [(0.10, 0.42, 0.035, 0.7), (0.93, 0.33, 0.03, 1.0)],
        "logo": (0.50, 0.215, 0.74), "stacked": True,
    }),
    "cover_portrait_1080x1920.png": (1080, 1920, {
        "hero": (0.52, 0.56, 0.74), "gun": (-0.42, -0.02), "hero_flip": False,
        "platform": (0.50, 0.73, 0.66, 0.20),
        "enemies": [
            ("crusher", 0.81, 0.34, 0.36, True, -6), ("gunner", 0.20, 0.84, 0.27, False, 4),
            ("sprinter", 0.83, 0.86, 0.32, True, 8), ("grunt", 0.50, 0.92, 0.22, False, -4),
        ],
        "targets": [(0.135, 0.39, 0.13)],
        "coins": [(0.11, 0.60, 0.036, 0.7), (0.92, 0.55, 0.03, 1.0), (0.32, 0.30, 0.03, 0.5)],
        "logo": (0.50, 0.165, 0.84), "stacked": True,
    }),
}

if __name__ == "__main__":
    for name, (w, h, spec) in SPECS.items():
        path = os.path.join(COVERS, name)
        scene(w, h, spec).save(path, optimize=True)
        print(name, os.path.getsize(path))
