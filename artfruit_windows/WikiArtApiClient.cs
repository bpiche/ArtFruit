using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArtFruit;

// MARK: - Style + Artist group data

/// A single style entry: display name + WikiArt slug.
public sealed record WikiArtStyle(string Name, string Slug);

/// A group of related styles (shown as a collapsible section in Preferences).
public sealed class WikiArtStyleGroup
{
    public string Name { get; }
    public IReadOnlyList<WikiArtStyle> Styles { get; }
    public WikiArtStyleGroup(string name, IReadOnlyList<WikiArtStyle> styles)
    {
        Name = name; Styles = styles;
    }
}

/// A single artist entry: display name + WikiArt slug.
public sealed record WikiArtArtist(string Name, string Slug);

/// A group of related artists (shown as a collapsible section in Preferences).
public sealed class WikiArtArtistGroup
{
    public string Name { get; }
    public IReadOnlyList<WikiArtArtist> Artists { get; }
    public WikiArtArtistGroup(string name, IReadOnlyList<WikiArtArtist> artists)
    {
        Name = name; Artists = artists;
    }
}

// MARK: - Data tables

public static class WikiArtData
{
    public static readonly IReadOnlyList<WikiArtStyleGroup> StyleGroups = new[]
    {
        new WikiArtStyleGroup("Modern & Contemporary", new WikiArtStyle[]
        {
            new("Impressionism",           "impressionism"),
            new("Post-Impressionism",      "post-impressionism"),
            new("Expressionism",           "expressionism"),
            new("Fauvism",                 "fauvism"),
            new("Cubism",                  "cubism"),
            new("Futurism",                "futurism"),
            new("Dadaism",                 "dadaism"),
            new("Surrealism",              "surrealism"),
            new("Abstract Expressionism",  "abstract-expressionism"),
            new("Abstract Art",            "abstract-art"),
            new("Pop Art",                 "pop-art"),
            new("Minimalism",              "minimalism"),
            new("Op Art",                  "op-art"),
            new("Conceptual Art",          "conceptual-art"),
            new("Neo-Expressionism",       "neo-expressionism"),
            new("Color Field Painting",    "color-field-painting"),
            new("Lyrical Abstraction",     "lyrical-abstraction"),
            new("Street Art",              "street-art"),
            new("Outsider Art",            "outsider-art"),
            new("Naive Art / Primitivism", "naive-art-primitivism"),
        }),
        new WikiArtStyleGroup("Modern Movements", new WikiArtStyle[]
        {
            new("Art Nouveau",             "art-nouveau-modern"),
            new("Art Deco",                "art-deco"),
            new("Symbolism",               "symbolism"),
            new("Modernism",               "modernism"),
            new("Constructivism",          "constructivism"),
            new("Suprematism",             "suprematism"),
            new("De Stijl",                "de-stijl"),
            new("Bauhaus",                 "bauhaus"),
            new("Neue Sachlichkeit",       "new-objectivity"),
            new("Pittura Metafisica",      "pittura-metafisica"),
            new("Les Nabis",               "les-nabis"),
            new("Synthetism",              "synthetism"),
            new("Cloisonnism",             "cloisonnism"),
            new("Pointillism",             "pointillism"),
            new("Divisionism",             "divisionism"),
            new("Japonisme",               "japonisme"),
            new("Tachisme",                "tachisme"),
            new("Art Informel",            "art-informel"),
            new("COBRA",                   "cobra"),
            new("Magic Realism",           "magic-realism"),
        }),
        new WikiArtStyleGroup("Realism & Naturalism", new WikiArtStyle[]
        {
            new("Realism",                 "realism"),
            new("Social Realism",          "social-realism"),
            new("American Realism",        "american-realism"),
            new("Socialist Realism",       "socialist-realism"),
            new("Photorealism",            "photorealism"),
            new("Hyperrealism",            "hyperrealism"),
            new("Hudson River School",     "hudson-river-school"),
            new("Ashcan School",           "ashcan-school"),
            new("Tonalism",                "tonalism"),
            new("Luminism",                "luminism"),
            new("Orientalism",             "orientalism"),
            new("Academic Art",            "academic-art"),
        }),
        new WikiArtStyleGroup("Renaissance & Baroque", new WikiArtStyle[]
        {
            new("Early Renaissance",       "early-renaissance"),
            new("High Renaissance",        "high-renaissance"),
            new("Northern Renaissance",    "northern-renaissance"),
            new("Mannerism",               "mannerism-late-renaissance"),
            new("Baroque",                 "baroque"),
            new("Rococo",                  "rococo"),
        }),
        new WikiArtStyleGroup("Neoclassicism & Romanticism", new WikiArtStyle[]
        {
            new("Neoclassicism",           "neoclassicism"),
            new("Romanticism",             "romanticism"),
            new("Pre-Raphaelite Brotherhood", "pre-raphaelite-brotherhood"),
            new("Biedermeier",             "biedermeier"),
            new("Neo-Impressionism",       "neo-impressionism"),
        }),
        new WikiArtStyleGroup("Medieval & Early", new WikiArtStyle[]
        {
            new("Gothic",                  "gothic"),
            new("Byzantine",               "byzantine"),
            new("Medieval",                "medieval"),
            new("Early Christian",         "early-christian"),
        }),
        new WikiArtStyleGroup("Ancient", new WikiArtStyle[]
        {
            new("Ancient Egyptian",        "ancient-egyptian-art"),
            new("Ancient Greek",           "ancient-greek-art"),
            new("Ancient Roman",           "ancient-roman-art"),
        }),
        new WikiArtStyleGroup("Asian & World", new WikiArtStyle[]
        {
            new("Ukiyo-e",                 "ukiyo-e"),
            new("Japanese Art",            "japanese-art"),
            new("Chinese Art",             "chinese-art"),
            new("Indian Art",              "indian-art"),
            new("Islamic Art",             "islamic-art"),
            new("Folk Art",                "folk-art"),
            new("Tribal Art",              "tribal-art"),
        }),
    };

