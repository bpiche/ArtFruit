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
    public const string WikiArt = "WikiArt";

    public static readonly IReadOnlyList<string> All = new[]
    {
        WikiArt,
    };
}

/// <summary>
/// The artist filter list surfaced in the Preferences "Artists" tab.
/// Matches the Swift <c>AICAvailableArtists</c> list — all have public-domain
/// works in the Art Institute of Chicago collection.
/// </summary>
public static class ArtArtists
{
    public static readonly IReadOnlyList<string> All = new[]
    {
        "Claude Monet",
        "Pierre-Auguste Renoir",
        "Paul Cézanne",
        "Edgar Degas",
        "Georges Seurat",
        "Vincent van Gogh",
        "Paul Gauguin",
        "Henri de Toulouse-Lautrec",
        "Édouard Manet",
        "Camille Pissarro",
        "Gustave Courbet",
        "Eugène Delacroix",
        "Jean-Auguste-Dominique Ingres",
        "Francisco José de Goya y Lucientes",
        "Rembrandt van Rijn",
        "Johannes Vermeer",
        "Peter Paul Rubens",
        "El Greco",
        "Caravaggio",
        "Sandro Botticelli",
        "Raphael",
        "Leonardo da Vinci",
        "Michelangelo Buonarroti",
        "Albrecht Dürer",
        "Hieronymus Bosch",
        "Jan van Eyck",
        "Pablo Picasso",
        "Henri Matisse",
        "Salvador Dalí",
        "René Magritte",
        "Wassily Kandinsky",
        "Paul Klee",
        "Piet Mondrian",
        "Amedeo Modigliani",
        "Marc Chagall",
        "Egon Schiele",
        "Gustav Klimt",
        "Edvard Munch",
        "James McNeill Whistler",
        "Winslow Homer",
        "John Singer Sargent",
        "Mary Cassatt",
        "Grant Wood",
        "Georgia O'Keeffe",
        "Edward Hopper",
        "Jackson Pollock",
        "Mark Rothko",
        "Andy Warhol",
        "Roy Lichtenstein",
        "Utagawa Hiroshige",
        "Katsushika Hokusai",
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
