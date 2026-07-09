# ArtFruit for Windows

A Windows system-tray app that rotates your desktop wallpaper through public
domain artwork from the [Art Institute of Chicago](https://www.artic.edu) open
API and [WikiArt](https://www.wikiart.org). This is a C# / .NET 8 + WinForms
port of the original macOS (Swift) ArtFruit menu-bar app.

## Features

- 🎨 Randomly selects from 130,000+ public domain artworks (Art Institute of Chicago) + millions from WikiArt
- 🧩 Filter by genre (Impressionism, Renaissance, Cubism, etc.)
- 🖥️ Per-monitor wallpapers sized to each display's exact pixel resolution (per-monitor-DPI aware)
- 🖼️ Scale-to-fit compositing with a dimmed background fill for letterboxed art
- ⏱️ Configurable rotation interval (15 min – 8 hours)
- 💾 Save any artwork to your Downloads folder from the tray menu — multi-monitor aware, saves the art from the screen your cursor is on
- ⏸️ Pause/resume from the tray menu
- 🔔 System tray notifications on each new artwork

The app lives in the system tray (notification area). There is no taskbar window.

## Requirements

- Windows 10 or 11 (x64)
- To build: the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
  installed **on the Windows host**
- To run a published build: nothing — it's self-contained (bundles the runtime)

## Build & Run

From a Windows PowerShell / `pwsh` prompt (or through WSL interop — see below):

```powershell
cd artfruit_windows
pwsh ./build.ps1 -Run
```

This publishes a self-contained, single-file `bin\publish\ArtFruit.exe` and
launches it. Look for the ArtFruit icon in the system tray.

Or with the SDK directly:

```powershell
dotnet publish ArtFruit.Windows.csproj -c Release -r win-x64 `
    --self-contained true -p:PublishSingleFile=true -o bin\publish
```

For quick iteration during development:

```powershell
dotnet run --project ArtFruit.Windows.csproj
```

### Building from WSL

Because WinForms targets `net8.0-windows`, the project must be built with the
**Windows** .NET SDK, not the Linux SDK inside WSL. Thanks to WSL interop you
can invoke it without leaving your shell:

```bash
# From the artfruit_windows directory inside WSL:
powershell.exe -ExecutionPolicy Bypass -File build.ps1
# or call the Windows SDK directly:
dotnet.exe publish ArtFruit.Windows.csproj -c Release -r win-x64 \
    --self-contained true -p:PublishSingleFile=true -o bin/publish
```

> Note: building across the `\\wsl$` boundary works but is slower than building
> on the native Windows filesystem. For faster iteration, copy the project to a
> path under `C:\` (e.g. `/mnt/c/...`).

## Usage

Right-click (or left-click) the tray icon for the menu:

- **Next Artwork** — fetch and apply a new wallpaper immediately
- **Pause / Resume** — stop/start automatic rotation
- **Save Artwork…** — save the artwork on the monitor under your cursor to Downloads
- **Preferences…** — interval, multi-monitor, title/artist overlay, source & style filters
- **Quit ArtFruit** — exit

Settings persist to `%APPDATA%\ArtFruit\settings.json`.

## How it maps to the macOS app

| macOS (Swift / AppKit) | Windows (C# / .NET) |
| --- | --- |
| `NSStatusItem` menu bar item | `NotifyIcon` + `ContextMenuStrip` |
| `NSWorkspace.setDesktopImageURL(_:for:)` | `IDesktopWallpaper` COM interface (per-monitor) |
| `SystemParametersInfo` fallback | `SystemParametersInfo(SPI_SETDESKWALLPAPER)` |
| `NSImage` / `NSBitmapImageRep` compositing | `System.Drawing` (GDI+) |
| `NSScreen.frame * backingScaleFactor` | `Screen.Bounds` under per-monitor-v2 DPI awareness |
| `URLSession` | `HttpClient` |
| `JSONDecoder` | `System.Text.Json` |
| `UserDefaults` | `%APPDATA%\ArtFruit\settings.json` |
| `UNUserNotificationCenter` | `NotifyIcon.ShowBalloonTip` |
| `~/Downloads` | `SHGetKnownFolderPath(FOLDERID_Downloads)` |

## Data Sources

- **[Art Institute of Chicago API](https://api.artic.edu/docs/)** — all works are public domain. No API key required.
- **[WikiArt](https://www.wikiart.org)** — extensive collection spanning many movements and genres.

## License

MIT
