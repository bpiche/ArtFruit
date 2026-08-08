import Foundation

// MARK: - Style groups (display name → WikiArt URL slug)

/// A single style entry: user-facing name + WikiArt slug.
struct WikiArtStyle: Hashable {
    let name: String
    let slug: String
}

/// A group of related styles shown as a collapsible section in Preferences.
struct WikiArtStyleGroup {
    let name: String
    let styles: [WikiArtStyle]
}

/// The complete grouped style taxonomy, mirroring WikiArt's own groupings.
let WikiArtStyleGroups: [WikiArtStyleGroup] = [
    WikiArtStyleGroup(name: "Modern & Contemporary", styles: [
        WikiArtStyle(name: "Impressionism",          slug: "impressionism"),
        WikiArtStyle(name: "Post-Impressionism",     slug: "post-impressionism"),
        WikiArtStyle(name: "Expressionism",          slug: "expressionism"),
        WikiArtStyle(name: "Fauvism",                slug: "fauvism"),
        WikiArtStyle(name: "Cubism",                 slug: "cubism"),
        WikiArtStyle(name: "Futurism",               slug: "futurism"),
        WikiArtStyle(name: "Dadaism",                slug: "dadaism"),
        WikiArtStyle(name: "Surrealism",             slug: "surrealism"),
        WikiArtStyle(name: "Abstract Expressionism", slug: "abstract-expressionism"),
        WikiArtStyle(name: "Abstract Art",           slug: "abstract-art"),
        WikiArtStyle(name: "Pop Art",                slug: "pop-art"),
        WikiArtStyle(name: "Minimalism",             slug: "minimalism"),
        WikiArtStyle(name: "Op Art",                 slug: "op-art"),
        WikiArtStyle(name: "Conceptual Art",         slug: "conceptual-art"),
        WikiArtStyle(name: "Neo-Expressionism",      slug: "neo-expressionism"),
        WikiArtStyle(name: "Color Field Painting",   slug: "color-field-painting"),
        WikiArtStyle(name: "Lyrical Abstraction",    slug: "lyrical-abstraction"),
        WikiArtStyle(name: "Street Art",             slug: "street-art"),
        WikiArtStyle(name: "Outsider Art",           slug: "outsider-art"),
        WikiArtStyle(name: "Naive Art / Primitivism",slug: "naive-art-primitivism"),
    ]),
    WikiArtStyleGroup(name: "Modern Movements", styles: [
        WikiArtStyle(name: "Art Nouveau",            slug: "art-nouveau-modern"),
        WikiArtStyle(name: "Art Deco",               slug: "art-deco"),
        WikiArtStyle(name: "Symbolism",              slug: "symbolism"),
        WikiArtStyle(name: "Modernism",              slug: "modernism"),
        WikiArtStyle(name: "Constructivism",         slug: "constructivism"),
        WikiArtStyle(name: "Suprematism",            slug: "suprematism"),
        WikiArtStyle(name: "De Stijl",               slug: "de-stijl"),
        WikiArtStyle(name: "Bauhaus",                slug: "bauhaus"),
        WikiArtStyle(name: "Neue Sachlichkeit",      slug: "new-objectivity"),
        WikiArtStyle(name: "Pittura Metafisica",     slug: "pittura-metafisica"),
        WikiArtStyle(name: "Les Nabis",              slug: "les-nabis"),
        WikiArtStyle(name: "Synthetism",             slug: "synthetism"),
        WikiArtStyle(name: "Cloisonnism",            slug: "cloisonnism"),
        WikiArtStyle(name: "Pointillism",            slug: "pointillism"),
        WikiArtStyle(name: "Divisionism",            slug: "divisionism"),
        WikiArtStyle(name: "Japonisme",              slug: "japonisme"),
        WikiArtStyle(name: "Tachisme",               slug: "tachisme"),
        WikiArtStyle(name: "Art Informel",           slug: "art-informel"),
        WikiArtStyle(name: "COBRA",                  slug: "cobra"),
        WikiArtStyle(name: "Magic Realism",          slug: "magic-realism"),
    ]),
    WikiArtStyleGroup(name: "Realism & Naturalism", styles: [
        WikiArtStyle(name: "Realism",                slug: "realism"),
        WikiArtStyle(name: "Social Realism",         slug: "social-realism"),
        WikiArtStyle(name: "American Realism",       slug: "american-realism"),
        WikiArtStyle(name: "Socialist Realism",      slug: "socialist-realism"),
        WikiArtStyle(name: "Photorealism",           slug: "photorealism"),
        WikiArtStyle(name: "Hyperrealism",           slug: "hyperrealism"),
        WikiArtStyle(name: "Hudson River School",    slug: "hudson-river-school"),
        WikiArtStyle(name: "Ashcan School",          slug: "ashcan-school"),
        WikiArtStyle(name: "Tonalism",               slug: "tonalism"),
        WikiArtStyle(name: "Luminism",               slug: "luminism"),
        WikiArtStyle(name: "Orientalism",            slug: "orientalism"),
        WikiArtStyle(name: "Academic Art",           slug: "academic-art"),
    ]),
    WikiArtStyleGroup(name: "Renaissance & Baroque", styles: [
        WikiArtStyle(name: "Early Renaissance",      slug: "early-renaissance"),
        WikiArtStyle(name: "High Renaissance",       slug: "high-renaissance"),
        WikiArtStyle(name: "Northern Renaissance",   slug: "northern-renaissance"),
        WikiArtStyle(name: "Mannerism",              slug: "mannerism-late-renaissance"),
        WikiArtStyle(name: "Baroque",                slug: "baroque"),
        WikiArtStyle(name: "Rococo",                 slug: "rococo"),
    ]),
    WikiArtStyleGroup(name: "Neoclassicism & Romanticism", styles: [
        WikiArtStyle(name: "Neoclassicism",          slug: "neoclassicism"),
        WikiArtStyle(name: "Romanticism",            slug: "romanticism"),
        WikiArtStyle(name: "Pre-Raphaelite Brotherhood", slug: "pre-raphaelite-brotherhood"),
        WikiArtStyle(name: "Biedermeier",            slug: "biedermeier"),
        WikiArtStyle(name: "Neo-Impressionism",      slug: "neo-impressionism"),
    ]),
    WikiArtStyleGroup(name: "Medieval & Early", styles: [
        WikiArtStyle(name: "Gothic",                 slug: "gothic"),
        WikiArtStyle(name: "Byzantine",              slug: "byzantine"),
        WikiArtStyle(name: "Medieval",               slug: "medieval"),
        WikiArtStyle(name: "Early Christian",        slug: "early-christian"),
    ]),
    WikiArtStyleGroup(name: "Ancient", styles: [
        WikiArtStyle(name: "Ancient Egyptian",       slug: "ancient-egyptian-art"),
        WikiArtStyle(name: "Ancient Greek",          slug: "ancient-greek-art"),
        WikiArtStyle(name: "Ancient Roman",          slug: "ancient-roman-art"),
    ]),
    WikiArtStyleGroup(name: "Asian & World", styles: [
        WikiArtStyle(name: "Ukiyo-e",                slug: "ukiyo-e"),
        WikiArtStyle(name: "Japanese Art",           slug: "japanese-art"),
        WikiArtStyle(name: "Chinese Art",            slug: "chinese-art"),
        WikiArtStyle(name: "Indian Art",             slug: "indian-art"),
        WikiArtStyle(name: "Islamic Art",            slug: "islamic-art"),
        WikiArtStyle(name: "Folk Art",               slug: "folk-art"),
        WikiArtStyle(name: "Tribal Art",             slug: "tribal-art"),
    ]),
]

