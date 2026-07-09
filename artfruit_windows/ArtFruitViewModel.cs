using System.Net.Http;
using System.Windows.Forms;

namespace ArtFruit;

/// <summary>
/// Orchestrates artwork rotation, applying wallpapers, saving, and settings —
/// the Windows port of the Swift <c>ArtFruitViewModel</c>. Timer callbacks and
/// public members are expected to be used from the UI thread.
/// </summary>
public sealed class ArtFruitViewModel : IDisposable
{
    private readonly HttpClient _http;
    private readonly AicApiClient _aic;
    private readonly WikiArtApiClient _wikiArt;
    private readonly WallpaperService _wallpaper;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly Random _random = Random.Shared;

    private Settings _settings;

    /// <summary>Artwork applied to each screen (multi-monitor mode), same order as <see cref="Monitors.Enumerate"/>.</summary>
    private List<Artwork> _screenArtworks = new();

    // Raised whenever the "current" artwork changes so the UI can refresh.
    public event Action? CurrentArtworkChanged;

    // Raised to request a user-facing notification (title, body).
    public event Action<string, string>? NotificationRequested;

    // Raised when pause state changes so the tray menu can update its label.
    public event Action<bool>? PauseStateChanged;

    public ArtFruitViewModel()
    {
        _settings = Settings.Load();

        _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60),
        };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("ArtFruit/1.0 (+https://github.com/bpiche/ArtFruit)");

        _aic = new AicApiClient(_http);
        _wikiArt = new WikiArtApiClient(_http);
        _wallpaper = new WallpaperService(_http);

        _timer = new System.Windows.Forms.Timer();
        _timer.Tick += (_, _) => FetchAndApplyArtwork();
    }

    // ------------------------------------------------------------------
    // Public state (settings-backed)
    // ------------------------------------------------------------------

    public string? CurrentTitle { get; private set; }
    public string? CurrentArtist { get; private set; }
    public Uri? CurrentImageUrl { get; private set; }

    public bool IsPaused { get; private set; }

    public double ChangeIntervalMinutes
    {
        get => _settings.ChangeIntervalMinutes;
        set
        {
            if (Math.Abs(_settings.ChangeIntervalMinutes - value) < 0.001) return;
            _settings.ChangeIntervalMinutes = value;
            _settings.Save();
            RescheduleTimer();
        }
    }

    public bool ShowTitle
    {
        get => _settings.ShowTitle;
        set { _settings.ShowTitle = value; _settings.Save(); }
    }

    public bool ShowArtist
    {
        get => _settings.ShowArtist;
        set { _settings.ShowArtist = value; _settings.Save(); }
    }

    public bool MultiMonitor
    {
        get => _settings.MultiMonitor;
        set { _settings.MultiMonitor = value; _settings.Save(); }
    }

    public HashSet<string> SelectedStyles
    {
        get => _settings.SelectedStyles;
        set { _settings.SelectedStyles = value; _settings.Save(); }
    }

    public HashSet<string> SelectedSources
    {
        get => _settings.SelectedSources;
        set { _settings.SelectedSources = value; _settings.Save(); }
    }

    public void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;
        IsPaused = paused;
        PauseStateChanged?.Invoke(IsPaused);
    }

    public void TogglePause() => SetPaused(!IsPaused);

    // ------------------------------------------------------------------
    // Rotation
    // ------------------------------------------------------------------

    public void StartRotation()
    {
        FetchAndApplyArtwork();
        ScheduleTimer();
    }

    public async void FetchAndApplyArtwork()
    {
        if (IsPaused) return;

        try
        {
            var targets = Monitors.Enumerate();

            if (MultiMonitor && targets.Count > 1)
            {
                Log.Info($"Multi-monitor mode: fetching {targets.Count} artworks...");

                // Fetch a unique artwork for each screen in parallel, preserving order.
                var fetchTasks = targets.Select(_ => FetchOneArtworkAsync()).ToArray();
                var artworks = await Task.WhenAll(fetchTasks).ConfigureAwait(true);
                _screenArtworks = artworks.ToList();

                // Apply each artwork to its corresponding screen.
                for (var i = 0; i < targets.Count; i++)
                {
                    var artwork = artworks[i % artworks.Length];
                    await _wallpaper.ApplyAsync(
                        artwork, ShowTitle, ShowArtist,
                        new[] { targets[i] }).ConfigureAwait(true);
                }

                var primary = artworks[0];
                SetCurrent(primary);
                NotificationRequested?.Invoke("New Artwork", $"{primary.Title} — {primary.Artist}");
            }
            else
            {
                var artwork = await FetchOneArtworkAsync().ConfigureAwait(true);
                _screenArtworks = new List<Artwork> { artwork };
                SetCurrent(artwork);
                Log.Info($"Got artwork: \"{artwork.Title}\" by {artwork.Artist} — {artwork.ImageUrl}");

                await _wallpaper.ApplyAsync(artwork, ShowTitle, ShowArtist, targets).ConfigureAwait(true);
                Log.Info("Wallpaper applied successfully.");
                NotificationRequested?.Invoke("New Artwork", $"{artwork.Title} — {artwork.Artist}");
            }
        }
        catch (Exception ex)
        {
            Log.Info($"ERROR: {ex.Message}");
            NotificationRequested?.Invoke("ArtFruit Error", ex.Message);
        }
    }

    // ------------------------------------------------------------------
    // Save current artwork
    // ------------------------------------------------------------------

    /// <summary>Downloads the artwork on the monitor under the cursor to the Downloads folder.</summary>
    public async void SaveCurrentArtwork()
    {
        try
        {
            var artwork = ArtworkUnderCursor();
            if (artwork is null)
            {
                NotificationRequested?.Invoke("Download Failed", "No artwork to save.");
                return;
            }

            var safeName = MakeSafeFileName($"{artwork.Title} - {artwork.Artist}");
            var downloads = KnownFolders.Downloads;
            var filePath = Path.Combine(downloads, $"{safeName}.jpg");

            var data = await _http.GetByteArrayAsync(artwork.ImageUrl).ConfigureAwait(true);
            await File.WriteAllBytesAsync(filePath, data).ConfigureAwait(true);
            Log.Info($"Saved artwork to {filePath}");

            // Reveal in Explorer, selecting the saved file.
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{filePath}\"",
                    UseShellExecute = true,
                });
            }
            catch { /* non-fatal */ }

            NotificationRequested?.Invoke("Artwork Saved", $"{artwork.Title} — {artwork.Artist}");
        }
        catch (Exception ex)
        {
            Log.Info($"Failed to download artwork: {ex.Message}");
            NotificationRequested?.Invoke("Download Failed", ex.Message);
        }
    }

    private Artwork? ArtworkUnderCursor()
    {
        if (_screenArtworks.Count == 0)
            return null;

        var cursor = Cursor.Position;
        var screens = Screen.AllScreens;
        var index = 0;
        for (var i = 0; i < screens.Length; i++)
        {
            if (screens[i].Bounds.Contains(cursor))
            {
                index = i;
                break;
            }
        }

        return _screenArtworks[index % _screenArtworks.Count];
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private async Task<Artwork> FetchOneArtworkAsync()
    {
        string source;
        if (SelectedSources.Count == 0)
        {
            source = ArtSources.All[_random.Next(ArtSources.All.Count)];
        }
        else
        {
            var list = SelectedSources.ToList();
            source = list[_random.Next(list.Count)];
        }

        Log.Info($"Fetching from source: {source}");
        return source == ArtSources.WikiArt
            ? await _wikiArt.RandomArtworkAsync(SelectedStyles).ConfigureAwait(true)
            : await _aic.RandomArtworkAsync(SelectedStyles).ConfigureAwait(true);
    }

    private void SetCurrent(Artwork artwork)
    {
        CurrentTitle = artwork.Title;
        CurrentArtist = artwork.Artist;
        CurrentImageUrl = artwork.ImageUrl;
        CurrentArtworkChanged?.Invoke();
    }

    private void ScheduleTimer()
    {
        var intervalMs = (int)Math.Clamp(ChangeIntervalMinutes * 60_000, 1000, int.MaxValue);
        _timer.Interval = intervalMs;
        _timer.Start();
    }

    private void RescheduleTimer()
    {
        if (!_timer.Enabled) return;
        _timer.Stop();
        ScheduleTimer();
    }

    private static string MakeSafeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '-');
        return name;
    }

    public void Dispose()
    {
        _timer.Dispose();
        _http.Dispose();
    }
}
