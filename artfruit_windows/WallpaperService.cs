using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Net.Http;

namespace ArtFruit;

/// <summary>
/// A single monitor to paint: its pixel bounds plus, when available, the shell
/// device id used to target it via <see cref="DesktopWallpaper"/>.
/// </summary>
public readonly record struct MonitorTarget(Rectangle PixelBounds, string? DeviceId);

/// <summary>
/// Downloads artwork and composites a per-monitor wallpaper (black fill →
/// dimmed/stretched background → scale-to-fit foreground → title/artist overlay),
/// then applies it. Ports the Swift <c>WallpaperService</c>.
/// </summary>
public sealed class WallpaperService
{
    private readonly HttpClient _http;
    private readonly string _cacheDir;
    private List<string> _lastWallpaperFiles = new();

    public WallpaperService(HttpClient http)
    {
        _http = http;
        _cacheDir = Path.Combine(Path.GetTempPath(), "io.github.bpiche.ArtFruit");
        Directory.CreateDirectory(_cacheDir);
        Log.Info($"Cache dir: {_cacheDir}");
    }

    /// <summary>
    /// Applies <paramref name="artwork"/> to the given targets (defaults to all
    /// monitors when <paramref name="targets"/> is null). The same artwork is
    /// fitted independently to each target's pixel resolution.
    /// </summary>
    public async Task ApplyAsync(
        Artwork artwork,
        bool showTitle,
        bool showArtist,
        IReadOnlyList<MonitorTarget>? targets = null,
        CancellationToken ct = default)
    {
        Log.Info($"Downloading image from {artwork.ImageUrl}...");
        var data = await _http.GetByteArrayAsync(artwork.ImageUrl, ct).ConfigureAwait(false);
        Log.Info($"Downloaded {data.Length} bytes.");

        using var ms = new MemoryStream(data);
        using var sourceImage = new Bitmap(ms);
        Log.Info($"Source image size: {sourceImage.Width}x{sourceImage.Height}");

        var monitorTargets = targets ?? Monitors.Enumerate();
        Log.Info($"Applying to {monitorTargets.Count} screen(s)...");

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var newFiles = new List<string>();

        for (var i = 0; i < monitorTargets.Count; i++)
        {
            var target = monitorTargets[i];
            var pixelWidth = Math.Max(target.PixelBounds.Width, 1);
            var pixelHeight = Math.Max(target.PixelBounds.Height, 1);
            Log.Info($"Screen {i}: {pixelWidth}x{pixelHeight}px");

            using var fitted = FitImage(sourceImage, pixelWidth, pixelHeight,
                artwork.Title, artwork.Artist, showTitle, showArtist);

            var file = Path.Combine(_cacheDir, $"artfruit_{timestamp}_screen{i}.jpg");
            SaveJpeg(fitted, file);
            newFiles.Add(file);

            ApplyToMonitor(target, file, i);
        }

        // Clean up previous wallpaper files.
        foreach (var old in _lastWallpaperFiles)
        {
            try { File.Delete(old); } catch { /* best effort */ }
        }
        _lastWallpaperFiles = newFiles;
        Log.Info("All screens updated.");
    }

    private static void ApplyToMonitor(MonitorTarget target, string file, int index)
    {
        try
        {
            if (target.DeviceId is { Length: > 0 } id)
            {
                DesktopWallpaper.SetWallpaper(id, file);
                Log.Info($"Screen {index}: applied wallpaper via IDesktopWallpaper");
            }
            else
            {
                LegacyWallpaper.SetForAllMonitors(file);
                Log.Info($"Screen {index}: applied wallpaper via SystemParametersInfo (all monitors)");
            }
        }
        catch (Exception ex)
        {
            Log.Info($"Screen {index}: FAILED: {ex.Message}");
            // Last-ditch fallback so the user still gets *a* wallpaper.
            try { LegacyWallpaper.SetForAllMonitors(file); } catch { /* give up */ }
        }
    }

    /// <summary>
    /// Scale-to-fit (letterbox) the artwork centered on a canvas, over a dimmed,
    /// scale-to-fill blurred background, with an optional title/artist overlay.
    /// </summary>
    private static Bitmap FitImage(
        Image source, int width, int height,
        string title, string artist, bool showTitle, bool showArtist)
    {
        var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(result);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        // --- Background: black fill, then a dimmed scale-to-fill copy of the art.
        g.Clear(Color.Black);

        double srcW = source.Width;
        double srcH = source.Height;

        var bgScale = Math.Max(width / srcW, height / srcH);
        var bgW = (float)(srcW * bgScale);
        var bgH = (float)(srcH * bgScale);
        var bgRect = new RectangleF(
            (float)((width - bgW) / 2),
            (float)((height - bgH) / 2),
            bgW, bgH);

        // Dim to 35% opacity (matches the Swift fraction: 0.35).
        using (var dim = new ImageAttributes())
        {
            var matrix = new ColorMatrix { Matrix33 = 0.35f };
            dim.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            g.DrawImage(
                source,
                new[]
                {
                    new PointF(bgRect.Left, bgRect.Top),
                    new PointF(bgRect.Right, bgRect.Top),
                    new PointF(bgRect.Left, bgRect.Bottom),
                },
                new RectangleF(0, 0, source.Width, source.Height),
                GraphicsUnit.Pixel,
                dim);
        }

        // --- Foreground: scale-to-fit (letterbox), centered.
        var fitScale = Math.Min(width / srcW, height / srcH);
        var fgW = (float)(srcW * fitScale);
        var fgH = (float)(srcH * fitScale);
        var fgRect = new RectangleF(
            (float)((width - fgW) / 2),
            (float)((height - fgH) / 2),
            fgW, fgH);
        g.DrawImage(source, fgRect);

        // --- Title/artist overlay (bottom-right of the artwork rect).
        var parts = new List<string>();
        if (showTitle) parts.Add(title);
        if (showArtist) parts.Add(artist);
        var text = string.Join("  ", parts);

        if (!string.IsNullOrWhiteSpace(text))
        {
            var fontSize = (float)Math.Max(24, Math.Min(44, width / 60.0));
            using var font = new Font("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            var measured = g.MeasureString(text, font);
            var padding = (float)Math.Max(16, width / 80.0);

            // GDI+ origin is top-left, so bottom padding is measured from fgRect.Bottom.
            var tx = fgRect.Right - measured.Width - padding;
            var ty = fgRect.Bottom - measured.Height - padding;

            using var shadow = new SolidBrush(Color.FromArgb(191, 0, 0, 0)); // ~0.75 alpha
            using var white = new SolidBrush(Color.White);
            g.DrawString(text, font, shadow, tx + 1, ty + 1);
            g.DrawString(text, font, white, tx, ty);
        }

        return result;
    }

    private static void SaveJpeg(Bitmap image, string path)
    {
        var encoder = ImageCodecInfo.GetImageEncoders()
            .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(Encoder.Quality, 92L);
        image.Save(path, encoder, parameters);
        Log.Info($"Saved {Path.GetFileName(path)}");
    }
}
