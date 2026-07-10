using System.Drawing;
using System.Windows.Forms;

namespace ArtFruit;

/// <summary>
/// The tray-only application context — the Windows analogue of the macOS
/// <c>AppDelegate</c>. Owns the notification (tray) icon, its context menu, and
/// the view model, and wires menu commands to view-model actions.
/// </summary>
public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly ArtFruitViewModel _viewModel = new();
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;
    private ToolStripMenuItem _pauseItem = null!;
    private PreferencesForm? _preferencesForm;

    public TrayApplicationContext()
    {
        _menu = BuildMenu();

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadTrayIcon(),
            Text = "ArtFruit",
            Visible = true,
            ContextMenuStrip = _menu,
        };
        // Left-click also opens the menu for convenience.
        _notifyIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                _menu.Show(Cursor.Position);
        };

        // Bridge view-model events to the tray UI.
        _viewModel.NotificationRequested += ShowNotification;
        _viewModel.PauseStateChanged += OnPauseStateChanged;

        _viewModel.StartRotation();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();

        var title = new ToolStripMenuItem("ArtFruit") { Enabled = false };
        menu.Items.Add(title);
        menu.Items.Add(new ToolStripSeparator());

        var next = new ToolStripMenuItem("Next Artwork", null, (_, _) =>
        {
            Log.Info("nextArtwork() fired");
            _viewModel.FetchAndApplyArtwork();
        })
        { ShortcutKeyDisplayString = "N" };
        menu.Items.Add(next);

        _pauseItem = new ToolStripMenuItem("Pause", null, (_, _) =>
        {
            Log.Info("togglePause() fired");
            _viewModel.TogglePause();
        })
        { ShortcutKeyDisplayString = "P" };
        menu.Items.Add(_pauseItem);

        menu.Items.Add(new ToolStripSeparator());

        var save = new ToolStripMenuItem("Save Artwork…", null, (_, _) =>
        {
            Log.Info("saveArtwork() fired");
            _viewModel.SaveCurrentArtwork();
        })
        { ShortcutKeyDisplayString = "S" };
        menu.Items.Add(save);

        menu.Items.Add(new ToolStripSeparator());

        var prefs = new ToolStripMenuItem("Preferences…", null, (_, _) => OpenPreferences())
        { ShortcutKeyDisplayString = "," };
        menu.Items.Add(prefs);

        menu.Items.Add(new ToolStripSeparator());

        var quit = new ToolStripMenuItem("Quit ArtFruit", null, (_, _) => Quit())
        { ShortcutKeyDisplayString = "Q" };
        menu.Items.Add(quit);

        return menu;
    }

    private void OnPauseStateChanged(bool isPaused)
    {
        _pauseItem.Text = isPaused ? "Resume" : "Pause";
    }

    private void OpenPreferences()
    {
        if (_preferencesForm is null || _preferencesForm.IsDisposed)
        {
            _preferencesForm = new PreferencesForm(_viewModel);
            _preferencesForm.FormClosed += (_, _) => _preferencesForm = null;
            _preferencesForm.Show();
        }

        _preferencesForm.WindowState = FormWindowState.Normal;
        _preferencesForm.Activate();
        _preferencesForm.BringToFront();
    }

    private void ShowNotification(string title, string body)
    {
        // BalloonTip is the lightweight, dependency-free equivalent of the macOS
        // UNUserNotification banner.
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = body;
        _notifyIcon.ShowBalloonTip(5000);
    }

    private static Icon LoadTrayIcon()
    {
        // Prefer a bundled icon next to the exe; fall back to the app icon,
        // then finally to a stock system icon so the app always has a tray glyph.
        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "ArtFruit.ico");
            if (File.Exists(iconPath))
            {
                // Ask for the frame that best matches the current tray size so the
                // multi-resolution .ico renders crisply at any DPI (SmallIconSize
                // already reflects the system DPI scaling).
                var size = SystemInformation.SmallIconSize;
                return new Icon(iconPath, size);
            }

            var exePath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exePath))
            {
                var extracted = Icon.ExtractAssociatedIcon(exePath);
                if (extracted is not null)
                    return extracted;
            }
        }
        catch (Exception ex)
        {
            Log.Info($"Failed to load tray icon, using stock: {ex.Message}");
        }

        return SystemIcons.Application;
    }


    private void Quit()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _viewModel.Dispose();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notifyIcon.Dispose();
            _menu.Dispose();
            _viewModel.Dispose();
        }
        base.Dispose(disposing);
    }
}
