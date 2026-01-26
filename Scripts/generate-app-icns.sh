#!/bin/bash
#
# Generates app.icns for macOS from existing PNG icon set.
# Must be run on macOS (uses iconutil).
#
# Usage: ./generate-app-icns.sh
#

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

ICON_SET_DIR="$REPO_ROOT/Assets/AppIcon/MacOS/AppIconSet"
OUTPUT_ICNS="$REPO_ROOT/Assets/AppIcon/MacOS/app.icns"
ICONSET_DIR="$REPO_ROOT/Assets/AppIcon/MacOS/app.iconset"

# Check if running on macOS
if [[ "$(uname)" != "Darwin" ]]; then
    echo "ERROR: This script must be run on macOS (requires iconutil)."
    echo ""
    echo "To generate app.icns on macOS:"
    echo "  1. Copy this repository to a Mac"
    echo "  2. Run: ./Scripts/generate-app-icns.sh"
    echo ""
    echo "Alternatively, use a cross-platform tool like png2icns:"
    echo "  npm install -g png2icns"
    echo "  png2icns Assets/AppIcon/MacOS/app.icns Assets/AppIcon/MacOS/AppIconSet/*.png"
    exit 1
fi

# Check if iconutil is available
if ! command -v iconutil &> /dev/null; then
    echo "ERROR: iconutil not found. This tool comes with Xcode Command Line Tools."
    echo "Install with: xcode-select --install"
    exit 1
fi

# Check if source PNGs exist
if [[ ! -d "$ICON_SET_DIR" ]]; then
    echo "ERROR: Icon set directory not found: $ICON_SET_DIR"
    exit 1
fi

echo "Generating app.icns from AppIconSet..."
echo "Source: $ICON_SET_DIR"
echo "Output: $OUTPUT_ICNS"

# Create .iconset directory structure
rm -rf "$ICONSET_DIR"
mkdir -p "$ICONSET_DIR"

# macOS iconset requires specific naming convention:
# icon_16x16.png, icon_16x16@2x.png (32px), icon_32x32.png, icon_32x32@2x.png (64px), etc.

echo "  Copying and renaming PNGs..."

# 16x16
cp "$ICON_SET_DIR/16.png" "$ICONSET_DIR/icon_16x16.png"
cp "$ICON_SET_DIR/32.png" "$ICONSET_DIR/icon_16x16@2x.png"

# 32x32
cp "$ICON_SET_DIR/32.png" "$ICONSET_DIR/icon_32x32.png"
cp "$ICON_SET_DIR/64.png" "$ICONSET_DIR/icon_32x32@2x.png"

# 128x128
cp "$ICON_SET_DIR/128.png" "$ICONSET_DIR/icon_128x128.png"
cp "$ICON_SET_DIR/256.png" "$ICONSET_DIR/icon_128x128@2x.png"

# 256x256
cp "$ICON_SET_DIR/256.png" "$ICONSET_DIR/icon_256x256.png"
cp "$ICON_SET_DIR/512.png" "$ICONSET_DIR/icon_256x256@2x.png"

# 512x512
cp "$ICON_SET_DIR/512.png" "$ICONSET_DIR/icon_512x512.png"
cp "$ICON_SET_DIR/1024.png" "$ICONSET_DIR/icon_512x512@2x.png"

echo "  Running iconutil..."

# Generate icns
iconutil -c icns "$ICONSET_DIR" -o "$OUTPUT_ICNS"

# Cleanup
rm -rf "$ICONSET_DIR"

echo ""
echo "SUCCESS: app.icns created at $OUTPUT_ICNS"