/// Flat map from display name → WikiArt slug (used for API lookups).
let WikiArtStyleSlugMap: [String: String] = {
    var map: [String: String] = [:]
    for group in WikiArtStyleGroups {
        for style in group.styles {
            map[style.name] = style.slug
        }
    }
    return map
}()

// MARK: - Artist slug mapping (display name → WikiArt URL slug)

struct WikiArtArtist: Hashable {
    let name: String
    let slug: String
}

struct WikiArtArtistGroup {
    let name: String
    let artists: [WikiArtArtist]
}

/// Grouped artist list for the Preferences Artists tab.
let WikiArtArtistGroups: [WikiArtArtistGroup] = [
    WikiArtArtistGroup(name: "Impressionism", artists: [
        WikiArtArtist(name: "Claude Monet",                        slug: "claude-monet"),
        WikiArtArtist(name: "Pierre-Auguste Renoir",               slug: "pierre-auguste-renoir"),
        WikiArtArtist(name: "Edgar Degas",                         slug: "edgar-degas"),
        WikiArtArtist(name: "Camille Pissarro",                    slug: "camille-pissarro"),
        WikiArtArtist(name: "Alfred Sisley",                       slug: "alfred-sisley"),
        WikiArtArtist(name: "Berthe Morisot",                      slug: "berthe-morisot"),
        WikiArtArtist(name: "Mary Cassatt",                        slug: "mary-cassatt"),
        WikiArtArtist(name: "Gustave Caillebotte",                 slug: "gustave-caillebotte"),
    ]),
    WikiArtArtistGroup(name: "Post-Impressionism", artists: [
        WikiArtArtist(name: "Paul Cézanne",                        slug: "paul-cezanne"),
        WikiArtArtist(name: "Vincent van Gogh",                    slug: "vincent-van-gogh"),
        WikiArtArtist(name: "Paul Gauguin",                        slug: "paul-gauguin"),
        WikiArtArtist(name: "Georges Seurat",                      slug: "georges-seurat"),
        WikiArtArtist(name: "Henri de Toulouse-Lautrec",           slug: "henri-de-toulouse-lautrec"),
        WikiArtArtist(name: "Paul Signac",                         slug: "paul-signac"),
        WikiArtArtist(name: "Odilon Redon",                        slug: "odilon-redon"),
    ]),
    WikiArtArtistGroup(name: "Modern Masters", artists: [
        WikiArtArtist(name: "Pablo Picasso",                       slug: "pablo-picasso"),
        WikiArtArtist(name: "Henri Matisse",                       slug: "henri-matisse"),
        WikiArtArtist(name: "Wassily Kandinsky",                   slug: "wassily-kandinsky"),
        WikiArtArtist(name: "Paul Klee",                           slug: "paul-klee"),
        WikiArtArtist(name: "Piet Mondrian",                       slug: "piet-mondrian"),
        WikiArtArtist(name: "Amedeo Modigliani",                   slug: "amedeo-modigliani"),
        WikiArtArtist(name: "Marc Chagall",                        slug: "marc-chagall"),
        WikiArtArtist(name: "Fernand Léger",                       slug: "fernand-leger"),
        WikiArtArtist(name: "Georges Braque",                      slug: "georges-braque"),
        WikiArtArtist(name: "Juan Gris",                           slug: "juan-gris"),
        WikiArtArtist(name: "Robert Delaunay",                     slug: "robert-delaunay"),
    ]),
    WikiArtArtistGroup(name: "Expressionism & Symbolism", artists: [
        WikiArtArtist(name: "Edvard Munch",                        slug: "edvard-munch"),
        WikiArtArtist(name: "Egon Schiele",                        slug: "egon-schiele"),
        WikiArtArtist(name: "Gustav Klimt",                        slug: "gustav-klimt"),
        WikiArtArtist(name: "Oskar Kokoschka",                     slug: "oskar-kokoschka"),
        WikiArtArtist(name: "Ernst Ludwig Kirchner",               slug: "ernst-ludwig-kirchner"),
        WikiArtArtist(name: "Emil Nolde",                          slug: "emil-nolde"),
        WikiArtArtist(name: "Franz Marc",                          slug: "franz-marc"),
        WikiArtArtist(name: "Wassily Kandinsky",                   slug: "wassily-kandinsky"),
        WikiArtArtist(name: "Gustav Moreau",                       slug: "gustave-moreau"),
        WikiArtArtist(name: "Gustave Moreau",                      slug: "gustave-moreau"),
    ]),
    WikiArtArtistGroup(name: "Surrealism & Dada", artists: [
        WikiArtArtist(name: "Salvador Dalí",                       slug: "salvador-dali"),
        WikiArtArtist(name: "René Magritte",                       slug: "rene-magritte"),
        WikiArtArtist(name: "Max Ernst",                           slug: "max-ernst"),
        WikiArtArtist(name: "Joan Miró",                           slug: "joan-miro"),
        WikiArtArtist(name: "Giorgio de Chirico",                  slug: "giorgio-de-chirico"),
        WikiArtArtist(name: "Frida Kahlo",                         slug: "frida-kahlo"),
        WikiArtArtist(name: "Man Ray",                             slug: "man-ray"),
        WikiArtArtist(name: "Marcel Duchamp",                      slug: "marcel-duchamp"),
        WikiArtArtist(name: "Yves Tanguy",                         slug: "yves-tanguy"),
        WikiArtArtist(name: "Paul Delvaux",                        slug: "paul-delvaux"),
    ]),
    WikiArtArtistGroup(name: "Abstract & Post-War", artists: [
        WikiArtArtist(name: "Jackson Pollock",                     slug: "jackson-pollock"),
        WikiArtArtist(name: "Mark Rothko",                         slug: "mark-rothko"),
        WikiArtArtist(name: "Willem de Kooning",                   slug: "willem-de-kooning"),
        WikiArtArtist(name: "Franz Kline",                         slug: "franz-kline"),
        WikiArtArtist(name: "Lee Krasner",                         slug: "lee-krasner"),
        WikiArtArtist(name: "Clyfford Still",                      slug: "clyfford-still"),
        WikiArtArtist(name: "Barnett Newman",                      slug: "barnett-newman"),
        WikiArtArtist(name: "Arshile Gorky",                       slug: "arshile-gorky"),
        WikiArtArtist(name: "Helen Frankenthaler",                 slug: "helen-frankenthaler"),
    ]),
    WikiArtArtistGroup(name: "Pop Art & Contemporary", artists: [
        WikiArtArtist(name: "Andy Warhol",                         slug: "andy-warhol"),
        WikiArtArtist(name: "Roy Lichtenstein",                    slug: "roy-lichtenstein"),
        WikiArtArtist(name: "Jasper Johns",                        slug: "jasper-johns"),
        WikiArtArtist(name: "Robert Rauschenberg",                 slug: "robert-rauschenberg"),
        WikiArtArtist(name: "David Hockney",                       slug: "david-hockney"),
        WikiArtArtist(name: "Keith Haring",                        slug: "keith-haring"),
        WikiArtArtist(name: "Jean-Michel Basquiat",                slug: "jean-michel-basquiat"),
        WikiArtArtist(name: "Banksy",                              slug: "banksy"),
    ]),
    WikiArtArtistGroup(name: "American Art", artists: [
        WikiArtArtist(name: "Winslow Homer",                       slug: "winslow-homer"),
        WikiArtArtist(name: "John Singer Sargent",                 slug: "john-singer-sargent"),
        WikiArtArtist(name: "James McNeill Whistler",              slug: "james-mcneill-whistler"),
        WikiArtArtist(name: "Georgia O'Keeffe",                    slug: "georgia-o-keeffe"),
        WikiArtArtist(name: "Edward Hopper",                       slug: "edward-hopper"),
        WikiArtArtist(name: "Grant Wood",                          slug: "grant-wood"),
        WikiArtArtist(name: "Thomas Eakins",                       slug: "thomas-eakins"),
        WikiArtArtist(name: "Mary Cassatt",                        slug: "mary-cassatt"),
        WikiArtArtist(name: "Frederic Remington",                  slug: "frederic-remington"),
        WikiArtArtist(name: "Albert Bierstadt",                    slug: "albert-bierstadt"),
        WikiArtArtist(name: "Thomas Cole",                         slug: "thomas-cole"),
    ]),
    WikiArtArtistGroup(name: "Old Masters – Italian", artists: [
        WikiArtArtist(name: "Leonardo da Vinci",                   slug: "leonardo-da-vinci"),
        WikiArtArtist(name: "Michelangelo Buonarroti",             slug: "michelangelo"),
        WikiArtArtist(name: "Raphael",                             slug: "raphael"),
        WikiArtArtist(name: "Sandro Botticelli",                   slug: "sandro-botticelli"),
        WikiArtArtist(name: "Caravaggio",                          slug: "caravaggio"),
        WikiArtArtist(name: "Titian",                              slug: "titian"),
        WikiArtArtist(name: "Giovanni Bellini",                    slug: "giovanni-bellini"),
        WikiArtArtist(name: "Giotto di Bondone",                   slug: "giotto-di-bondone"),
        WikiArtArtist(name: "Fra Angelico",                        slug: "fra-angelico"),
        WikiArtArtist(name: "Piero della Francesca",               slug: "piero-della-francesca"),
        WikiArtArtist(name: "Paolo Veronese",                      slug: "paolo-veronese"),
        WikiArtArtist(name: "Tintoretto",                          slug: "tintoretto"),
        WikiArtArtist(name: "Canaletto",                           slug: "canaletto"),
        WikiArtArtist(name: "Giovanni Battista Tiepolo",           slug: "giovanni-battista-tiepolo"),
    ]),
    WikiArtArtistGroup(name: "Old Masters – Northern European", artists: [
        WikiArtArtist(name: "Rembrandt van Rijn",                  slug: "rembrandt"),
        WikiArtArtist(name: "Johannes Vermeer",                    slug: "johannes-vermeer"),
        WikiArtArtist(name: "Peter Paul Rubens",                   slug: "peter-paul-rubens"),
        WikiArtArtist(name: "Jan van Eyck",                        slug: "jan-van-eyck"),
        WikiArtArtist(name: "Hieronymus Bosch",                    slug: "hieronymus-bosch"),
        WikiArtArtist(name: "Albrecht Dürer",                      slug: "albrecht-durer"),
        WikiArtArtist(name: "Hans Holbein the Younger",            slug: "hans-holbein-the-younger"),
        WikiArtArtist(name: "Pieter Bruegel the Elder",            slug: "pieter-bruegel-the-elder"),
        WikiArtArtist(name: "Anthony van Dyck",                    slug: "anthony-van-dyck"),
        WikiArtArtist(name: "Frans Hals",                          slug: "frans-hals"),
        WikiArtArtist(name: "Jan Steen",                           slug: "jan-steen"),
        WikiArtArtist(name: "Caspar David Friedrich",              slug: "caspar-david-friedrich"),
    ]),
    WikiArtArtistGroup(name: "Old Masters – Spanish & Other", artists: [
        WikiArtArtist(name: "Francisco José de Goya y Lucientes",  slug: "francisco-goya"),
        WikiArtArtist(name: "Diego Velázquez",                     slug: "diego-velazquez"),
        WikiArtArtist(name: "El Greco",                            slug: "el-greco"),
        WikiArtArtist(name: "Bartolomé Esteban Murillo",           slug: "bartolome-esteban-murillo"),
        WikiArtArtist(name: "Francisco de Zurbarán",               slug: "francisco-de-zurbaran"),
    ]),
    WikiArtArtistGroup(name: "19th Century", artists: [
        WikiArtArtist(name: "Édouard Manet",                       slug: "edouard-manet"),
        WikiArtArtist(name: "Gustave Courbet",                     slug: "gustave-courbet"),
        WikiArtArtist(name: "Eugène Delacroix",                    slug: "eugene-delacroix"),
        WikiArtArtist(name: "Jean-Auguste-Dominique Ingres",       slug: "jean-auguste-dominique-ingres"),
        WikiArtArtist(name: "Jacques-Louis David",                 slug: "jacques-louis-david"),
        WikiArtArtist(name: "William Bouguereau",                  slug: "william-bouguereau"),
        WikiArtArtist(name: "Jean-Baptiste-Camille Corot",         slug: "jean-baptiste-camille-corot"),
        WikiArtArtist(name: "Honoré Daumier",                      slug: "honore-daumier"),
        WikiArtArtist(name: "William Turner",                      slug: "william-turner"),
        WikiArtArtist(name: "John Constable",                      slug: "john-constable"),
        WikiArtArtist(name: "Dante Gabriel Rossetti",              slug: "dante-gabriel-rossetti"),
        WikiArtArtist(name: "John Everett Millais",                slug: "john-everett-millais"),
        WikiArtArtist(name: "Gustave Moreau",                      slug: "gustave-moreau"),
        WikiArtArtist(name: "Arnold Böcklin",                      slug: "arnold-bocklin"),
    ]),
    WikiArtArtistGroup(name: "Japanese", artists: [
        WikiArtArtist(name: "Katsushika Hokusai",                  slug: "katsushika-hokusai"),
        WikiArtArtist(name: "Utagawa Hiroshige",                   slug: "utagawa-hiroshige"),
        WikiArtArtist(name: "Kitagawa Utamaro",                    slug: "kitagawa-utamaro"),
        WikiArtArtist(name: "Utagawa Kuniyoshi",                   slug: "utagawa-kuniyoshi"),
        WikiArtArtist(name: "Utagawa Kunisada",                    slug: "utagawa-kunisada"),
        WikiArtArtist(name: "Kawase Hasui",                        slug: "kawase-hasui"),
    ]),
]

/// Flat map from display name → WikiArt slug (used for API lookups).
let WikiArtArtistSlugMap: [String: String] = {
    var map: [String: String] = [:]
    for group in WikiArtArtistGroups {
        for artist in group.artists {
            map[artist.name] = artist.slug
        }
    }
    return map
}()

// MARK: - Fallback slugs (used when no filter is selected)

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
    "high-renaissance",
    "symbolism",
    "fauvism",
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
    /// Display names are mapped to WikiArt slugs; unmapped values are ignored.
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
