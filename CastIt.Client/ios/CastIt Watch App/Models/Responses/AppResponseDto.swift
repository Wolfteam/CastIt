import Foundation

struct AppResponseDto<T: Codable>: Codable {
    let succeed: Bool
    let result: T?
    let message: String?
    let messageId: String?
}
