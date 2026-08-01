import Foundation

// MARK: - Models

struct AICArtwork: Identifiable {
    let id: Int
    let title: String
    let artist: String
    let imageURL: URL
}

// MARK: - Available artists (with known public-domain works in AIC and/or WikiArt)

/// Artists available in the Art Institute of Chicago collection.
/// The display name matches the `artist_title` field returned by the AIC API.
let AICAvailableArtists: [String] = [
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
]

// MARK: - Available styles (sourced from AIC aggregations on public-domain artworks)

let AICAvailableStyles: [String] = [
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
]

// MARK: - Private response types

private struct AICListResponse: Decodable {
    struct Pagination: Decodable {
        let total: Int
        let totalPages: Int
        enum CodingKeys: String, CodingKey {
            case total
            case totalPages = "total_pages"
        }
    }
    struct ArtworkData: Decodable {
        let id: Int
        let title: String?
        let artistTitle: String?
        let imageId: String?
        enum CodingKeys: String, CodingKey {
            case id
            case title
            case artistTitle = "artist_title"
            case imageId = "image_id"
        }
    }
    let pagination: Pagination
    let data: [ArtworkData]
}

private struct AICSearchResponse: Decodable {
    struct Pagination: Decodable {
        let total: Int
        let totalPages: Int
        enum CodingKeys: String, CodingKey {
            case total
            case totalPages = "total_pages"
        }
    }
    struct ArtworkData: Decodable {
        let id: Int
        let title: String?
        let artistTitle: String?
        let imageId: String?
        enum CodingKeys: String, CodingKey {
            case id
            case title
            case artistTitle = "artist_title"
            case imageId = "image_id"
        }
    }
    let pagination: Pagination
    let data: [ArtworkData]
}

// MARK: - Client

final class AICAPIClient {
    private let baseURL = "https://api.artic.edu/api/v1"
    private let iiifBase = "https://www.artic.edu/iiif/2"
    private let session: URLSession = {
        let config = URLSessionConfiguration.default
        config.timeoutIntervalForRequest = 30
        return URLSession(configuration: config)
    }()

    /// Fetch a random public-domain artwork, optionally filtered to one of the given styles
    /// and/or one of the given artists.
    /// Empty sets mean "no filter — show everything".
    func randomArtwork(styles: Set<String> = [], artists: Set<String> = []) async throws -> AICArtwork {
        if styles.isEmpty && artists.isEmpty {
            return try await randomArtworkUnfiltered()
        } else {
            return try await randomArtworkFiltered(styles: styles, artists: artists)
        }
    }

    // MARK: - Unfiltered path (original behaviour)

    private func randomArtworkUnfiltered() async throws -> AICArtwork {
        let firstPage = try await fetchArtworkPage(1)
        let totalPages = min(firstPage.pagination.totalPages, 12000)
        NSLog("[ArtFruit] Total pages: \(totalPages)")

        let randomPage = Int.random(in: 1...totalPages)
        NSLog("[ArtFruit] Fetching page \(randomPage)")

        let page = randomPage == 1 ? firstPage : (try await fetchArtworkPage(randomPage))
        let withImages = page.data.filter { $0.imageId != nil && !($0.imageId!.isEmpty) }
        NSLog("[ArtFruit] Artworks with images on this page: \(withImages.count)")

        guard let pick = withImages.randomElement(), let imageId = pick.imageId else {
            let fallback = firstPage.data.filter { $0.imageId != nil && !($0.imageId!.isEmpty) }
            guard let pick2 = fallback.randomElement(), let imageId2 = pick2.imageId else {
                throw ArtFruitError.noArtworksFound
            }
            return makeArtwork(pick2, imageId: imageId2)
        }

        return makeArtwork(pick, imageId: imageId)
    }

    private func fetchArtworkPage(_ page: Int) async throws -> AICListResponse {
        let urlStr = "\(baseURL)/artworks?page=\(page)&limit=100&fields=id,title,artist_title,image_id&is_public_domain=1"
        guard let url = URL(string: urlStr) else { throw ArtFruitError.noArtworksFound }
        NSLog("[ArtFruit] Fetching: \(urlStr)")

        let (data, response) = try await session.data(from: url)
        if let http = response as? HTTPURLResponse {
            NSLog("[ArtFruit] HTTP \(http.statusCode)")
        }

        return try JSONDecoder().decode(AICListResponse.self, from: data)
    }

    // MARK: - Style/Artist-filtered path (search endpoint)

