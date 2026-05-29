#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

APP_NAME="ArtFruit"
BUILD_DIR=".build"
APP_BUNDLE="$BUILD_DIR/$APP_NAME.app"
CONTENTS="$APP_BUNDLE/Contents"
MACOS="$CONTENTS/MacOS"
RESOURCES="$CONTENTS/Resources"

echo "=> Building $APP_NAME with swift build..."
swift build -c release 2>&1
echo "==> Compilation successful."

# Create .app bundle structure
rm -rf "$APP_BUNDLE"
mkdir -p "$MACOS" "$RESOURCES"

# Copy binary
cp ".build/release/$APP_NAME" "$MACOS/$APP_NAME"
chmod +x "$MACOS/$APP_NAME"

# Copy icon and Info.plist
cp Resources/ArtFruit.icns "$RESOURCES/ArtFruit.icns"
cp Info.plist "$CONTENTS/Info.plist"
printf 'APPL????' > "$CONTENTS/PkgInfo"

echo ""
echo "✅  $APP_NAME.app built at:"
echo "    $SCRIPT_DIR/$APP_BUNDLE"
echo ""
echo "    Run with:  open \"$SCRIPT_DIR/$APP_BUNDLE\""
