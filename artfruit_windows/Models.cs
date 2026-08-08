namespace ArtFruit;

/// <summary>
/// A single artwork with the metadata ArtFruit needs to display and save it.
/// </summary>
public sealed record Artwork(int Id, string Title, string Artist, Uri ImageUrl);

/// <summary>Thrown when no usable artwork could be located from a source.</summary>
public sealed class NoArtworksFoundException : Exception
{
    public NoArtworksFoundException()
        : base("No artworks could be found.") { }
}

/// <summary>Names of the art sources the user can enable in Preferences.</summary>
public static class ArtSources
{
    public const string WikiArt = "WikiArt";

    public static readonly IReadOnlyList<string> All = new[]
    {
        WikiArt,
    };
}