    public static readonly IReadOnlyList<WikiArtArtistGroup> ArtistGroups = new[]
    {
        new WikiArtArtistGroup("Impressionism", new WikiArtArtist[]
        {
            new("Claude Monet",                       "claude-monet"),
            new("Pierre-Auguste Renoir",              "pierre-auguste-renoir"),
            new("Edgar Degas",                        "edgar-degas"),
            new("Camille Pissarro",                   "camille-pissarro"),
            new("Alfred Sisley",                      "alfred-sisley"),
            new("Berthe Morisot",                     "berthe-morisot"),
            new("Mary Cassatt",                       "mary-cassatt"),
            new("Gustave Caillebotte",                "gustave-caillebotte"),
        }),
        new WikiArtArtistGroup("Post-Impressionism", new WikiArtArtist[]
        {
            new("Paul Cézanne",                       "paul-cezanne"),
            new("Vincent van Gogh",                   "vincent-van-gogh"),
            new("Paul Gauguin",                       "paul-gauguin"),
            new("Georges Seurat",                     "georges-seurat"),
            new("Henri de Toulouse-Lautrec",          "henri-de-toulouse-lautrec"),
            new("Paul Signac",                        "paul-signac"),
            new("Odilon Redon",                       "odilon-redon"),
        }),
        new WikiArtArtistGroup("Modern Masters", new WikiArtArtist[]
        {
            new("Pablo Picasso",                      "pablo-picasso"),
            new("Henri Matisse",                      "henri-matisse"),
            new("Wassily Kandinsky",                  "wassily-kandinsky"),
            new("Paul Klee",                          "paul-klee"),
            new("Piet Mondrian",                      "piet-mondrian"),
            new("Amedeo Modigliani",                  "amedeo-modigliani"),
            new("Marc Chagall",                       "marc-chagall"),
            new("Fernand Léger",                      "fernand-leger"),
            new("Georges Braque",                     "georges-braque"),
            new("Juan Gris",                          "juan-gris"),
            new("Robert Delaunay",                    "robert-delaunay"),
        }),
        new WikiArtArtistGroup("Expressionism & Symbolism", new WikiArtArtist[]
        {
            new("Edvard Munch",                       "edvard-munch"),
            new("Egon Schiele",                       "egon-schiele"),
            new("Gustav Klimt",                       "gustav-klimt"),
            new("Oskar Kokoschka",                    "oskar-kokoschka"),
            new("Ernst Ludwig Kirchner",              "ernst-ludwig-kirchner"),
            new("Emil Nolde",                         "emil-nolde"),
            new("Franz Marc",                         "franz-marc"),
            new("Gustave Moreau",                     "gustave-moreau"),
        }),
        new WikiArtArtistGroup("Surrealism & Dada", new WikiArtArtist[]
        {
            new("Salvador Dalí",                      "salvador-dali"),
            new("René Magritte",                      "rene-magritte"),
            new("Max Ernst",                          "max-ernst"),
            new("Joan Miró",                          "joan-miro"),
            new("Giorgio de Chirico",                 "giorgio-de-chirico"),
            new("Frida Kahlo",                        "frida-kahlo"),
            new("Man Ray",                            "man-ray"),
            new("Marcel Duchamp",                     "marcel-duchamp"),
            new("Yves Tanguy",                        "yves-tanguy"),
            new("Paul Delvaux",                       "paul-delvaux"),
        }),
        new WikiArtArtistGroup("Abstract & Post-War", new WikiArtArtist[]
        {
            new("Jackson Pollock",                    "jackson-pollock"),
            new("Mark Rothko",                        "mark-rothko"),
            new("Willem de Kooning",                  "willem-de-kooning"),
            new("Franz Kline",                        "franz-kline"),
            new("Lee Krasner",                        "lee-krasner"),
            new("Clyfford Still",                     "clyfford-still"),
            new("Barnett Newman",                     "barnett-newman"),
            new("Arshile Gorky",                      "arshile-gorky"),
            new("Helen Frankenthaler",                "helen-frankenthaler"),
        }),
        new WikiArtArtistGroup("Pop Art & Contemporary", new WikiArtArtist[]
        {
            new("Andy Warhol",                        "andy-warhol"),
            new("Roy Lichtenstein",                   "roy-lichtenstein"),
            new("Jasper Johns",                       "jasper-johns"),
            new("Robert Rauschenberg",                "robert-rauschenberg"),
            new("David Hockney",                      "david-hockney"),
            new("Keith Haring",                       "keith-haring"),
            new("Jean-Michel Basquiat",               "jean-michel-basquiat"),
            new("Banksy",                             "banksy"),
        }),
        new WikiArtArtistGroup("American Art", new WikiArtArtist[]
        {
            new("Winslow Homer",                      "winslow-homer"),
            new("John Singer Sargent",                "john-singer-sargent"),
            new("James McNeill Whistler",             "james-mcneill-whistler"),
            new("Georgia O'Keeffe",                   "georgia-o-keeffe"),
            new("Edward Hopper",                      "edward-hopper"),
            new("Grant Wood",                         "grant-wood"),
            new("Thomas Eakins",                      "thomas-eakins"),
            new("Mary Cassatt",                       "mary-cassatt"),
            new("Frederic Remington",                 "frederic-remington"),
            new("Albert Bierstadt",                   "albert-bierstadt"),
            new("Thomas Cole",                        "thomas-cole"),
        }),
        new WikiArtArtistGroup("Old Masters – Italian", new WikiArtArtist[]
        {
            new("Leonardo da Vinci",                  "leonardo-da-vinci"),
            new("Michelangelo Buonarroti",            "michelangelo"),
            new("Raphael",                            "raphael"),
            new("Sandro Botticelli",                  "sandro-botticelli"),
            new("Caravaggio",                         "caravaggio"),
            new("Titian",                             "titian"),
            new("Giovanni Bellini",                   "giovanni-bellini"),
            new("Giotto di Bondone",                  "giotto-di-bondone"),
            new("Fra Angelico",                       "fra-angelico"),
            new("Piero della Francesca",              "piero-della-francesca"),
            new("Paolo Veronese",                     "paolo-veronese"),
            new("Tintoretto",                         "tintoretto"),
            new("Canaletto",                          "canaletto"),
            new("Giovanni Battista Tiepolo",          "giovanni-battista-tiepolo"),
        }),
        new WikiArtArtistGroup("Old Masters – Northern European", new WikiArtArtist[]
        {
            new("Rembrandt van Rijn",                 "rembrandt"),
            new("Johannes Vermeer",                   "johannes-vermeer"),
            new("Peter Paul Rubens",                  "peter-paul-rubens"),
            new("Jan van Eyck",                       "jan-van-eyck"),
            new("Hieronymus Bosch",                   "hieronymus-bosch"),
            new("Albrecht Dürer",                     "albrecht-durer"),
            new("Hans Holbein the Younger",           "hans-holbein-the-younger"),
            new("Pieter Bruegel the Elder",           "pieter-bruegel-the-elder"),
            new("Anthony van Dyck",                   "anthony-van-dyck"),
            new("Frans Hals",                         "frans-hals"),
            new("Jan Steen",                          "jan-steen"),
            new("Caspar David Friedrich",             "caspar-david-friedrich"),
        }),
        new WikiArtArtistGroup("Old Masters – Spanish & Other", new WikiArtArtist[]
        {
            new("Francisco José de Goya y Lucientes", "francisco-goya"),
            new("Diego Velázquez",                    "diego-velazquez"),
            new("El Greco",                           "el-greco"),
            new("Bartolomé Esteban Murillo",          "bartolome-esteban-murillo"),
            new("Francisco de Zurbarán",              "francisco-de-zurbaran"),
        }),
        new WikiArtArtistGroup("19th Century", new WikiArtArtist[]
        {
            new("Édouard Manet",                      "edouard-manet"),
            new("Gustave Courbet",                    "gustave-courbet"),
            new("Eugène Delacroix",                   "eugene-delacroix"),
            new("Jean-Auguste-Dominique Ingres",      "jean-auguste-dominique-ingres"),
            new("Jacques-Louis David",                "jacques-louis-david"),
            new("William Bouguereau",                 "william-bouguereau"),
            new("Jean-Baptiste-Camille Corot",        "jean-baptiste-camille-corot"),
            new("Honoré Daumier",                     "honore-daumier"),
            new("William Turner",                     "william-turner"),
            new("John Constable",                     "john-constable"),
            new("Dante Gabriel Rossetti",             "dante-gabriel-rossetti"),
            new("John Everett Millais",               "john-everett-millais"),
            new("Gustave Moreau",                     "gustave-moreau"),
            new("Arnold Böcklin",                     "arnold-bocklin"),
        }),
        new WikiArtArtistGroup("Japanese", new WikiArtArtist[]
        {
            new("Katsushika Hokusai",                 "katsushika-hokusai"),
            new("Utagawa Hiroshige",                  "utagawa-hiroshige"),
            new("Kitagawa Utamaro",                   "kitagawa-utamaro"),
            new("Utagawa Kuniyoshi",                  "utagawa-kuniyoshi"),
            new("Utagawa Kunisada",                   "utagawa-kunisada"),
            new("Kawase Hasui",                       "kawase-hasui"),
        }),
    };

