using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArtFruit;

/// <summary>
/// Client for the Art Institute of Chicago public API. Ports the Swift
/// <c>AICAPIClient</c>: an unfiltered paginated path plus a style-filtered path
/// backed by the Elasticsearch <c>/artworks/search</c> endpoint.
/// </summary>
public sealed class AicApiClient
{
    private const string BaseUrl = "https://api.artic.edu/api/v1";
    private const string IiifBase = "https://www.artic.edu/iiif/2";

    private readonly HttpClient _http;
    private readonly Random _random = Random.Shared;

    public AicApiClient(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Fetch a random public-domain artwork, optionally filtered by styles and/or
    /// artists. When both sets are empty, falls back to the unfiltered paginated listing.
    /// </summary>
    public async Task<Artwork> RandomArtworkAsync(ISet<string> styles, ISet<string> artists, CancellationToken ct = default)
    {
        if (styles.Count == 0 && artists.Count == 0)
            return await RandomArtworkUnfilteredAsync(ct).ConfigureAwait(false);

        return await RandomArtworkFilteredAsync(styles, artists, ct).ConfigureAwait(false);
    }

    // ------------------------------------------------------------------
    // Unfiltered path
    // ------------------------------------------------------------------

    private async Task<Artwork> RandomArtworkUnfilteredAsync(CancellationToken ct)
    {
        var firstPage = await FetchArtworkPageAsync(1, ct).ConfigureAwait(false);
        var totalPages = Math.Min(firstPage.Pagination.TotalPages, 12000);
        Log.Info($"Total pages: {totalPages}");

        var randomPage = _random.Next(1, Math.Max(totalPages, 1) + 1);
        Log.Info($"Fetching page {randomPage}");

        var page = randomPage == 1 ? firstPage : await FetchArtworkPageAsync(randomPage, ct).ConfigureAwait(false);
        var withImages = page.Data.Where(d => !string.IsNullOrEmpty(d.ImageId)).ToList();
        Log.Info($"Artworks with images on this page: {withImages.Count}");

        var pick = withImages.Count > 0 ? withImages[_random.Next(withImages.Count)] : null;
        if (pick is null)
        {
            var fallback = firstPage.Data.Where(d => !string.IsNullOrEmpty(d.ImageId)).ToList();
            if (fallback.Count == 0)
                throw new NoArtworksFoundException();
            var pick2 = fallback[_random.Next(fallback.Count)];
            return MakeArtwork(pick2);
        }

        return MakeArtwork(pick);
    }

    private async Task<AicListResponse> FetchArtworkPageAsync(int page, CancellationToken ct)
    {
        var url = $"{BaseUrl}/artworks?page={page}&limit=100&fields=id,title,artist_title,image_id&is_public_domain=1";
        Log.Info($"Fetching: {url}");

        using var response = await _http.GetAsync(url, ct).ConfigureAwait(false);
        Log.Info($"HTTP {(int)response.StatusCode}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AicListResponse>(JsonOpts, ct).ConfigureAwait(false);
        return result ?? throw new NoArtworksFoundException();
    }

    // ------------------------------------------------------------------
    // Style/Artist-filtered path (Elasticsearch search endpoint)
    // ------------------------------------------------------------------

    private async Task<Artwork> RandomArtworkFilteredAsync(ISet<string> styles, ISet<string> artists, CancellationToken ct)
    {
        // Build must-clauses combining style and artist filters.
        var mustClauses = new List<object>
        {
            new { term = new Dictionary<string, object> { ["is_public_domain"] = true } },
            new { exists = new { field = "image_id" } },
        };

        if (styles.Count > 0)
        {
            var styleList = styles.ToList();
            var chosenStyle = styleList[_random.Next(styleList.Count)];
            mustClauses.Add(new { term = new Dictionary<string, object> { ["style_title.keyword"] = chosenStyle } });
            Log.Info($"AIC filtering by style: '{chosenStyle}'");
        }

        if (artists.Count > 0)
        {
            var artistList = artists.ToList();
            var chosenArtist = artistList[_random.Next(artistList.Count)];
            mustClauses.Add(new { match = new Dictionary<string, object> { ["artist_title"] = chosenArtist } });
            Log.Info($"AIC filtering by artist: '{chosenArtist}'");
        }

        var firstPage = await FetchSearchPageAsync(1, mustClauses, ct).ConfigureAwait(false);
        if (firstPage.Pagination.Total <= 0)
        {
            Log.Info("No results for the given filters, falling back to unfiltered.");
            return await RandomArtworkUnfilteredAsync(ct).ConfigureAwait(false);
        }

        // Cap at 100 pages (10,000 results — Elasticsearch limit).
        var totalPages = Math.Min(firstPage.Pagination.TotalPages, 100);
        Log.Info($"AIC filtered: {firstPage.Pagination.Total} artworks, {totalPages} pages");

        var randomPage = _random.Next(1, Math.Max(totalPages, 1) + 1);
        var page = randomPage == 1 ? firstPage : await FetchSearchPageAsync(randomPage, mustClauses, ct).ConfigureAwait(false);

        var withImages = page.Data.Where(d => !string.IsNullOrEmpty(d.ImageId)).ToList();
        Log.Info($"Artworks with images on page {randomPage}: {withImages.Count}");

        var pick = withImages.Count > 0 ? withImages[_random.Next(withImages.Count)] : null;
        if (pick is null)
        {
            var fallback = firstPage.Data.Where(d => !string.IsNullOrEmpty(d.ImageId)).ToList();
            if (fallback.Count == 0)
                throw new NoArtworksFoundException();
            var pick2 = fallback[_random.Next(fallback.Count)];
            return MakeArtwork(pick2);
        }

        return MakeArtwork(pick);
    }

    private async Task<AicListResponse> FetchSearchPageAsync(int page, List<object> mustClauses, CancellationToken ct)
    {
        var query = new
        {
            query = new
            {
                @bool = new { must = mustClauses },
            },
            fields = new[] { "id", "title", "artist_title", "image_id" },
            _source = new[] { "id", "title", "artist_title", "image_id" },
            from = (page - 1) * 100,
            size = 100,
        };

        var url = $"{BaseUrl}/artworks/search";
        var json = JsonSerializer.Serialize(query);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        Log.Info($"AIC search POST page {page}");
        using var response = await _http.PostAsync(url, content, ct).ConfigureAwait(false);
        Log.Info($"HTTP {(int)response.StatusCode}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AicListResponse>(JsonOpts, ct).ConfigureAwait(false);
        return result ?? throw new NoArtworksFoundException();
    }

    // ------------------------------------------------------------------
    // Shared helpers
    // ------------------------------------------------------------------

    private static Artwork MakeArtwork(AicArtworkData data)
    {
        var imageUrl = new Uri($"{IiifBase}/{data.ImageId}/full/1400,/0/default.jpg");
        Log.Info($"Selected: \"{data.Title ?? "nil"}\" by {data.ArtistTitle ?? "nil"}");
        return new Artwork(
            data.Id,
            data.Title ?? "Untitled",
            data.ArtistTitle ?? "Unknown Artist",
            imageUrl);
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    // ------------------------------------------------------------------
    // Response DTOs (both the list and search endpoints share this shape)
    // ------------------------------------------------------------------

    private sealed class AicListResponse
    {
        [JsonPropertyName("pagination")]
        public AicPagination Pagination { get; set; } = new();

        [JsonPropertyName("data")]
        public List<AicArtworkData> Data { get; set; } = new();
    }

    private sealed class AicPagination
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }
    }

    private sealed class AicArtworkData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("artist_title")]
        public string? ArtistTitle { get; set; }

        [JsonPropertyName("image_id")]
        public string? ImageId { get; set; }
    }
}
