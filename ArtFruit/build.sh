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

# ---------------------------------------------------------------------------
# Code signing
#
# This MUST happen after the bundle is fully assembled. `swift build` emits a
# "linker-signed" ad-hoc binary, but that signature covers only the Mach-O
# executable — it has no _CodeSignature/CodeResources. Once we add Info.plist
# and Resources/, macOS evaluates the *bundle* signature, finds no resource
# seal, and rejects it with:
#
#     "code has no resources but signature indicates they must be present"
#
# A *broken* signature is worse than none: Gatekeeper shows the dead-end
# "ArtFruit.app is damaged and can't be opened" dialog, which offers the user
# no way to proceed. Re-signing the assembled bundle produces a valid seal, so
# a quarantined download instead gets the recoverable "Not Opened" dialog with
# an "Open Anyway" button in System Settings ▸ Privacy & Security.
#
# Set CODESIGN_IDENTITY to a "Developer ID Application: ..." identity to
# produce a distributable (notarizable) build. Defaults to ad-hoc ("-").
# ---------------------------------------------------------------------------
BUNDLE_ID="io.github.bpiche.ArtFruit"
CODESIGN_IDENTITY="${CODESIGN_IDENTITY:--}"

if [ "$CODESIGN_IDENTITY" = "-" ]; then
  echo "=> Signing (ad-hoc)..."
  codesign --force --sign - --identifier "$BUNDLE_ID" "$APP_BUNDLE"
else
  echo "=> Signing with identity: $CODESIGN_IDENTITY"
  codesign --force --sign "$CODESIGN_IDENTITY" \
    --identifier "$BUNDLE_ID" \
    --options runtime \
    --timestamp \
    "$APP_BUNDLE"
fi

echo "=> Verifying signature..."
codesign --verify --strict --verbose=2 "$APP_BUNDLE"

echo ""
echo "✅  $APP_NAME.app built at:"

echo "    $SCRIPT_DIR/$APP_BUNDLE"
echo ""
echo "    Run with:  open \"$SCRIPT_DIR/$APP_BUNDLE\""
