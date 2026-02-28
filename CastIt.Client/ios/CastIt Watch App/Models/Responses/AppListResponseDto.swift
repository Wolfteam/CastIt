import Foundation

struct AppListResponseDto<T: Codable>: Codable {
    let succeed: Bool
    let result: [T]
    let message: String?
    let messageId: String?
}
