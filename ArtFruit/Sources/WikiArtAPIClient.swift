import Foundation

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

// MARK: - Client

final class WikiArtAPIClient {
    private let baseURL = "https://www.wikiart.org/en/paintings-by-style"
    private let session: URLSession = {
        let config = URLSessionConfiguration.default
        config.timeoutIntervalForRequest = 30
        return URLSession(configuration: config)
    }()

    /// Fetch a random artwork, optionally filtered to styles from the shared style list.
    /// AIC style names are mapped to WikiArt slugs; unmapped styles are ignored.
    /// Falls back to a random popular style if no styles are selected or none map.
    func randomArtwork(styles: Set<String> = []) async throws -> AICArtwork {
        let mappedSlugs = styles.compactMap { WikiArtStyleSlugMap[$0] }
        let slug = mappedSlugs.randomElement() ?? wikiArtFallbackSlugs.randomElement()!
        NSLog("[ArtFruit] WikiArt using style slug: '\(slug)'")
        return try await randomArtworkForSlug(slug)
    }

    // MARK: - Private helpers

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
        let urlStr = "\(baseURL)/\(slug)?json=2&layout=new&page=\(page)&resultType=masonry"
        guard let url = URL(string: urlStr) else { throw ArtFruitError.noArtworksFound }
        NSLog("[ArtFruit] WikiArt fetching: \(urlStr)")

        let (data, response) = try await session.data(from: url)
        if let http = response as? HTTPURLResponse {
            NSLog("[ArtFruit] WikiArt HTTP \(http.statusCode)")
        }

        return try JSONDecoder().decode(WikiArtPageResponse.self, from: data)
    }

    private func makeArtwork(_ painting: WikiArtPageResponse.Painting, imageURL: URL) -> AICArtwork {
        NSLog("[ArtFruit] WikiArt selected: \"\(painting.title ?? "nil")\" by \(painting.artistName ?? "nil")")
        return AICArtwork(
            id: abs(painting.id.hashValue),
            title: painting.title ?? "Untitled",
            artist: painting.artistName ?? "Unknown Artist",
            imageURL: imageURL
        )
    }
}
