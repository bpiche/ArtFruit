import SwiftUI

struct PreferencesView: View {
    @ObservedObject var viewModel: ArtFruitViewModel

    private let intervals: [Double] = [15, 30, 60, 120, 240, 480]

    @State private var selectedTab: Tab = .general
    @State private var pendingStyles: Set<String> = []
    @State private var stylesApplied = false
    @State private var pendingArtists: Set<String> = []
    @State private var artistsApplied = false

    // Track which groups are expanded (collapsed by default)
    @State private var expandedStyleGroups: Set<String> = []
    @State private var expandedArtistGroups: Set<String> = []

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
        .frame(minWidth: 340, idealWidth: 380, maxWidth: .infinity,
               minHeight: 370, idealHeight: 440, maxHeight: .infinity)
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
                VStack(alignment: .leading, spacing: 4) {
                    ForEach(WikiArtStyleGroups, id: \.name) { group in
                        styleGroupRow(group)
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

                Button(stylesApplied ? "Applied ✓" : "Apply") {
                    viewModel.selectedStyles = pendingStyles
                    viewModel.fetchAndApplyArtwork()
                    withAnimation { stylesApplied = true }
                    DispatchQueue.main.asyncAfter(deadline: .now() + 2) {
                        stylesApplied = false
                    }
                }
                .buttonStyle(.borderedProminent)
                .disabled(pendingStyles == viewModel.selectedStyles)
            }
        }
        .padding(20)
    }

    @ViewBuilder
    private func styleGroupRow(_ group: WikiArtStyleGroup) -> some View {
        let allNames = group.styles.map(\.name)
        let allSelected = allNames.allSatisfy { pendingStyles.contains($0) }
        let someSelected = allNames.contains { pendingStyles.contains($0) }
        let isExpanded = expandedStyleGroups.contains(group.name)

        VStack(alignment: .leading, spacing: 0) {
            // Group header row
            HStack(spacing: 4) {
                // Expand/collapse arrow
                Button(action: {
                    if isExpanded {
                        expandedStyleGroups.remove(group.name)
                    } else {
                        expandedStyleGroups.insert(group.name)
                    }
                }) {
                    Image(systemName: isExpanded ? "chevron.down" : "chevron.right")
                        .font(.system(size: 9, weight: .semibold))
                        .frame(width: 14, height: 14)
                        .foregroundColor(.secondary)
                }
                .buttonStyle(.plain)

                // Group-level checkbox (tri-state: all / some / none)
                Button(action: {
                    if allSelected {
                        allNames.forEach { pendingStyles.remove($0) }
                    } else {
                        allNames.forEach { pendingStyles.insert($0) }
                    }
                }) {
                    Image(systemName: allSelected ? "checkmark.square.fill"
                                    : someSelected ? "minus.square.fill"
                                    : "square")
                        .font(.system(size: 13))
                        .foregroundColor(allSelected || someSelected ? .accentColor : .secondary)
                }
                .buttonStyle(.plain)

                Text(group.name)
                    .font(.system(size: 12, weight: .semibold))

                Spacer()
            }
            .padding(.vertical, 2)
            .contentShape(Rectangle())

            // Expandable style list
            if isExpanded {
                VStack(alignment: .leading, spacing: 4) {
                    ForEach(group.styles, id: \.name) { style in
                        HStack(spacing: 0) {
                            Toggle(isOn: Binding(
                                get: { pendingStyles.contains(style.name) },
                                set: { checked in
                                    if checked { pendingStyles.insert(style.name) }
                                    else { pendingStyles.remove(style.name) }
                                }
                            )) {
                                Text(style.name)
                                    .font(.system(size: 12))
                            }
                            .toggleStyle(.checkbox)
                            Spacer()
                        }
                    }
                }
                .padding(.leading, 28)
            }
        }
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
                VStack(alignment: .leading, spacing: 4) {
                    ForEach(WikiArtArtistGroups, id: \.name) { group in
                        artistGroupRow(group)
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

    @ViewBuilder
    private func artistGroupRow(_ group: WikiArtArtistGroup) -> some View {
        let allNames = group.artists.map(\.name)
        let allSelected = allNames.allSatisfy { pendingArtists.contains($0) }
        let someSelected = allNames.contains { pendingArtists.contains($0) }
        let isExpanded = expandedArtistGroups.contains(group.name)

        VStack(alignment: .leading, spacing: 0) {
            HStack(spacing: 4) {
                Button(action: {
                    if isExpanded {
                        expandedArtistGroups.remove(group.name)
                    } else {
                        expandedArtistGroups.insert(group.name)
                    }
                }) {
                    Image(systemName: isExpanded ? "chevron.down" : "chevron.right")
                        .font(.system(size: 9, weight: .semibold))
                        .frame(width: 14, height: 14)
                        .foregroundColor(.secondary)
                }
                .buttonStyle(.plain)

                Button(action: {
                    if allSelected {
                        allNames.forEach { pendingArtists.remove($0) }
                    } else {
                        allNames.forEach { pendingArtists.insert($0) }
                    }
                }) {
                    Image(systemName: allSelected ? "checkmark.square.fill"
                                    : someSelected ? "minus.square.fill"
                                    : "square")
                        .font(.system(size: 13))
                        .foregroundColor(allSelected || someSelected ? .accentColor : .secondary)
                }
                .buttonStyle(.plain)

                Text(group.name)
                    .font(.system(size: 12, weight: .semibold))

                Spacer()
            }
            .padding(.vertical, 2)
            .contentShape(Rectangle())

            if isExpanded {
                VStack(alignment: .leading, spacing: 4) {
                    ForEach(group.artists, id: \.name) { artist in
                        HStack(spacing: 0) {
                            Toggle(isOn: Binding(
                                get: { pendingArtists.contains(artist.name) },
                                set: { checked in
                                    if checked { pendingArtists.insert(artist.name) }
                                    else { pendingArtists.remove(artist.name) }
                                }
                            )) {
                                Text(artist.name)
                                    .font(.system(size: 12))
                            }
                            .toggleStyle(.checkbox)
                            Spacer()
                        }
                    }
                }
                .padding(.leading, 28)
            }
        }
    }

    // MARK: - Helpers

    private func label(for minutes: Double) -> String {
        if minutes < 60 { return "\(Int(minutes)) minutes" }
        let hours = Int(minutes / 60)
        return hours == 1 ? "1 hour" : "\(hours) hours"
    }
}
