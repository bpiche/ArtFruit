import Foundation

// MARK: - Artist slug mapping (display name → WikiArt URL slug)

/// Maps the artist display names (matching AICAvailableArtists) to their WikiArt URL slugs.
/// Only artists whose works appear on WikiArt are included here.
let WikiArtArtistSlugMap: [String: String] = [
    "Claude Monet":                         "claude-monet",
    "Pierre-Auguste Renoir":                "pierre-auguste-renoir",
    "Paul Cézanne":                         "paul-cezanne",
    "Edgar Degas":                          "edgar-degas",
    "Georges Seurat":                       "georges-seurat",
    "Vincent van Gogh":                     "vincent-van-gogh",
    "Paul Gauguin":                         "paul-gauguin",
    "Henri de Toulouse-Lautrec":            "henri-de-toulouse-lautrec",
    "Édouard Manet":                        "edouard-manet",
    "Camille Pissarro":                     "camille-pissarro",
    "Gustave Courbet":                      "gustave-courbet",
    "Eugène Delacroix":                     "eugene-delacroix",
    "Jean-Auguste-Dominique Ingres":        "jean-auguste-dominique-ingres",
    "Francisco José de Goya y Lucientes":   "francisco-goya",
    "Rembrandt van Rijn":                   "rembrandt",
    "Johannes Vermeer":                     "johannes-vermeer",
    "Peter Paul Rubens":                    "peter-paul-rubens",
    "El Greco":                             "el-greco",
    "Caravaggio":                           "caravaggio",
    "Sandro Botticelli":                    "sandro-botticelli",
    "Raphael":                              "raphael",
    "Leonardo da Vinci":                    "leonardo-da-vinci",
    "Michelangelo Buonarroti":              "michelangelo",
    "Albrecht Dürer":                       "albrecht-durer",
    "Hieronymus Bosch":                     "hieronymus-bosch",
    "Jan van Eyck":                         "jan-van-eyck",
    "Pablo Picasso":                        "pablo-picasso",
    "Henri Matisse":                        "henri-matisse",
    "Salvador Dalí":                        "salvador-dali",
    "René Magritte":                        "rene-magritte",
    "Wassily Kandinsky":                    "wassily-kandinsky",
    "Paul Klee":                            "paul-klee",
    "Piet Mondrian":                        "piet-mondrian",
    "Amedeo Modigliani":                    "amedeo-modigliani",
    "Marc Chagall":                         "marc-chagall",
    "Egon Schiele":                         "egon-schiele",
    "Gustav Klimt":                         "gustav-klimt",
    "Edvard Munch":                         "edvard-munch",
    "James McNeill Whistler":               "james-mcneill-whistler",
    "Winslow Homer":                        "winslow-homer",
    "John Singer Sargent":                  "john-singer-sargent",
    "Mary Cassatt":                         "mary-cassatt",
    "Utagawa Hiroshige":                    "utagawa-hiroshige",
    "Katsushika Hokusai":                   "katsushika-hokusai",
]

// MARK: - Style slug mapping (AIC style name → WikiArt URL slug)

let WikiArtStyleSlugMap: [String: String] = [
    "Impressionism":           "impressionism",
    "Post-Impressionism":      "post-impressionism",
    "Surrealism":              "surrealism",
    "Abstract Expressionism":  "abstract-expressionism",
    "Modernism":               "modernism",
    "Baroque":                 "baroque",
    "Renaissance":             "high-renaissance",
    "Romanticism":             "romanticism",
    "Realism":                 "realism",
    "Art Nouveau":             "art-nouveau-modern",
    "Art Deco":                "art-deco",
    "Cubism":                  "cubism",
    "Expressionism":           "expressionism",
    "Minimalism":              "minimalism",
    "Pop Art":                 "pop-art",
]

private let wikiArtFallbackArtistSlugs: [String] = [
    "claude-monet",
    "vincent-van-gogh",
    "pablo-picasso",
    "rembrandt",
    "pierre-auguste-renoir",
]

private let wikiArtFallbackSlugs: [String] = [
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
]

