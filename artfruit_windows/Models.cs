namespace ArtFruit;

/// <summary>
/// A single artwork with the metadata ArtFruit needs to display and save it.
/// Mirrors the Swift <c>AICArtwork</c> struct.
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
    public const string ArtInstituteOfChicago = "The Art Institute of Chicago";
    public const string WikiArt = "WikiArt";

    public static readonly IReadOnlyList<string> All = new[]
    {
        ArtInstituteOfChicago,
        WikiArt,
    };
}

/// <summary>
/// The style filter list surfaced in the Preferences "Style" tab.
/// Sourced from AIC aggregations on public-domain artworks (matches Swift's
/// <c>AICAvailableStyles</c>).
/// </summary>
public static class ArtStyles
{
    public static readonly IReadOnlyList<string> All = new[]
    {
        "Impressionism",
        "Post-Impressionism",
        "Surrealism",
        "Abstract Expressionism",
        "Modernism",
        "Baroque",
        "Renaissance",
        "Romanticism",
        "Realism",
        "Art Nouveau",
        "Art Deco",
        "Cubism",
        "Expressionism",
        "Minimalism",
        "Pop Art",
        "contemporary",
        "medieval",
        "19th century",
        "18th Century",
        "17th Century",
        "greek",
        "egyptian",
        "roman (ancient, style or period)",
        "Japanese (culture or style)",
        "Chinese (culture or style)",
        "South Asian",
        "Himalayan",
        "Pictorialism",
        "nazca",
        "moche",
    };
}
