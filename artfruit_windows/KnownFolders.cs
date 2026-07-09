using System.Runtime.InteropServices;

namespace ArtFruit;

/// <summary>
/// Resolves Windows known folders. Uses <c>SHGetKnownFolderPath</c> for the
/// Downloads folder (which has no <see cref="Environment.SpecialFolder"/> entry),
/// falling back to <c>%USERPROFILE%\Downloads</c>.
/// </summary>
public static class KnownFolders
{
    // FOLDERID_Downloads
    private static readonly Guid DownloadsGuid = new("374DE290-123F-4565-9164-39C4925E467B");

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
    private static extern string SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
        uint dwFlags,
        IntPtr hToken);

    public static string Downloads
    {
        get
        {
            try
            {
                var path = SHGetKnownFolderPath(DownloadsGuid, 0, IntPtr.Zero);
                if (!string.IsNullOrEmpty(path))
                    return path;
            }
            catch (Exception ex)
            {
                Log.Info($"SHGetKnownFolderPath(Downloads) failed: {ex.Message}");
            }

            // Fallback: %USERPROFILE%\Downloads
            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(profile, "Downloads");
        }
    }
}