    private func randomArtworkFiltered(styles: Set<String>, artists: Set<String>) async throws -> AICArtwork {
        // Build must-clauses for both style and artist filters
        var mustClauses: [[String: Any]] = [
            ["term": ["is_public_domain": true]],
            ["exists": ["field": "image_id"]]
        ]
        if !styles.isEmpty {
            let chosenStyle = Array(styles).randomElement()!
            mustClauses.append(["term": ["style_title.keyword": chosenStyle]])
            NSLog("[ArtFruit] AIC filtering by style: '\(chosenStyle)'")
        }
        if !artists.isEmpty {
            let chosenArtist = Array(artists).randomElement()!
            mustClauses.append(["match": ["artist_title": chosenArtist]])
            NSLog("[ArtFruit] AIC filtering by artist: '\(chosenArtist)'")
        }

        let firstPage = try await fetchSearchPage(1, mustClauses: mustClauses)
        guard firstPage.pagination.total > 0 else {
            NSLog("[ArtFruit] No results for the given filters, falling back to unfiltered.")
            return try await randomArtworkUnfiltered()
        }

        // Cap at 100 pages (10,000 results — Elasticsearch limit)
        let totalPages = min(firstPage.pagination.totalPages, 100)
        NSLog("[ArtFruit] AIC filtered: \(firstPage.pagination.total) artworks, \(totalPages) pages")

        let randomPage = Int.random(in: 1...totalPages)
        let page = randomPage == 1 ? firstPage : (try await fetchSearchPage(randomPage, mustClauses: mustClauses))

        let withImages = page.data.filter { $0.imageId != nil && !($0.imageId!.isEmpty) }
        NSLog("[ArtFruit] Artworks with images on page \(randomPage): \(withImages.count)")

        guard let pick = withImages.randomElement(), let imageId = pick.imageId else {
            let fallback = firstPage.data.filter { $0.imageId != nil && !($0.imageId!.isEmpty) }
            guard let pick2 = fallback.randomElement(), let imageId2 = pick2.imageId else {
                throw ArtFruitError.noArtworksFound
            }
            return makeArtwork(pick2, imageId: imageId2)
        }

        return makeArtwork(pick, imageId: imageId)
    }

    private func fetchSearchPage(_ page: Int, mustClauses: [[String: Any]]) async throws -> AICSearchResponse {
        let query: [String: Any] = [
            "query": [
                "bool": [
                    "must": mustClauses
                ]
            ],
            "fields": ["id", "title", "artist_title", "image_id"],
            "_source": ["id", "title", "artist_title", "image_id"],
            "from": (page - 1) * 100,
            "size": 100
        ]

        let urlStr = "\(baseURL)/artworks/search"
        guard let url = URL(string: urlStr) else { throw ArtFruitError.noArtworksFound }

        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.httpBody = try JSONSerialization.data(withJSONObject: query)

        NSLog("[ArtFruit] AIC search POST page \(page)")
        let (data, response) = try await session.data(for: request)
        if let http = response as? HTTPURLResponse {
            NSLog("[ArtFruit] HTTP \(http.statusCode)")
        }

        return try JSONDecoder().decode(AICSearchResponse.self, from: data)
    }

    // MARK: - Shared helpers

    private func makeArtwork(_ data: AICListResponse.ArtworkData, imageId: String) -> AICArtwork {
        let imageURL = URL(string: "\(iiifBase)/\(imageId)/full/1400,/0/default.jpg")!
        NSLog("[ArtFruit] Selected: \"\(data.title ?? "nil")\" by \(data.artistTitle ?? "nil")")
        return AICArtwork(
            id: data.id,
            title: data.title ?? "Untitled",
            artist: data.artistTitle ?? "Unknown Artist",
            imageURL: imageURL
        )
    }

    private func makeArtwork(_ data: AICSearchResponse.ArtworkData, imageId: String) -> AICArtwork {
        let imageURL = URL(string: "\(iiifBase)/\(imageId)/full/1400,/0/default.jpg")!
        NSLog("[ArtFruit] Selected: \"\(data.title ?? "nil")\" by \(data.artistTitle ?? "nil")")
        return AICArtwork(
            id: data.id,
            title: data.title ?? "Untitled",
            artist: data.artistTitle ?? "Unknown Artist",
            imageURL: imageURL
        )
    }
}

enum ArtFruitError: LocalizedError {
    case noArtworksFound

    var errorDescription: String? {
        switch self {
        case .noArtworksFound:
            return "No artworks could be found."
        }
    }
}