    /// Flat style name → slug map (for API lookups).
    public static readonly IReadOnlyDictionary<string, string> StyleSlugMap = BuildStyleSlugMap();
    /// Flat artist name → slug map (for API lookups).
    public static readonly IReadOnlyDictionary<string, string> ArtistSlugMap = BuildArtistSlugMap();

    private static Dictionary<string, string> BuildStyleSlugMap()
    {
        var d = new Dictionary<string, string>();
        foreach (var g in StyleGroups)
            foreach (var s in g.Styles)
                d[s.Name] = s.Slug;
        return d;
    }

    private static Dictionary<string, string> BuildArtistSlugMap()
    {
        var d = new Dictionary<string, string>();
        foreach (var g in ArtistGroups)
            foreach (var a in g.Artists)
                d[a.Name] = a.Slug;
        return d;
    }
}

// MARK: - API client

public sealed class WikiArtApiClient
{
    private readonly HttpClient _http;
    private const string StyleBase  = "https://www.wikiart.org/en/paintings-by-style";
    private const string ArtistBase = "https://www.wikiart.org/en";

    // Expose slug maps for badge labels in PreferencesForm (kept for compat).
    public static IReadOnlyDictionary<string, string> StyleSlugMap  => WikiArtData.StyleSlugMap;
    public static IReadOnlyDictionary<string, string> ArtistSlugMap => WikiArtData.ArtistSlugMap;

