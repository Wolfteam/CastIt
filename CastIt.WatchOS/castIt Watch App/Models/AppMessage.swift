import Foundation

enum AppMessage: String, Codable {
    case info = "Info"
    case warning = "Warning"
    case error = "Error"
    case success = "Success"
    case unknown = "Unknown"
}
