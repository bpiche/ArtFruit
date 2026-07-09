using System.Drawing;
using System.Windows.Forms;

namespace ArtFruit;

/// <summary>
/// Enumerates the connected monitors as <see cref="MonitorTarget"/>s. Because the
/// process is per-monitor-DPI aware (see the app manifest), <c>Screen.Bounds</c>
/// reports real pixel dimensions — the Windows analogue of macOS's
/// <c>NSScreen.frame * backingScaleFactor</c>.
///
/// Each <see cref="Screen"/> is matched to its <see cref="DesktopWallpaper"/>
/// device id by bounds so per-monitor wallpapers land on the correct display.
/// </summary>
public static class Monitors
{
    public static IReadOnlyList<MonitorTarget> Enumerate()
    {
        var screens = Screen.AllScreens;

        // Try to pair each screen with a COM monitor device id by matching bounds.
        IReadOnlyList<WallpaperMonitor> comMonitors;
        try
        {
            comMonitors = DesktopWallpaper.GetMonitors();
        }
        catch (Exception ex)
        {
            Log.Info($"IDesktopWallpaper unavailable, using legacy fallback: {ex.Message}");
            comMonitors = Array.Empty<WallpaperMonitor>();
        }

        var targets = new List<MonitorTarget>(screens.Length);
        foreach (var screen in screens)
        {
            var deviceId = MatchDeviceId(screen.Bounds, comMonitors);
            targets.Add(new MonitorTarget(screen.Bounds, deviceId));
        }

        return targets;
    }

    /// <summary>Returns just the primary monitor as a target.</summary>
    public static MonitorTarget Primary()
    {
        var all = Enumerate();
        var primaryScreen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
        foreach (var t in all)
        {
            if (t.PixelBounds == primaryScreen.Bounds)
                return t;
        }
        return all.Count > 0 ? all[0] : new MonitorTarget(primaryScreen.Bounds, null);
    }

    private static string? MatchDeviceId(Rectangle screenBounds, IReadOnlyList<WallpaperMonitor> comMonitors)
    {
        // Exact origin match first (most reliable).
        foreach (var m in comMonitors)
        {
            if (m.Bounds.IsEmpty) continue;
            if (m.Bounds.Location == screenBounds.Location)
                return m.Id;
        }

        // Fall back to the closest top-left corner within a small tolerance.
        string? best = null;
        var bestDist = int.MaxValue;
        foreach (var m in comMonitors)
        {
            if (m.Bounds.IsEmpty) continue;
            var dx = m.Bounds.X - screenBounds.X;
            var dy = m.Bounds.Y - screenBounds.Y;
            var dist = Math.Abs(dx) + Math.Abs(dy);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = m.Id;
            }
        }

        // If we couldn't reliably match, returning null lets the caller fall back
        // to the legacy whole-desktop setter.
        return bestDist <= 8 ? best : null;
    }
}
