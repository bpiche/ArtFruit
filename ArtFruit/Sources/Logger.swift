import Foundation
import os

/// Lightweight logging shim for ArtFruit.
///
/// In DEBUG builds, messages are written through `os_log` (visible in Console.app
/// and Xcode). In release builds the debug/info calls compile down to no-ops, so we
/// don't spam the unified log / Console with our diagnostic chatter in shipped binaries.
enum Log {
    private static let logger = os.Logger(subsystem: "io.github.bpiche.ArtFruit", category: "general")

    static func debug(_ message: @autoclosure () -> String) {
        #if DEBUG
        let text = message()
        logger.debug("\(text, privacy: .public)")
        #endif
    }

    static func info(_ message: @autoclosure () -> String) {
        #if DEBUG
        let text = message()
        logger.info("\(text, privacy: .public)")
        #endif
    }

    /// Errors are always logged (even in release) since they're rare and useful
    /// for diagnosing problems in the field.
    static func error(_ message: @autoclosure () -> String) {
        let text = message()
        logger.error("\(text, privacy: .public)")
    }
}