// MARK: - Private response types

private struct WikiArtPageResponse: Decodable {
    struct Painting: Decodable {
        let id: String
        let title: String?
        let artistName: String?
        let image: String?
    }
    let paintings: [Painting]
    let allPaintingsCount: Int
    let pageSize: Int

    enum CodingKeys: String, CodingKey {
        case paintings         = "Paintings"
        case allPaintingsCount = "AllPaintingsCount"
        case pageSize          = "PageSize"
    }
}

private struct WikiArtArtistResponse: Decodable {
    struct Painting: Decodable {
        let id: String
        let title: String?
        let artistName: String?
        let image: String?
    }
    let paintings: [Painting]
    let allPaintingsCount: Int
    let pageSize: Int

    enum CodingKeys: String, CodingKey {
        case paintings         = "Paintings"
        case allPaintingsCount = "AllPaintingsCount"
        case pageSize          = "PageSize"
    }
}

// MARK: - Client

final class WikiArtAPIClient {
    private let styleBaseURL  = "https://www.wikiart.org/en/paintings-by-style"
    private let artistBaseURL = "https://www.wikiart.org/en"
    private let session: URLSession = {
        let config = URLSessionConfiguration.default
        config.timeoutIntervalForRequest = 30
        return URLSession(configuration: config)
    }()

    /// Fetch a random artwork, optionally filtered to styles and/or artists.
    /// AIC style/artist names are mapped to WikiArt slugs; unmapped values are ignored.
    /// Falls back to a random popular style if no valid slugs are found.
    func randomArtwork(styles: Set<String> = [], artists: Set<String> = []) async throws -> AICArtwork {
        // Artist filter takes priority when present and mappable
        let mappedArtistSlugs = artists.compactMap { WikiArtArtistSlugMap[$0] }
        if let artistSlug = mappedArtistSlugs.randomElement() {
            NSLog("[ArtFruit] WikiArt filtering by artist slug: '\(artistSlug)'")
            return try await randomArtworkForArtist(artistSlug)
        }

        // Fall back to style filtering
        let mappedSlugs = styles.compactMap { WikiArtStyleSlugMap[$0] }
        let slug = mappedSlugs.randomElement() ?? wikiArtFallbackSlugs.randomElement()!
        NSLog("[ArtFruit] WikiArt using style slug: '\(slug)'")
        return try await randomArtworkForSlug(slug)
    }

    // MARK: - Artist-filtered path

    private func randomArtworkForArtist(_ artistSlug: String) async throws -> AICArtwork {
        let firstPage = try await fetchArtistPage(1, artistSlug: artistSlug)
        guard firstPage.allPaintingsCount > 0 else {
            NSLog("[ArtFruit] WikiArt: no results for artist '\(artistSlug)', falling back to style")
            let fallbackSlug = wikiArtFallbackSlugs.randomElement()!
            return try await randomArtworkForSlug(fallbackSlug)
        }

        let totalPages = min(firstPage.allPaintingsCount / max(firstPage.pageSize, 1), 60)
        let randomPage = Int.random(in: 1...max(totalPages, 1))
        NSLog("[ArtFruit] WikiArt artist '\(artistSlug)': \(firstPage.allPaintingsCount) artworks, page \(randomPage)/\(totalPages)")

        let page = randomPage == 1 ? firstPage : (try await fetchArtistPage(randomPage, artistSlug: artistSlug))
        let withImages = page.paintings.filter { $0.image != nil && !($0.image!.isEmpty) }

        guard let pick = withImages.randomElement(),
              let imageStr = pick.image,
              let imageURL = URL(string: imageStr) else {
            let fallback = firstPage.paintings.filter { $0.image != nil && !($0.image!.isEmpty) }
            guard let pick2 = fallback.randomElement(),
                  let img2 = pick2.image,
                  let url2 = URL(string: img2) else {
                throw ArtFruitError.noArtworksFound
            }
            return makeArtwork(pick2, imageURL: url2)
        }

        return makeArtwork(pick, imageURL: imageURL)
    }

