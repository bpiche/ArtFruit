import SwiftUI

struct PreferencesView: View {
    @ObservedObject var viewModel: ArtFruitViewModel

    private let intervals: [Double] = [15, 30, 60, 120, 240, 480]

    @State private var selectedTab: Tab = .general
    @State private var pendingStyles: Set<String> = []
    @State private var applied = false
    @State private var pendingArtists: Set<String> = []
    @State private var artistsApplied = false

    enum Tab {
        case general, style, artists
    }

    var body: some View {
        TabView(selection: $selectedTab) {

            // MARK: General tab
            generalTab
                .tabItem { Label("General", systemImage: "gearshape") }
                .tag(Tab.general)

            // MARK: Style tab
            styleTab
                .tabItem { Label("Style", systemImage: "paintpalette") }
                .tag(Tab.style)

            // MARK: Artists tab
            artistsTab
                .tabItem { Label("Artists", systemImage: "person.2") }
                .tag(Tab.artists)
        }
        .frame(minWidth: 340, idealWidth: 340, maxWidth: .infinity,
               minHeight: 370, idealHeight: 370, maxHeight: .infinity)
        .onAppear {
            pendingStyles = viewModel.selectedStyles
            pendingArtists = viewModel.selectedArtists
        }
    }

    // MARK: - General tab content

    private var generalTab: some View {
            VStack(alignment: .leading, spacing: 16) {
                Picker("Change artwork every:", selection: $viewModel.changeIntervalMinutes) {
                    ForEach(intervals, id: \.self) { minutes in
                        Text(label(for: minutes)).tag(minutes)
                    }
                }
                .pickerStyle(.menu)

                Toggle("Pause artwork rotation", isOn: $viewModel.isPaused)

                Toggle("Display artwork on multiple monitors", isOn: $viewModel.multiMonitor)

                Toggle("Display artwork title", isOn: $viewModel.showTitle)

                Toggle("Display artist name", isOn: $viewModel.showArtist)

            if let title = viewModel.currentTitle {
                Divider()
                VStack(alignment: .leading, spacing: 4) {
                    Text("Current artwork:")
                        .font(.caption)
                        .foregroundColor(.secondary)
                    Text(title)
                        .font(.body)
                        .lineLimit(2)
                    if let artist = viewModel.currentArtist {
                        Text(artist)
                            .font(.caption)
                            .foregroundColor(.secondary)
                    }
                }
            }

            Spacer()
        }
        .padding(20)
    }

    // MARK: - Style tab content

    private var styleTab: some View {
        VStack(alignment: .leading, spacing: 8) {
            Text("Filter artwork by style. Leave all unchecked for any style.")
                .font(.caption)
                .foregroundColor(.secondary)
                .fixedSize(horizontal: false, vertical: true)

            Divider()

            ScrollView {
                VStack(alignment: .leading, spacing: 6) {
                    ForEach(AICAvailableStyles, id: \.self) { style in
                        HStack(spacing: 0) {
                            Toggle(isOn: Binding(
                                get: { pendingStyles.contains(style) },
                                set: { checked in
                                    if checked {
                                        pendingStyles.insert(style)
                                    } else {
                                        pendingStyles.remove(style)
                                    }
                                }
                            )) {
                                Text(style)
                                    .font(.system(size: 12))
                            }
                            .toggleStyle(.checkbox)

                            Spacer()

                            Text(styleSourceLabel(style))
                                .font(.system(size: 10))
                                .foregroundColor(.secondary)
                        }
                    }
                }
                .padding(.vertical, 4)
            }

            Divider()

            HStack {
                Button("Clear All") {
                    pendingStyles.removeAll()
                }
                .buttonStyle(.plain)
                .font(.caption)
                .foregroundColor(.secondary)

                Spacer()

                Button(applied ? "Applied ✓" : "Apply") {
                    viewModel.selectedStyles = pendingStyles
                    viewModel.fetchAndApplyArtwork()
                    withAnimation { applied = true }
                    DispatchQueue.main.asyncAfter(deadline: .now() + 2) {
                        applied = false
                    }
                }
                .buttonStyle(.borderedProminent)
                .disabled(pendingStyles == viewModel.selectedStyles)
            }
        }
        .padding(20)
    }

    // MARK: - Artists tab content

    private var artistsTab: some View {
        VStack(alignment: .leading, spacing: 8) {
            Text("Filter artwork by artist. Leave all unchecked for any artist.")
                .font(.caption)
                .foregroundColor(.secondary)
                .fixedSize(horizontal: false, vertical: true)

            Divider()

            ScrollView {
                VStack(alignment: .leading, spacing: 6) {
                    ForEach(AICAvailableArtists, id: \.self) { artist in
                        HStack(spacing: 0) {
                            Toggle(isOn: Binding(
                                get: { pendingArtists.contains(artist) },
                                set: { checked in
                                    if checked {
                                        pendingArtists.insert(artist)
                                    } else {
                                        pendingArtists.remove(artist)
                                    }
                                }
                            )) {
                                Text(artist)
                                    .font(.system(size: 12))
                            }
                            .toggleStyle(.checkbox)

                            Spacer()

                            Text(artistSourceLabel(artist))
                                .font(.system(size: 10))
                                .foregroundColor(.secondary)
                        }
                    }
                }
                .padding(.vertical, 4)
            }

            Divider()

            HStack {
                Button("Clear All") {
                    pendingArtists.removeAll()
                }
                .buttonStyle(.plain)
                .font(.caption)
                .foregroundColor(.secondary)

                Spacer()

                Button(artistsApplied ? "Applied ✓" : "Apply") {
                    viewModel.selectedArtists = pendingArtists
                    viewModel.fetchAndApplyArtwork()
                    withAnimation { artistsApplied = true }
                    DispatchQueue.main.asyncAfter(deadline: .now() + 2) {
                        artistsApplied = false
                    }
                }
                .buttonStyle(.borderedProminent)
                .disabled(pendingArtists == viewModel.selectedArtists)
            }
        }
        .padding(20)
    }

    // MARK: - Helpers

    private func label(for minutes: Double) -> String {
        if minutes < 60 { return "\(Int(minutes)) minutes" }
        let hours = Int(minutes / 60)
        return hours == 1 ? "1 hour" : "\(hours) hours"
    }

    /// Short badge shown to the right of each style row.
    private func styleSourceLabel(_ style: String) -> String {
        WikiArtStyleSlugMap[style] != nil ? "WikiArt" : ""
    }

    /// Short badge shown to the right of each artist row.
    private func artistSourceLabel(_ artist: String) -> String {
        WikiArtArtistSlugMap[artist] != nil ? "WikiArt" : ""
    }
}
