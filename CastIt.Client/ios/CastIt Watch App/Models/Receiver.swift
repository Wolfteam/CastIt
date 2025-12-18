import Foundation

struct Receiver: Codable, Identifiable {
    let id: String
    let friendlyName: String
    let type: String
    let host: String
    let port: Int
    let isConnected: Bool
}
