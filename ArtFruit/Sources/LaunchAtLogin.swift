import Foundation
import ServiceManagement

/// Thin wrapper around `SMAppService.mainApp` for "Launch at Login" support.
///
/// `SMAppService` is available on macOS 13+; ArtFruit targets macOS 14, so it's
/// always present. Registering the main app adds it to the user's Login Items
/// (System Settings ▸ General ▸ Login Items) without needing a separate helper
/// bundle or a privileged login-item daemon.
enum LaunchAtLogin {
    /// Whether the app is currently registered to launch at login.
    static var isEnabled: Bool {
        SMAppService.mainApp.status == .enabled
    }

    /// Register or unregister the app as a login item.
    /// - Returns: `true` on success, `false` if the operation threw.
    @discardableResult
    static func setEnabled(_ enabled: Bool) -> Bool {
        do {
            if enabled {
                if SMAppService.mainApp.status != .enabled {
                    try SMAppService.mainApp.register()
                }
            } else {
                if SMAppService.mainApp.status == .enabled {
                    try SMAppService.mainApp.unregister()
                }
            }
            return true
        } catch {
            Log.error("LaunchAtLogin failed to set \(enabled): \(error.localizedDescription)")
            return false
        }
    }
}
