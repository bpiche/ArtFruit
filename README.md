# ArtFruit

A macOS menu bar app that rotates your desktop wallpaper through public domain artwork from the [Art Institute of Chicago](https://www.artic.edu) open API.

## Features

- 🎨 Randomly selects from 130,000+ public domain artworks
- 🖥️ Per-screen wallpapers sized to each display's exact pixel resolution (retina-aware)
- 🖼️ Scale-to-fit compositing with a blurred/dimmed background fill for letterboxed art
- ⏱️ Configurable rotation interval (15 min – 8 hours)
- ⏸️ Pause/resume from the menu bar
- 🔔 System notifications on each new artwork

## Download

Grab the latest release from the [Releases page](../../releases/latest):

- **ArtFruit-vX.X.zip** — unzip and drag `ArtFruit.app` to Applications
- **ArtFruit-vX.X.dmg** — open and drag `ArtFruit.app` to the Applications shortcut

> **First launch:** macOS will block the app because it isn't notarized. Right-click → **Open** → **Open**, or run:
> ```bash
> xattr -dr com.apple.quarantine /Applications/ArtFruit.app
> ```

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

## Project Structure

```
ArtFruit/
├── Sources/
│   ├── ArtFruitApp.swift        # Entry point
│   ├── AppDelegate.swift        # Menu bar setup, NSStatusItem
│   ├── ArtFruitViewModel.swift  # State, timer, fetch orchestration
│   ├── AICAPIClient.swift       # Art Institute of Chicago REST API client
│   ├── WallpaperService.swift   # Image compositing + NSWorkspace wallpaper
│   └── PreferencesView.swift    # SwiftUI preferences panel
├── Info.plist
├── Package.swift
└── build.sh
```

## Data Source

Artwork is fetched from the [Art Institute of Chicago API](https://api.artic.edu/docs/) — all works are public domain. No API key required.

## License

MIT
