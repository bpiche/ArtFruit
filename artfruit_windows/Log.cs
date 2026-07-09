using System.Diagnostics;

namespace ArtFruit;

/// <summary>
/// Minimal logging helper — the Windows analogue of the macOS app's <c>NSLog</c>
/// calls. Writes to the debugger/trace output and, in debug builds, the console.
/// </summary>
public static class Log
{
    public static void Info(string message)
    {
        var line = $"[ArtFruit] {message}";
        Debug.WriteLine(line);
        Trace.WriteLine(line);
    }
}
