import SwiftUI

struct PreferencesView: View {
    @ObservedObject var viewModel: ArtFruitViewModel

    private let intervals: [Double] = [15, 30, 60, 120, 240, 480]

    @State private var selectedTab: Tab = .general
    @State private var pendingStyles: Set<String> = []
    @State private var applied = false
    @State private var pendingSources: Set<String> = []
    @State private var sourcesApplied = false

    enum Tab {
        case general, style, sources
    }

    var body: some View {
        TabView(selection: $selectedTab) {

            // MARK: General tab
            generalTab
                .tabItem { Label("General", systemImage: "gearshape") }
                .tag(Tab.general)

            // MARK: Sources tab
            sourcesTab
                .tabItem { Label("Sources", systemImage: "photo.on.rectangle.angled") }
                .tag(Tab.sources)

            // MARK: Style tab
            styleTab
                .tabItem { Label("Style", systemImage: "paintpalette") }
                .tag(Tab.style)
        }
        .frame(width: 340, height: 370)
        .onAppear {
            pendingStyles = viewModel.selectedStyles
            pendingSources = viewModel.selectedSources
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
                        let enabled = isStyleEnabled(style)
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
                            .disabled(!enabled)

                            Spacer()

                            Text(styleSourceLabel(style))
                                .font(.system(size: 10))
                                .foregroundColor(.secondary)
                                .opacity(enabled ? 1.0 : 0.4)
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

    // MARK: - Sources tab content

    private var sourcesTab: some View {
        VStack(alignment: .leading, spacing: 8) {
            Text("Fetch artwork from selected sources. Leave all unchecked for all sources.")
                .font(.caption)
                .foregroundColor(.secondary)
                .fixedSize(horizontal: false, vertical: true)

            Divider()

            VStack(alignment: .leading, spacing: 6) {
                ForEach(ArtFruitSources, id: \.self) { source in
                    Toggle(isOn: Binding(
                        get: { pendingSources.contains(source) },
                        set: { checked in
                            if checked {
                                pendingSources.insert(source)
                            } else {
                                pendingSources.remove(source)
                            }
                        }
                    )) {
                        Text(source)
                            .font(.system(size: 12))
                    }
                    .toggleStyle(.checkbox)
                }
            }
            .padding(.vertical, 4)

            Spacer()

            Divider()

            HStack {
                Button("Clear All") {
                    pendingSources.removeAll()
                }
                .buttonStyle(.plain)
                .font(.caption)
                .foregroundColor(.secondary)

                Spacer()

                Button(sourcesApplied ? "Applied ✓" : "Apply") {
                    viewModel.selectedSources = pendingSources
                    viewModel.fetchAndApplyArtwork()
                    withAnimation { sourcesApplied = true }
                    DispatchQueue.main.asyncAfter(deadline: .now() + 2) {
                        sourcesApplied = false
                    }
                }
                .buttonStyle(.borderedProminent)
                .disabled(pendingSources == viewModel.selectedSources)
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
        WikiArtStyleSlugMap[style] != nil ? "ARTIC · WikiArt" : "ARTIC"
    }

    /// Returns false when every source that supports this style is unchecked in pendingSources.
    private func isStyleEnabled(_ style: String) -> Bool {
        guard !pendingSources.isEmpty else { return true } // all sources active
        let aicActive    = pendingSources.contains("The Art Institute of Chicago")
        let wikiActive   = pendingSources.contains("WikiArt")
        let hasWikiArt   = WikiArtStyleSlugMap[style] != nil
        return hasWikiArt ? (aicActive || wikiActive) : aicActive
    }
}
