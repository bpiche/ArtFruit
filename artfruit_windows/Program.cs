using System.Windows.Forms;

namespace ArtFruit;

/// <summary>
/// Entry point. Ports the Swift <c>ArtFruitApp</c> — instead of an
/// <c>NSApplication</c> with an accessory (Dock-less) activation policy, we run a
/// WinForms message loop hosting a tray-only <see cref="TrayApplicationContext"/>
/// with no main window (the Windows equivalent of "menu bar only, no Dock icon").
/// </summary>
internal static class Program
{
    // Ensures only one ArtFruit instance runs at a time.
    private static Mutex? _singleInstanceMutex;

    [STAThread]
    private static void Main()
    {
        const string mutexName = "Local\\io.github.bpiche.ArtFruit";
        try
        {
            _singleInstanceMutex = new Mutex(initiallyOwned: true, mutexName, out var createdNew);
            if (!createdNew)
            {
                // Another instance already owns the tray icon — exit quietly.
                return;
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Mutex exists but we don't have access; assume another instance is running.
            return;
        }

        // Per-monitor-v2 DPI awareness so Screen.Bounds reports true pixels and
        // wallpapers are composited at native resolution on high-DPI displays.
        // (The app.manifest also declares this; setting it here is a belt-and-braces.)
        try { Application.SetHighDpiMode(HighDpiMode.PerMonitorV2); } catch { /* older runtime */ }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using var context = new TrayApplicationContext();
        Application.Run(context);

        GC.KeepAlive(_singleInstanceMutex);
    }
}
