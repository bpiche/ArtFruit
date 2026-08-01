#!/bin/bash
set -e

cd /Users/bpiche/Projects/ArtFruit/ArtFruit

echo "=> Building..."
bash build.sh

echo "=> Zipping .app..."
cd .build
rm -f ArtFruit-1.0.0.zip
zip -r --symlinks ArtFruit-1.0.0.zip ArtFruit.app
echo "Created: $(pwd)/ArtFruit-1.0.0.zip"

cd /Users/bpiche/Projects/ArtFruit

echo "=> Committing version bump..."
git add ArtFruit/Info.plist
git commit -m "Release 1.0.0"

echo "=> Tagging v1.0.0..."
git tag -a v1.0.0 -m "Version 1.0.0"

echo "=> Pushing tag..."
git push origin main --tags

echo "=> Creating GitHub release..."
gh release create v1.0.0 \
  ArtFruit/ArtFruit/.build/ArtFruit-1.0.0.zip \
  --title "ArtFruit 1.0.0" \
  --notes "Initial release of ArtFruit — a macOS menu bar app that sets your wallpaper to artwork from WikiArt and AI-generated captions via AICP." \
  --latest

echo "Done!"
