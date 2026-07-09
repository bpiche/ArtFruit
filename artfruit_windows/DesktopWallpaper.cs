using System.Drawing;
using System.Runtime.InteropServices;

namespace ArtFruit;

/// <summary>A monitor as reported by <see cref="DesktopWallpaper"/>: its shell device id and pixel rect.</summary>
public readonly record struct WallpaperMonitor(string Id, Rectangle Bounds);


/// <summary>
/// Minimal wrapper around the Windows <c>IDesktopWallpaper</c> COM interface
/// (Windows 8+). This is what lets ArtFruit set a *different* wallpaper on each
/// connected monitor — the Windows analogue of macOS's
/// <c>NSWorkspace.setDesktopImageURL(_:for:options:)</c>.
///
/// If the COM interface is unavailable, callers should fall back to
/// <see cref="LegacyWallpaper.SetForAllMonitors"/>.
/// </summary>
public static class DesktopWallpaper
{
    /// <summary>Returns the per-monitor device paths, ordered as the shell reports them.</summary>
    public static IReadOnlyList<string> GetMonitorDevicePaths()
    {
        var wallpaper = CreateInstance();
        try
        {
            var count = wallpaper.GetMonitorDevicePathCount();
            var paths = new List<string>((int)count);
            for (uint i = 0; i < count; i++)
            {
                var id = wallpaper.GetMonitorDevicePathAt(i);
                if (!string.IsNullOrEmpty(id))
                    paths.Add(id);
            }
            return paths;
        }
        finally
        {
            Marshal.FinalReleaseComObject(wallpaper);
        }
    }

    /// <summary>
    /// Returns every monitor's shell device id together with its pixel rectangle,
    /// so callers can match a monitor to a <see cref="System.Windows.Forms.Screen"/>.
    /// </summary>
    public static IReadOnlyList<WallpaperMonitor> GetMonitors()
    {
        var wallpaper = CreateInstance();
        try
        {
            var count = wallpaper.GetMonitorDevicePathCount();
            var monitors = new List<WallpaperMonitor>((int)count);
            for (uint i = 0; i < count; i++)
            {
                var id = wallpaper.GetMonitorDevicePathAt(i);
                if (string.IsNullOrEmpty(id))
                    continue;

                Rectangle bounds = Rectangle.Empty;
                try
                {
                    wallpaper.GetMonitorRECT(id, out var r);
                    bounds = Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
                }
                catch
                {
                    // Detached/mirrored monitors can fail here; leave bounds empty.
                }

                monitors.Add(new WallpaperMonitor(id, bounds));
            }
            return monitors;
        }
        finally
        {
            Marshal.FinalReleaseComObject(wallpaper);
        }
    }

    /// <summary>Sets the wallpaper for a specific monitor device path.</summary>
    public static void SetWallpaper(string monitorId, string imagePath)
    {

        var wallpaper = CreateInstance();
        try
        {
            wallpaper.SetWallpaper(monitorId, imagePath);
        }
        finally
        {
            Marshal.FinalReleaseComObject(wallpaper);
        }
    }

    /// <summary>Sets the same wallpaper on every monitor (monitorId = null applies to all).</summary>
    public static void SetWallpaperAllMonitors(string imagePath)
    {
        var wallpaper = CreateInstance();
        try
        {
            wallpaper.SetWallpaper(null, imagePath);
        }
        finally
        {
            Marshal.FinalReleaseComObject(wallpaper);
        }
    }

    private static IDesktopWallpaper CreateInstance()
    {
        var type = Type.GetTypeFromCLSID(new Guid(CLSID_DesktopWallpaper))
            ?? throw new InvalidOperationException("IDesktopWallpaper CLSID not found.");
        return (IDesktopWallpaper)(Activator.CreateInstance(type)
            ?? throw new InvalidOperationException("Failed to create IDesktopWallpaper instance."));
    }

    private const string CLSID_DesktopWallpaper = "C2CF3110-460E-4fc1-B9D0-8A1C0C9CC4BD";

    [ComImport]
    [Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDesktopWallpaper
    {
        void SetWallpaper(
            [MarshalAs(UnmanagedType.LPWStr)] string? monitorID,
            [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID);

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetMonitorDevicePathAt(uint monitorIndex);

        [return: MarshalAs(UnmanagedType.U4)]
        uint GetMonitorDevicePathCount();

        // Remaining vtable entries are declared to preserve layout but unused.
        void GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitorID, out RECT displayRect);

        void SetBackgroundColor(uint color);
        uint GetBackgroundColor();
        void SetPosition(int position);
        int GetPosition();
        void SetSlideshow(IntPtr items);
        IntPtr GetSlideshow();
        void SetSlideshowOptions(int options, uint slideshowTick);
        void GetSlideshowOptions(out int options, out uint slideshowTick);
        void AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)] string monitorID, int direction);
        int GetStatus();
        void Enable([MarshalAs(UnmanagedType.Bool)] bool enable);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}

/// <summary>
/// Legacy wallpaper setter via <c>SystemParametersInfo</c>. Applies a single
/// image across the whole virtual desktop. Used as a fallback when
/// <see cref="DesktopWallpaper"/> (the COM interface) is unavailable.
/// </summary>
public static class LegacyWallpaper
{
    private const int SPI_SETDESKWALLPAPER = 0x0014;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDCHANGE = 0x02;

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    public static void SetForAllMonitors(string imagePath)
    {
        var ok = SystemParametersInfo(
            SPI_SETDESKWALLPAPER,
            0,
            imagePath,
            SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        if (ok == 0)
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }
}
