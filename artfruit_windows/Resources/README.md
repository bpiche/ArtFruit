# App icon

The Windows build looks for `ArtFruit.ico` in this folder. When present it is
used both as the executable icon and as the system-tray glyph.

The macOS project ships `ArtFruit.icns` (in `../../ArtFruit/Resources/`). The
committed `ArtFruit.ico` is a multi-resolution icon
(16/24/32/48/64/128/256 px) generated from it. To regenerate it, use any of:

**Python / Pillow (cross-platform, no extra system tools — used to build the
committed icon)**
```bash
python3 - <<'PY'
import io, struct
from pathlib import Path
from PIL import Image

data = Path("../../ArtFruit/Resources/ArtFruit.icns").read_bytes()
total = struct.unpack_from(">I", data, 4)[0]
chunks, off = {}, 8
while off + 8 <= total:
    tag = data[off:off+4]; ln = struct.unpack_from(">I", data, off+4)[0]
    chunks[tag] = data[off+8:off+ln]; off += ln
for tag in (b'ic10', b'ic14', b'ic09', b'ic13', b'ic08', b'ic07'):
    c = chunks.get(tag)
    if c and c[:8] == b'\x89PNG\r\n\x1a\n':
        src = Image.open(io.BytesIO(c)).convert("RGBA"); break
sizes = [16, 24, 32, 48, 64, 128, 256]
frames = [src.resize((s, s), Image.LANCZOS) for s in sizes]
pngs = []
for f in frames:
    b = io.BytesIO(); f.save(b, "PNG"); pngs.append(b.getvalue())
with open("ArtFruit.ico", "wb") as out:
    out.write(struct.pack("<HHH", 0, 1, len(pngs)))
    offset = 6 + 16 * len(pngs)
    for s, p in zip(sizes, pngs):
        w = h = 0 if s == 256 else s
        out.write(struct.pack("<BBBBHHII", w, h, 0, 0, 1, 32, len(p), offset)); offset += len(p)
    for p in pngs: out.write(p)
print("wrote ArtFruit.ico")
PY
```

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
