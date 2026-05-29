import Combine
import Foundation
import AppKit
import UserNotifications

// MARK: - Available sources

let ArtFruitSources: [String] = [
    "The Art Institute of Chicago",
    "WikiArt",
]

@MainActor
final class ArtFruitViewModel: ObservableObject {
    @Published var isPaused = false
    @Published var currentTitle: String?
    @Published var currentArtist: String?
    @Published private(set) var currentImageURL: URL?
    @Published var changeIntervalMinutes: Double = 60 {
        didSet { rescheduleTimer() }
    }

    @Published var showTitle: Bool {
        didSet {
            UserDefaults.standard.set(showTitle, forKey: "showTitle")
        }
    }

    @Published var showArtist: Bool {
        didSet {
            UserDefaults.standard.set(showArtist, forKey: "showArtist")
        }
    }

    /// The set of style titles the user has selected in Preferences.
    /// An empty set means "no filter — show everything".
    @Published var selectedStyles: Set<String> {
        didSet {
            UserDefaults.standard.set(Array(selectedStyles), forKey: "selectedStyles")
        }
    }

    /// The set of sources the user has selected in Preferences.
    /// An empty set means "use all sources".
    @Published var selectedSources: Set<String> {
        didSet {
            UserDefaults.standard.set(Array(selectedSources), forKey: "selectedSources")
        }
    }

    /// When true, a different artwork is fetched and applied to each connected monitor.
    @Published var multiMonitor: Bool {
        didSet {
            UserDefaults.standard.set(multiMonitor, forKey: "multiMonitor")
        }
    }

    private let apiClient = AICAPIClient()
    private let wikiArtClient = WikiArtAPIClient()
    private let wallpaperService = WallpaperService()
    private var timer: Timer?

    /// Artwork for each screen, indexed in the same order as NSScreen.screens.
    private var screenArtworks: [AICArtwork] = []

    init() {
        let saved = UserDefaults.standard.stringArray(forKey: "selectedStyles") ?? []
        selectedStyles = Set(saved)
        let savedSources = UserDefaults.standard.stringArray(forKey: "selectedSources") ?? []
        selectedSources = Set(savedSources)
        multiMonitor = UserDefaults.standard.bool(forKey: "multiMonitor")
        // Default both to true for first-time users
        let showTitleSaved = UserDefaults.standard.object(forKey: "showTitle")
        showTitle = showTitleSaved != nil ? UserDefaults.standard.bool(forKey: "showTitle") : true
        let showArtistSaved = UserDefaults.standard.object(forKey: "showArtist")
        showArtist = showArtistSaved != nil ? UserDefaults.standard.bool(forKey: "showArtist") : true
    }

    func startRotation() {
        requestNotificationPermission()
        fetchAndApplyArtwork()
        scheduleTimer()
    }

    func fetchAndApplyArtwork() {
        guard !isPaused else { return }

        Task {
            do {
                if multiMonitor && NSScreen.screens.count > 1 {
                    // Fetch a unique artwork for each screen in parallel.
                    // Tag each task with its screen index so we preserve ordering.
                    NSLog("[ArtFruit] Multi-monitor mode: fetching \(NSScreen.screens.count) artworks...")
                    let screens = NSScreen.screens
                    let tagged = try await withThrowingTaskGroup(of: (Int, AICArtwork).self) { group in
                        for index in screens.indices {
                            group.addTask { [self] in
                                let artwork = try await self.fetchOneArtwork()
                                return (index, artwork)
                            }
                        }
                        var pairs: [(Int, AICArtwork)] = []
                        for try await pair in group { pairs.append(pair) }
                        return pairs
                    }
                    let artworks = tagged.sorted { $0.0 < $1.0 }.map(\.1)
                    screenArtworks = artworks
                    // Apply each artwork to its corresponding screen
                    for (index, screen) in screens.enumerated() {
                        let artwork = artworks[index % artworks.count]
                        try await wallpaperService.apply(
                            imageURL: artwork.imageURL,
                            title: artwork.title,
                            artist: artwork.artist,
                            showTitle: showTitle,
                            showArtist: showArtist,
                            screens: [screen]
                        )
                    }
                    // Track the artwork on the primary screen
                    let primaryArtwork = artworks[0 % artworks.count]
                    currentTitle = primaryArtwork.title
                    currentArtist = primaryArtwork.artist
                    currentImageURL = primaryArtwork.imageURL
                    showNotification(title: "New Artwork", body: "\(primaryArtwork.title) — \(primaryArtwork.artist)")
                } else {
                    let artwork = try await fetchOneArtwork()
                    currentTitle = artwork.title
                    currentArtist = artwork.artist
                    currentImageURL = artwork.imageURL
                    NSLog("[ArtFruit] Got artwork: \"\(artwork.title)\" by \(artwork.artist) — \(artwork.imageURL)")
                    try await wallpaperService.apply(
                        imageURL: artwork.imageURL,
                        title: artwork.title,
                        artist: artwork.artist,
                        showTitle: showTitle,
                        showArtist: showArtist
                    )
                    NSLog("[ArtFruit] Wallpaper applied successfully.")
                    showNotification(title: "New Artwork", body: "\(artwork.title) — \(artwork.artist)")
                }
            } catch {
                NSLog("[ArtFruit] ERROR: \(error.localizedDescription)")
                showNotification(title: "ArtFruit Error", body: error.localizedDescription)
            }
        }
    }

