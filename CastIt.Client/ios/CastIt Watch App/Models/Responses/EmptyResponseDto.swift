import Foundation

struct EmptyResponseDto: Codable {
    let succeed: Bool
    let message: String?
    let messageId: String?
}
