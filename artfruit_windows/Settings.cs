using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArtFruit;

/// <summary>
/// User-configurable settings, persisted as JSON in
/// <c>%APPDATA%\ArtFruit\settings.json</c>. This is the Windows analogue of the
/// macOS app's <c>UserDefaults</c> usage.
/// </summary>
public sealed class Settings
{
    // Defaults chosen to match the Swift app's first-run behaviour.
    public double ChangeIntervalMinutes { get; set; } = 60;
    public bool ShowTitle { get; set; } = true;
    public bool ShowArtist { get; set; } = true;
    public bool MultiMonitor { get; set; } = false;

    /// <summary>Empty means "no filter — show everything".</summary>
    public HashSet<string> SelectedStyles { get; set; } = new();

    /// <summary>Empty means "use all sources".</summary>
    public HashSet<string> SelectedSources { get; set; } = new();

    // ------------------------------------------------------------------
    // Persistence
    // ------------------------------------------------------------------

    [JsonIgnore]
    private static string SettingsDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ArtFruit");

    [JsonIgnore]
    private static string SettingsPath =>
        Path.Combine(SettingsDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>Loads settings from disk, returning defaults if none exist or the file is corrupt.</summary>
    public static Settings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var loaded = JsonSerializer.Deserialize<Settings>(json, JsonOptions);
                if (loaded is not null)
                {
                    // Guard against null collections from older/partial files.
                    loaded.SelectedStyles ??= new();
                    loaded.SelectedSources ??= new();
                    return loaded;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Info($"Failed to load settings, using defaults: {ex.Message}");
        }

        return new Settings();
    }

    /// <summary>Writes the current settings to disk (best-effort).</summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            Log.Info($"Failed to save settings: {ex.Message}");
        }
    }
}
