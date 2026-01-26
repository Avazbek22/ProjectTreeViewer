#!/usr/bin/env python3
"""
Generates app.ico for Windows from the master app icon PNG.

Features:
- Creates multi-resolution ICO with 7 sizes
- Preserves transparency
- Makes image square if needed (centers content)

Requires: pip install Pillow

Usage: python generate-app-ico.py
"""

import sys
import struct
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("ERROR: Pillow library is not installed.")
    print("")
    print("Install it with:")
    print("  pip install Pillow")
    sys.exit(1)


def make_square(img: Image.Image) -> Image.Image:
    """
    Make image square by adding minimal transparent padding if needed.
    Centers the content in the square canvas.
    """
    width, height = img.size

    if width == height:
        return img

    # Use the larger dimension
    new_size = max(width, height)

    # Create transparent canvas
    square = Image.new('RGBA', (new_size, new_size), (0, 0, 0, 0))

    # Center the image
    x = (new_size - width) // 2
    y = (new_size - height) // 2

    square.paste(img, (x, y), img if img.mode == 'RGBA' else None)

    return square


def main():
    script_dir = Path(__file__).parent.resolve()
    repo_root = script_dir.parent

    source_png = repo_root / "Assets" / "AppIcon" / "Source" / "appicon-master.png"
    output_ico = repo_root / "Assets" / "AppIcon" / "Windows" / "app.ico"

    if not source_png.exists():
        print(f"ERROR: Source image not found: {source_png}")
        sys.exit(1)

    print("=" * 60)
    print("Generating app.ico")
    print("=" * 60)
    print(f"Source: {source_png}")
    print(f"Output: {output_ico}")
    print()

    # ICO sizes for Windows
    sizes = [16, 24, 32, 48, 64, 128, 256]

    # Load source image
    img = Image.open(source_png)
    print(f"Source: {img.size[0]}x{img.size[1]}, mode: {img.mode}")

    # Ensure RGBA
    if img.mode != 'RGBA':
        img = img.convert('RGBA')

    # Make square if needed
    if img.size[0] != img.size[1]:
        print(f"Making square...")
        img = make_square(img)
        print(f"  Result: {img.size[0]}x{img.size[1]}")

    # Create resized versions
    print("Creating icon sizes...")
    icons = []
    for size in sizes:
        resized = img.resize((size, size), Image.Resampling.LANCZOS)
        icons.append(resized)
        print(f"  {size}x{size}")

    # Save as multi-resolution ICO
    print("Saving ICO...")

    # Largest first, then append smaller
    icons_desc = list(reversed(icons))
    icons_desc[0].save(
        output_ico,
        format='ICO',
        append_images=icons_desc[1:]
    )

    # Verify
    print()
    print("=" * 60)
    print("SUCCESS!")
    print("=" * 60)
    print(f"File: {output_ico}")
    print(f"Size: {output_ico.stat().st_size:,} bytes")
    print()
    print("Embedded sizes:")

    with open(output_ico, 'rb') as f:
        f.read(6)
        for i in range(7):
            entry = f.read(16)
            w, h, _, _, _, bpp, size, _ = struct.unpack('<BBBBHHII', entry)
            w = w if w != 0 else 256
            usage = " (taskbar)" if w in [32, 48] else ""
            usage = " (Alt+Tab)" if w >= 64 else usage
            print(f"  {w}x{w}, {bpp}bpp, {size:,} bytes{usage}")


if __name__ == "__main__":
    main()
