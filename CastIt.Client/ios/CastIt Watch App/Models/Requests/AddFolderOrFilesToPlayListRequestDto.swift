import Foundation

// Mirrors ClientApp IAddFolderOrFilesToPlayListRequestDto
struct AddFolderOrFilesToPlayListRequestDto: Codable {
    let folders: [String]
    let files: [String]
    let includeSubFolders: Bool
}
