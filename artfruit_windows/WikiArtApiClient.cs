using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArtFruit;

/// <summary>
/// Client for the WikiArt "paintings-by-style" JSON feed. Ports the Swift
/// <c>WikiArtAPIClient</c>, including the AIC-style-name → WikiArt-slug mapping.
/// </summary>
public sealed class WikiArtApiClient
{
    private const string BaseUrl = "https://www.wikiart.org/en/paintings-by-style";

    private readonly HttpClient _http;
    private readonly Random _random = Random.Shared;

    public WikiArtApiClient(HttpClient http)
    {
        _http = http;
    }

    /// <summary>Maps AIC style names to WikiArt URL slugs.</summary>
    public static readonly IReadOnlyDictionary<string, string> StyleSlugMap = new Dictionary<string, string>
    {
        ["Impressionism"] = "impressionism",
        ["Post-Impressionism"] = "post-impressionism",
        ["Surrealism"] = "surrealism",
        ["Abstract Expressionism"] = "abstract-expressionism",
        ["Modernism"] = "modernism",
        ["Baroque"] = "baroque",
        ["Renaissance"] = "high-renaissance",
        ["Romanticism"] = "romanticism",
        ["Realism"] = "realism",
        ["Art Nouveau"] = "art-nouveau-modern",
        ["Art Deco"] = "art-deco",
        ["Cubism"] = "cubism",
        ["Expressionism"] = "expressionism",
        ["Minimalism"] = "minimalism",
        ["Pop Art"] = "pop-art",
    };

    private static readonly string[] FallbackSlugs =
    {
        "impressionism",
        "post-impressionism",
        "surrealism",
        "baroque",
        "romanticism",
        "realism",
        "expressionism",
        "cubism",
        "abstract-expressionism",
        "art-nouveau-modern",
        "art-deco",
        "minimalism",
        "pop-art",
    };

    /// <summary>
    /// Fetch a random artwork, optionally filtered to styles from the shared
    /// style list. Unmapped styles are ignored; falls back to a random popular
    /// style if nothing maps.
    /// </summary>
    public async Task<Artwork> RandomArtworkAsync(ISet<string> styles, CancellationToken ct = default)
    {
        var mappedSlugs = styles
            .Where(s => StyleSlugMap.ContainsKey(s))
            .Select(s => StyleSlugMap[s])
            .ToList();

        var slug = mappedSlugs.Count > 0
            ? mappedSlugs[_random.Next(mappedSlugs.Count)]
            : FallbackSlugs[_random.Next(FallbackSlugs.Length)];

        Log.Info($"WikiArt using style slug: '{slug}'");
        return await RandomArtworkForSlugAsync(slug, ct).ConfigureAwait(false);
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private async Task<Artwork> RandomArtworkForSlugAsync(string slug, CancellationToken ct)
    {
        var firstPage = await FetchPageAsync(1, slug, ct).ConfigureAwait(false);
        if (firstPage.AllPaintingsCount <= 0)
        {
            Log.Info($"WikiArt: no results for '{slug}', falling back");
            var alternatives = FallbackSlugs.Where(s => s != slug).ToList();
            var fallbackSlug = alternatives.Count > 0
                ? alternatives[_random.Next(alternatives.Count)]
                : FallbackSlugs[0];
            return await RandomArtworkForSlugAsync(fallbackSlug, ct).ConfigureAwait(false);
        }

        var totalPages = Math.Min(firstPage.AllPaintingsCount / Math.Max(firstPage.PageSize, 1), 60);
        var randomPage = _random.Next(1, Math.Max(totalPages, 1) + 1);
        Log.Info($"WikiArt '{slug}': {firstPage.AllPaintingsCount} artworks, page {randomPage}/{totalPages}");

        var page = randomPage == 1 ? firstPage : await FetchPageAsync(randomPage, slug, ct).ConfigureAwait(false);
        var withImages = page.Paintings.Where(p => !string.IsNullOrEmpty(p.Image)).ToList();

        var pick = withImages.Count > 0 ? withImages[_random.Next(withImages.Count)] : null;
        if (pick is null || !Uri.TryCreate(pick.Image, UriKind.Absolute, out var imageUrl))
        {
            var fallback = firstPage.Paintings.Where(p => !string.IsNullOrEmpty(p.Image)).ToList();
            if (fallback.Count == 0)
                throw new NoArtworksFoundException();

            var pick2 = fallback[_random.Next(fallback.Count)];
            if (!Uri.TryCreate(pick2.Image, UriKind.Absolute, out var url2))
                throw new NoArtworksFoundException();
            return MakeArtwork(pick2, url2);
        }

        return MakeArtwork(pick, imageUrl);
    }

    private async Task<WikiArtPageResponse> FetchPageAsync(int page, string slug, CancellationToken ct)
    {
        var url = $"{BaseUrl}/{slug}?json=2&layout=new&page={page}&resultType=masonry";
        Log.Info($"WikiArt fetching: {url}");

        using var response = await _http.GetAsync(url, ct).ConfigureAwait(false);
        Log.Info($"WikiArt HTTP {(int)response.StatusCode}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<WikiArtPageResponse>(JsonOpts, ct).ConfigureAwait(false);
        return result ?? throw new NoArtworksFoundException();
    }

    private static Artwork MakeArtwork(WikiArtPainting painting, Uri imageUrl)
    {
        Log.Info($"WikiArt selected: \"{painting.Title ?? "nil"}\" by {painting.ArtistName ?? "nil"}");
        return new Artwork(
            Math.Abs((painting.Id ?? string.Empty).GetHashCode()),
            painting.Title ?? "Untitled",
            painting.ArtistName ?? "Unknown Artist",
            imageUrl);
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    // ------------------------------------------------------------------
    // Response DTOs
    // ------------------------------------------------------------------

    private sealed class WikiArtPageResponse
    {
        [JsonPropertyName("Paintings")]
        public List<WikiArtPainting> Paintings { get; set; } = new();

        [JsonPropertyName("AllPaintingsCount")]
        public int AllPaintingsCount { get; set; }

        [JsonPropertyName("PageSize")]
        public int PageSize { get; set; }
    }

    private sealed class WikiArtPainting
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("artistName")]
        public string? ArtistName { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }
    }
}