    private static readonly string[] FallbackSlugs =
    {
        "impressionism", "post-impressionism", "surrealism", "baroque",
        "romanticism", "realism", "expressionism", "cubism",
        "abstract-expressionism", "art-nouveau-modern", "art-deco",
        "minimalism", "pop-art", "high-renaissance", "symbolism", "fauvism",
    };

    public WikiArtApiClient(HttpClient http) => _http = http;

    public async Task<Artwork> RandomArtworkAsync(
        IReadOnlySet<string> styles,
        IReadOnlySet<string> artists,
        CancellationToken ct = default)
    {
        // Artist filter takes priority
        var artistSlugs = artists
            .Where(a => WikiArtData.ArtistSlugMap.ContainsKey(a))
            .Select(a => WikiArtData.ArtistSlugMap[a])
            .ToList();

        if (artistSlugs.Count > 0)
        {
            var slug = artistSlugs[Random.Shared.Next(artistSlugs.Count)];
            Log.Info($"WikiArt: filtering by artist slug '{slug}'");
            return await RandomArtworkForArtistAsync(slug, ct).ConfigureAwait(false);
        }

        // Style filter
        var styleSlugs = styles
            .Where(s => WikiArtData.StyleSlugMap.ContainsKey(s))
            .Select(s => WikiArtData.StyleSlugMap[s])
            .ToList();

        var styleSlug = styleSlugs.Count > 0
            ? styleSlugs[Random.Shared.Next(styleSlugs.Count)]
            : FallbackSlugs[Random.Shared.Next(FallbackSlugs.Length)];

        Log.Info($"WikiArt: using style slug '{styleSlug}'");
        return await RandomArtworkForSlugAsync(styleSlug, ct).ConfigureAwait(false);
    }