    private func fetchArtistPage(_ page: Int, artistSlug: String) async throws -> WikiArtArtistResponse {
        let urlStr = "\(artistBaseURL)/\(artistSlug)/all-works/text-list?json=2&layout=new&page=\(page)&resultType=masonry"
        guard let url = URL(string: urlStr) else { throw ArtFruitError.noArtworksFound }
        NSLog("[ArtFruit] WikiArt artist fetching: \(urlStr)")

        let (data, response) = try await session.data(from: url)
        if let http = response as? HTTPURLResponse {
            NSLog("[ArtFruit] WikiArt HTTP \(http.statusCode)")
        }

        return try JSONDecoder().decode(WikiArtArtistResponse.self, from: data)
    }

    // MARK: - Style-filtered path

    private func randomArtworkForSlug(_ slug: String) async throws -> AICArtwork {
        let firstPage = try await fetchPage(1, slug: slug)
        guard firstPage.allPaintingsCount > 0 else {
            NSLog("[ArtFruit] WikiArt: no results for '\(slug)', falling back")
            let fallback = wikiArtFallbackSlugs.filter { $0 != slug }.randomElement()
                ?? wikiArtFallbackSlugs[0]
            return try await randomArtworkForSlug(fallback)
        }

        let totalPages = min(firstPage.allPaintingsCount / max(firstPage.pageSize, 1), 60)
        let randomPage = Int.random(in: 1...max(totalPages, 1))
        NSLog("[ArtFruit] WikiArt '\(slug)': \(firstPage.allPaintingsCount) artworks, page \(randomPage)/\(totalPages)")

        let page = randomPage == 1 ? firstPage : (try await fetchPage(randomPage, slug: slug))
        let withImages = page.paintings.filter { $0.image != nil && !($0.image!.isEmpty) }

        guard let pick = withImages.randomElement(),
              let imageStr = pick.image,
              let imageURL = URL(string: imageStr) else {
            let fallback = firstPage.paintings.filter { $0.image != nil && !($0.image!.isEmpty) }
            guard let pick2 = fallback.randomElement(),
                  let img2 = pick2.image,
                  let url2 = URL(string: img2) else {
                throw ArtFruitError.noArtworksFound
            }
            return makeArtwork(pick2, imageURL: url2)
        }

        return makeArtwork(pick, imageURL: imageURL)
    }

    private func fetchPage(_ page: Int, slug: String) async throws -> WikiArtPageResponse {
        let urlStr = "\(styleBaseURL)/\(slug)?json=2&layout=new&page=\(page)&resultType=masonry"
        guard let url = URL(string: urlStr) else { throw ArtFruitError.noArtworksFound }
        NSLog("[ArtFruit] WikiArt fetching: \(urlStr)")

        let (data, response) = try await session.data(from: url)
        if let http = response as? HTTPURLResponse {
            NSLog("[ArtFruit] WikiArt HTTP \(http.statusCode)")
        }

        return try JSONDecoder().decode(WikiArtPageResponse.self, from: data)
    }

    // MARK: - Shared helpers

    private func makeArtwork(_ painting: WikiArtPageResponse.Painting, imageURL: URL) -> AICArtwork {
        NSLog("[ArtFruit] WikiArt selected: \"\(painting.title ?? "nil")\" by \(painting.artistName ?? "nil")")
        return AICArtwork(
            id: abs(painting.id.hashValue),
            title: painting.title ?? "Untitled",
            artist: painting.artistName ?? "Unknown Artist",
            imageURL: imageURL
        )
    }

    private func makeArtwork(_ painting: WikiArtArtistResponse.Painting, imageURL: URL) -> AICArtwork {
        NSLog("[ArtFruit] WikiArt selected: \"\(painting.title ?? "nil")\" by \(painting.artistName ?? "nil")")
        return AICArtwork(
            id: abs(painting.id.hashValue),
            title: painting.title ?? "Untitled",
            artist: painting.artistName ?? "Unknown Artist",
            imageURL: imageURL
        )
    }
}
