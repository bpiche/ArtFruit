// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "ArtFruit",
    platforms: [
        .macOS(.v14)
    ],
    products: [
        .executable(name: "ArtFruit", targets: ["ArtFruit"])
    ],
    targets: [
        .executableTarget(
            name: "ArtFruit",
            path: "Sources"
        )
    ]
)