    // ----------------------------------------------------------------
    // Artist path
    // ----------------------------------------------------------------

    private async Task<Artwork> RandomArtworkForArtistAsync(string artistSlug, CancellationToken ct)
    {
        var first = await FetchArtistPageAsync(artistSlug, 1, ct).ConfigureAwait(false);
        if (first.AllPaintingsCount == 0)
        {
            Log.Info($"WikiArt: no artworks for artist '{artistSlug}', falling back");
            return await RandomArtworkForSlugAsync(FallbackSlugs[Random.Shared.Next(FallbackSlugs.Length)], ct)
                .ConfigureAwait(false);
        }

        var totalPages = Math.Min(first.AllPaintingsCount / Math.Max(first.PageSize, 1), 60);
        var page = Random.Shared.Next(1, Math.Max(totalPages, 1) + 1);
        Log.Info($"WikiArt artist '{artistSlug}': {first.AllPaintingsCount} artworks, page {page}/{totalPages}");

        var result = page == 1 ? first : await FetchArtistPageAsync(artistSlug, page, ct).ConfigureAwait(false);
        return PickFromPaintings(result.Paintings) ?? PickFromPaintings(first.Paintings)
            ?? throw new NoArtworksFoundException();
    }

    private async Task<WikiArtResponse> FetchArtistPageAsync(string slug, int page, CancellationToken ct)
    {
        var url = $"{ArtistBase}/{slug}/all-works/text-list?json=2&layout=new&page={page}&resultType=masonry";
        Log.Info($"WikiArt fetching artist: {url}");
        return await _http.GetFromJsonAsync<WikiArtResponse>(url, ct).ConfigureAwait(false)
            ?? throw new NoArtworksFoundException();
    }

