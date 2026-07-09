# App icon

The Windows build looks for `ArtFruit.ico` in this folder. When present it is
used both as the executable icon and as the system-tray glyph.

The macOS project ships `ArtFruit.icns` (in `../../ArtFruit/Resources/`). To
produce a matching Windows icon, convert it to a multi-resolution `.ico`
(16/24/32/48/256 px). A couple of easy options:

**ImageMagick**
```bash
# From a 1024x1024 PNG exported out of the .icns:
magick convert artfruit-1024.png -define icon:auto-resize=256,128,64,48,32,24,16 ArtFruit.ico
```

**On macOS (extract PNG from .icns first)**
```bash
iconutil -c iconset ArtFruit.icns   # or: sips to export a PNG
```

If `ArtFruit.ico` is absent the app still runs fine — it falls back to the
executable's associated icon and then to a stock system icon, so the tray icon
is always populated.
