# ArtFruit

A macOS menu bar app that rotates your desktop wallpaper through public domain artwork from the [Art Institute of Chicago](https://www.artic.edu) open API and [WikiArt](https://www.wikiart.org).

## Features

- 🎨 Randomly selects from millions of public domain artworks on WikiArt
- 🧩 Filter by style — full WikiArt style taxonomy, grouped and collapsible in Preferences
- 🖌️ Filter by artist — complete A–Z WikiArt artist index (3,500+ artists), grouped and collapsible in Preferences
- 🖥️ Per-screen wallpapers sized to each display's exact pixel resolution (retina-aware)
- 🖼️ Scale-to-fit compositing with a blurred/dimmed background fill for letterboxed art
- ⏱️ Configurable rotation interval (15 min – 8 hours)
- 💾 Save any artwork to ~/Downloads from the menu bar (Cmd+S) — multi-monitor aware, saves the art from the screen your cursor is on
- ⏸️ Pause/resume from the menu bar
- 🔔 System notifications on each new artwork

## Download

Grab the latest release from the [Releases page](../../releases/latest):

- **ArtFruit-vX.X.zip** — unzip and drag `ArtFruit.app` to Applications
- **ArtFruit-vX.X.dmg** — open and drag `ArtFruit.app` to the Applications shortcut

### ⚠️ First launch

ArtFruit isn't notarized by Apple (that requires a paid Apple Developer account), so
macOS blocks it the first time you open it. **This is expected, and it only happens
once.**

1. Drag `ArtFruit.app` to **Applications**, then double-click it.
2. macOS shows a message saying the app can't be opened. Click **Done**.
3. Open **System Settings** ▸ **Privacy & Security**.
4. Scroll down to the **Security** section. You'll see
   *"ArtFruit.app was blocked to protect your Mac."* Click **Open Anyway**.
5. Confirm with **Open Anyway** and authenticate with Touch ID or your password.

ArtFruit lives in the **menu bar** (paint palette icon, top-right of your screen), not
the Dock — it has no window of its own.

> **Note:** On macOS Sequoia (15) and later, the old "right-click ▸ Open" shortcut no
> longer works — you must use System Settings as described above.

<details>
<summary>Prefer the Terminal? (optional, one command)</summary>

```bash
xattr -dr com.apple.quarantine /Applications/ArtFruit.app
```
</details>

## Requirements

- macOS 14 (Sonoma) or later
- Apple Silicon or Intel Mac
- Xcode Command Line Tools (`xcode-select --install`)

## Build & Run

```bash
cd ArtFruit
./build.sh
open .build/ArtFruit.app
```

The app lives in the menu bar (paint palette icon). No Dock icon.

## Data Sources

- **[WikiArt](https://www.wikiart.org)** — extensive collection of public domain and openly licensed visual art spanning every major movement, style, and artist.

## License

MIT