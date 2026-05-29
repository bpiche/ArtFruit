import AppKit

@MainActor
final class WallpaperService {
    private let fileManager = FileManager.default
    private let cacheDir: URL
    private var lastWallpaperURLs: [URL] = []

    init() {
        cacheDir = fileManager.temporaryDirectory
            .appendingPathComponent("io.github.bpiche.ArtFruit", isDirectory: true)
        try? fileManager.createDirectory(at: cacheDir, withIntermediateDirectories: true)
        NSLog("[ArtFruit] Cache dir: \(cacheDir.path)")
    }

    /// Apply an artwork to the given screens (defaults to all screens when `screens` is nil).
    func apply(imageURL: URL, title: String, artist: String, showTitle: Bool = true, showArtist: Bool = true, screens: [NSScreen]? = nil) async throws {
        NSLog("[ArtFruit] Downloading image from \(imageURL)...")
        let (data, _) = try await URLSession.shared.data(from: imageURL)
        NSLog("[ArtFruit] Downloaded \(data.count) bytes.")

        guard let sourceImage = NSImage(data: data) else {
            throw WallpaperError.invalidImage
        }
        NSLog("[ArtFruit] Source image size: \(sourceImage.size)")

        let screens = screens ?? NSScreen.screens
        NSLog("[ArtFruit] Applying to \(screens.count) screen(s)...")

        let timestamp = Int(Date().timeIntervalSince1970)
        var newURLs: [URL] = []

        for (i, screen) in screens.enumerated() {
            // Use backing pixel size so retina screens get full-res images
            let screenRect = screen.frame
            let scale = screen.backingScaleFactor
            let pixelWidth = Int(screenRect.width * scale)
            let pixelHeight = Int(screenRect.height * scale)
            NSLog("[ArtFruit] Screen \(i): \(pixelWidth)×\(pixelHeight)px (scale \(scale)x)")

            let fitted = try fitImage(sourceImage, toWidth: pixelWidth, height: pixelHeight,
                                      title: title, artist: artist,
                                      showTitle: showTitle, showArtist: showArtist)
            let url = cacheDir.appendingPathComponent("artfruit_\(timestamp)_screen\(i).jpg")
            try saveJPEG(fitted, to: url)
            newURLs.append(url)

            do {
                // Image is already composited to exact pixel dimensions — no extra scaling needed
                try NSWorkspace.shared.setDesktopImageURL(url, for: screen, options: [:])
                NSLog("[ArtFruit] Screen \(i): applied \(pixelWidth)×\(pixelHeight) wallpaper")
            } catch {
                NSLog("[ArtFruit] Screen \(i): FAILED: \(error.localizedDescription)")
                throw error
            }
        }

        // Clean up previous wallpaper files
        for old in lastWallpaperURLs {
            try? fileManager.removeItem(at: old)
        }
        lastWallpaperURLs = newURLs
        NSLog("[ArtFruit] All screens updated.")
    }

    /// Scale-to-fill (cover): artwork fills the canvas, cropping if needed.
    /// Adds a blurred/darkened background so portrait art on landscape screens looks good.
    private func fitImage(_ source: NSImage, toWidth width: Int, height: Int,
                          title: String, artist: String,
                          showTitle: Bool, showArtist: Bool) throws -> NSImage {
        let canvasSize = NSSize(width: width, height: height)
        let srcSize = source.size
        guard srcSize.width > 0, srcSize.height > 0 else { throw WallpaperError.invalidImage }

        let result = NSImage(size: canvasSize)
        guard let rep = NSBitmapImageRep(
            bitmapDataPlanes: nil,
            pixelsWide: width,
            pixelsHigh: height,
            bitsPerSample: 8,
            samplesPerPixel: 4,
            hasAlpha: true,
            isPlanar: false,
            colorSpaceName: .deviceRGB,
            bytesPerRow: 0,
            bitsPerPixel: 0
        ) else { throw WallpaperError.encodeFailure }
        result.addRepresentation(rep)

        result.lockFocus()
        defer { result.unlockFocus() }

        let canvas = NSRect(origin: .zero, size: canvasSize)

        // --- Background: fill with black, then draw a blurred/dimmed version of the
        //     artwork stretched to cover (so letterbox bars aren't just black)
        NSColor.black.setFill()
        canvas.fill()

        // Draw blurred background (stretched to fill, then dimmed)
        let bgScale = max(CGFloat(width) / srcSize.width, CGFloat(height) / srcSize.height)
        let bgW = srcSize.width * bgScale
        let bgH = srcSize.height * bgScale
        let bgRect = NSRect(
            x: (CGFloat(width) - bgW) / 2,
            y: (CGFloat(height) - bgH) / 2,
            width: bgW,
            height: bgH
        )
        source.draw(in: bgRect, from: .zero, operation: .copy, fraction: 0.35)

        // --- Foreground: scale-to-fit (letterbox) centered
        let fitScale = min(CGFloat(width) / srcSize.width, CGFloat(height) / srcSize.height)
        let fgW = srcSize.width * fitScale
        let fgH = srcSize.height * fitScale
        let fgRect = NSRect(
            x: (CGFloat(width) - fgW) / 2,
            y: (CGFloat(height) - fgH) / 2,
            width: fgW,
            height: fgH
        )
        source.draw(in: fgRect, from: .zero, operation: .sourceOver, fraction: 1.0)

        // --- Title/artist overlay (bottom-right of the artwork rect)
        var textParts: [String] = []
        if showTitle { textParts.append(title) }
        if showArtist { textParts.append(artist) }
        let text = textParts.joined(separator: "  ")

        if !text.isEmpty {
            let fontSize: CGFloat = max(24, min(44, CGFloat(width) / 60))
            let textAttrs: [NSAttributedString.Key: Any] = [
                .font: NSFont.systemFont(ofSize: fontSize, weight: .medium),
                .foregroundColor: NSColor.white
            ]
            let shadowAttrs: [NSAttributedString.Key: Any] = [
                .font: NSFont.systemFont(ofSize: fontSize, weight: .medium),
                .foregroundColor: NSColor.black.withAlphaComponent(0.75)
            ]
            let measured = NSAttributedString(string: text, attributes: textAttrs).size()
            let padding: CGFloat = max(16, CGFloat(width) / 80)
            let tx = fgRect.maxX - measured.width - padding
            let ty = fgRect.minY + padding

            NSAttributedString(string: text, attributes: shadowAttrs)
                .draw(at: NSPoint(x: tx + 1, y: ty - 1))
            NSAttributedString(string: text, attributes: textAttrs)
                .draw(at: NSPoint(x: tx, y: ty))
        }

        return result
    }

    private func saveJPEG(_ image: NSImage, to url: URL) throws {
        guard let tiffData = image.tiffRepresentation,
              let bitmap = NSBitmapImageRep(data: tiffData),
              let jpegData = bitmap.representation(using: .jpeg, properties: [.compressionFactor: 0.92])
        else { throw WallpaperError.encodeFailure }
        try jpegData.write(to: url)
        NSLog("[ArtFruit] Saved \(url.lastPathComponent) (\(jpegData.count) bytes)")
    }

    enum WallpaperError: LocalizedError {
        case invalidImage, encodeFailure
        var errorDescription: String? {
            switch self {
            case .invalidImage: return "Downloaded data is not a valid image."
            case .encodeFailure: return "Could not encode wallpaper image."
            }
        }
    }
}