    // ----------------------------------------------------------------
    // Style path
    // ----------------------------------------------------------------

    private async Task<Artwork> RandomArtworkForSlugAsync(string slug, CancellationToken ct)
    {
        var first = await FetchStylePageAsync(slug, 1, ct).ConfigureAwait(false);
        if (first.AllPaintingsCount == 0)
        {
            var fallback = FallbackSlugs.Where(s => s != slug).OrderBy(_ => Random.Shared.Next()).First();
            return await RandomArtworkForSlugAsync(fallback, ct).ConfigureAwait(false);
        }

        var totalPages = Math.Min(first.AllPaintingsCount / Math.Max(first.PageSize, 1), 60);
        var page = Random.Shared.Next(1, Math.Max(totalPages, 1) + 1);
        Log.Info($"WikiArt style '{slug}': {first.AllPaintingsCount} artworks, page {page}/{totalPages}");

        var result = page == 1 ? first : await FetchStylePageAsync(slug, page, ct).ConfigureAwait(false);
        return PickFromPaintings(result.Paintings) ?? PickFromPaintings(first.Paintings)
            ?? throw new NoArtworksFoundException();
    }

    private async Task<WikiArtResponse> FetchStylePageAsync(string slug, int page, CancellationToken ct)
    {
        var url = $"{StyleBase}/{slug}?json=2&layout=new&page={page}&resultType=masonry";
        Log.Info($"WikiArt fetching: {url}");
        return await _http.GetFromJsonAsync<WikiArtResponse>(url, ct).ConfigureAwait(false)
            ?? throw new NoArtworksFoundException();
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static Artwork? PickFromPaintings(IReadOnlyList<WikiArtPainting> paintings)
    {
        var withImages = paintings.Where(p => !string.IsNullOrEmpty(p.Image)).ToList();
        if (withImages.Count == 0) return null;
        var pick = withImages[Random.Shared.Next(withImages.Count)];
        Log.Info($"WikiArt selected: \"{pick.Title}\" by {pick.ArtistName}");
        return new Artwork(
            Id: Math.Abs(pick.Id?.GetHashCode() ?? 0),
            Title: pick.Title ?? "Untitled",
            Artist: pick.ArtistName ?? "Unknown Artist",
            ImageUrl: new Uri(pick.Image!)
        );
    }

    // ----------------------------------------------------------------
    // JSON response types
    // ----------------------------------------------------------------

    private sealed class WikiArtResponse
    {
        [JsonPropertyName("Paintings")]       public List<WikiArtPainting> Paintings       { get; set; } = new();
        [JsonPropertyName("AllPaintingsCount")] public int AllPaintingsCount { get; set; }
        [JsonPropertyName("PageSize")]        public int PageSize             { get; set; }
    }

    private sealed class WikiArtPainting
    {
        [JsonPropertyName("id")]         public string? Id         { get; set; }
        [JsonPropertyName("title")]      public string? Title      { get; set; }
        [JsonPropertyName("artistName")] public string? ArtistName { get; set; }
        [JsonPropertyName("image")]      public string? Image      { get; set; }
    }
}