    /// Downloads the artwork from the screen under the user's mouse to ~/Downloads.
    func saveCurrentArtwork() {
        let screens = NSScreen.screens
        let mouseLocation = NSEvent.mouseLocation
        let screenIndex = screens.firstIndex(where: {
            $0.frame.contains(mouseLocation)
        }) ?? 0

        if multiMonitor && !screenArtworks.isEmpty, screenIndex < screenArtworks.count {
            let artwork = screenArtworks[screenIndex]
            let safeName = artwork.title.replacingOccurrences(of: "/", with: "-")
            let fileURL = FileManager.default.urls(for: .downloadsDirectory, in: .userDomainMask).first!
                .appendingPathComponent("\(safeName) - \(artwork.artist).jpg")
            downloadAndSave(url: artwork.imageURL, fileURL: fileURL)
        } else {
            guard let url = currentImageURL, let title = currentTitle, let artist = currentArtist else {
                showNotification(title: "Download Failed", body: "No artwork to save.")
                return
            }
            let safeName = title.replacingOccurrences(of: "/", with: "-")
            let fileURL = FileManager.default.urls(for: .downloadsDirectory, in: .userDomainMask).first!
                .appendingPathComponent("\(safeName) - \(artist).jpg")
            downloadAndSave(url: url, fileURL: fileURL)
        }
    }

    private func downloadAndSave(url: URL, fileURL: URL) {
        Task {
            do {
                let (data, _) = try await URLSession.shared.data(from: url)
                try data.write(to: fileURL)
                NSLog("[ArtFruit] Saved artwork to \(fileURL.path)")
                NSWorkspace.shared.activateFileViewerSelecting([fileURL])
            } catch {
                NSLog("[ArtFruit] Failed to download artwork: \(error.localizedDescription)")
                showNotification(title: "Download Failed", body: error.localizedDescription)
            }
        }
    }

    // MARK: - Private helpers

    private func fetchOneArtwork() async throws -> AICArtwork {
        let source: String
        if selectedSources.isEmpty {
            source = ArtFruitSources.randomElement()!
        } else {
            source = selectedSources.randomElement()!
        }
        NSLog("[ArtFruit] Fetching from source: \(source)")
        if source == "WikiArt" {
            return try await wikiArtClient.randomArtwork(styles: selectedStyles)
        } else {
            return try await apiClient.randomArtwork(styles: selectedStyles)
        }
    }

    private func scheduleTimer() {
        timer?.invalidate()
        let interval = changeIntervalMinutes * 60
        timer = Timer.scheduledTimer(
            withTimeInterval: interval,
            repeats: true
        ) { [weak self] _ in
            Task { @MainActor in
                self?.fetchAndApplyArtwork()
            }
        }
    }

    private func rescheduleTimer() {
        guard timer != nil else { return }
        scheduleTimer()
    }

    private func requestNotificationPermission() {
        UNUserNotificationCenter.current().requestAuthorization(options: [.alert, .sound]) { _, _ in }
    }

    private func showNotification(title: String, body: String) {
        let content = UNMutableNotificationContent()
        content.title = title
        content.body = body
        let request = UNNotificationRequest(
            identifier: UUID().uuidString,
            content: content,
            trigger: nil
        )
        UNUserNotificationCenter.current().add(request)
    }
}