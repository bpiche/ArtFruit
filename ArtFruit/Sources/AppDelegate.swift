import AppKit
import SwiftUI
import UserNotifications

@MainActor
final class AppDelegate: NSObject, NSApplicationDelegate, UNUserNotificationCenterDelegate {
    private var statusItem: NSStatusItem!
    private var viewModel = ArtFruitViewModel()
    private var preferencesWindow: NSWindow?

    func applicationDidFinishLaunching(_ notification: Notification) {
        NSApp.setActivationPolicy(.accessory) // no Dock icon

        // Register as delegate BEFORE requesting permission so notifications
        // are delivered reliably even while the app is the active process.
        UNUserNotificationCenter.current().delegate = self

        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.squareLength)
        if let button = statusItem.button {
            button.image = NSImage(systemSymbolName: "paintpalette", accessibilityDescription: "ArtFruit")
        }

        buildMenu()
        viewModel.startRotation()
    }

    private func buildMenu() {
        let menu = NSMenu()

        let titleItem = NSMenuItem(title: "ArtFruit", action: nil, keyEquivalent: "")
        titleItem.isEnabled = false
        menu.addItem(titleItem)
        menu.addItem(.separator())

        menu.addItem(NSMenuItem(title: "Next Artwork", action: #selector(nextArtwork), keyEquivalent: "n"))

        let pauseItem = NSMenuItem(title: "Pause", action: #selector(togglePause), keyEquivalent: "p")
        menu.addItem(pauseItem)

        menu.addItem(.separator())
        menu.addItem(NSMenuItem(title: "Save Artwork…", action: #selector(saveArtwork), keyEquivalent: "s"))

        menu.addItem(.separator())
        menu.addItem(NSMenuItem(title: "Preferences…", action: #selector(openPreferences), keyEquivalent: ","))
        menu.addItem(.separator())
        menu.addItem(NSMenuItem(title: "Quit ArtFruit", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q"))

        statusItem.menu = menu
    }

    /// Ensures banners appear even when ArtFruit is the foreground app.
    nonisolated func userNotificationCenter(
        _ center: UNUserNotificationCenter,
        willPresent notification: UNNotification,
        withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void
    ) {
        completionHandler([.banner, .sound])
    }

    @objc private func nextArtwork() {
        NSLog("[ArtFruit] nextArtwork() selector fired")
        Task { @MainActor in
            self.viewModel.fetchAndApplyArtwork()
        }
    }

    @objc private func togglePause() {
        NSLog("[ArtFruit] togglePause() selector fired")
        Task { @MainActor in
            self.viewModel.isPaused.toggle()
            if let menu = self.statusItem.menu,
               let item = menu.items.first(where: { $0.action == #selector(self.togglePause) }) {
                item.title = self.viewModel.isPaused ? "Resume" : "Pause"
            }
        }
    }

    @objc private func saveArtwork() {
        NSLog("[ArtFruit] saveArtwork() selector fired")
        viewModel.saveCurrentArtwork()
    }

    @objc private func openPreferences() {
        if preferencesWindow == nil {
            let view = PreferencesView(viewModel: viewModel)
            let hosting = NSHostingController(rootView: view)
            let window = NSWindow(contentViewController: hosting)
            window.title = "ArtFruit Preferences"
            window.styleMask = [.titled, .closable]
            // Center the title by balancing the traffic-light buttons with an
            // equivalent invisible spacer on the right side of the title bar.
            window.titlebarAppearsTransparent = false
            window.titleVisibility = .visible
            if let titlebarView = window.standardWindowButton(.closeButton)?.superview?.superview {
                titlebarView.wantsLayer = true
            }
            // Use a toolbar so macOS has room to truly center the title string
            let toolbar = NSToolbar(identifier: "PreferencesToolbar")
            toolbar.showsBaselineSeparator = false
            window.toolbar = toolbar
            window.setContentSize(NSSize(width: 340, height: 320))
            window.center()
            preferencesWindow = window
        }
        NSApp.activate(ignoringOtherApps: true)
        preferencesWindow?.makeKeyAndOrderFront(nil)
    }
}
