import Foundation

struct Receiver: Codable, Identifiable {
    let id: String
    let name: String
    let type: String
    let isConnected: Bool
}
