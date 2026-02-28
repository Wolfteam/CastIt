import Foundation

enum ApiError: Error {
    case invalidUrl
    case noData
    case decodingError
    case serverError(String)
    case unknown
}

class ApiService {
    private let baseUrl: URL
    
    init(baseUrl: String = "http://castit.home.internal") {
        // Remove 'api' from base url if it exists, as controllers are mapped to /[controller]
        // But the user mentioned 'PlayerController' and 'PlayListsController'
        // BaseController has [Route("[controller]")]
        // So they are /Player and /PlayLists
        self.baseUrl = URL(string: baseUrl)!
    }
    
    // MARK: - PlayerController
    
    func getPlayerStatus() async throws -> AppResponseDto<ServerPlayerStatusResponseDto> {
        return try await get(path: "/Player/Status")
    }
    
    func getAllDevices() async throws -> AppListResponseDto<Receiver> {
        return try await get(path: "/Player/AllDevices")
    }
    
    func connect(host: String, port: Int) async throws -> AppResponseDto<EmptyResponseDto> {
        let dto = ConnectRequestDto(host: host, port: port)
        return try await post(path: "/Player/Connect", body: dto)
    }
    
    func disconnect() async throws -> AppResponseDto<EmptyResponseDto> {
        return try await post(path: "/Player/Disconnect")
    }
    
    func togglePlayback() async throws -> AppResponseDto<EmptyResponseDto> {
        return try await post(path: "/Player/TogglePlayback")
    }
    
    func stop() async throws -> AppResponseDto<EmptyResponseDto> {
        return try await post(path: "/Player/Stop")
    }
    
    func setVolume(volume: Double, isMuted: Bool) async throws -> AppResponseDto<EmptyResponseDto> {
        let dto = SetVolumeRequestDto(isMuted: isMuted, volumeLevel: volume)
        return try await post(path: "/Player/SetVolume", body: dto)
    }
    
    func next() async throws -> AppResponseDto<EmptyResponseDto> {
        return try await post(path: "/Player/Next")
    }
    
    func previous() async throws -> AppResponseDto<EmptyResponseDto> {
        return try await post(path: "/Player/Previous")
    }
    
    func seek(seconds: Double) async throws -> AppResponseDto<EmptyResponseDto> {
        return try await post(path: "/Player/Seek?seconds=\(seconds)")
    }
    
    func getSettings() async throws -> AppResponseDto<ServerAppSettings> {
        return try await get(path: "/Player/Settings")
    }

    // MARK: - PlayListsController
    
    func getAllPlayLists() async throws -> AppListResponseDto<GetAllPlayListResponseDto> {
        return try await get(path: "/PlayLists")
    }
    
    func getPlayList(id: Int) async throws -> AppResponseDto<PlayListItemResponseDto> {
        return try await get(path: "/PlayLists/\(id)")
    }
    
    func addNewPlayList() async throws -> AppResponseDto<PlayListItemResponseDto> {
        return try await post(path: "/PlayLists")
    }
    
    func updatePlayList(id: Int, name: String) async throws -> AppResponseDto<PlayListItemResponseDto> {
        let dto = UpdatePlayListRequestDto(name: name)
        return try await post(path: "/PlayLists/\(id)", body: dto)
    }
    
    func deletePlayList(id: Int) async throws -> AppResponseDto<EmptyResponseDto> {
        return try await delete(path: "/PlayLists/\(id)")
    }
    
    func play(playlistId: Int, fileId: Int) async throws -> AppResponseDto<EmptyResponseDto> {
        return try await post(path: "/PlayLists/\(playlistId)/Play/\(fileId)")
    }
    
    // MARK: - Helper Methods
    
    private func get<T: Codable>(path: String) async throws -> T {
        guard let url = URL(string: path, relativeTo: baseUrl) else {
            throw ApiError.invalidUrl
        }
        
        let (data, response) = try await URLSession.shared.data(from: url)
        
        guard let httpResponse = response as? HTTPURLResponse, (200...299).contains(httpResponse.statusCode) else {
            throw ApiError.serverError("HTTP status code: \((response as? HTTPURLResponse)?.statusCode ?? -1)")
        }
        
        do {
            let decoder = JSONDecoder()
            return try decoder.decode(T.self, from: data)
        } catch {
            print("Decoding error for \(path): \(error)")
            throw ApiError.decodingError
        }
    }
    
    private func post<T: Codable, B: Codable>(path: String, body: B? = nil) async throws -> T {
        guard let url = URL(string: path, relativeTo: baseUrl) else {
            throw ApiError.invalidUrl
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        if let body = body {
            request.httpBody = try JSONEncoder().encode(body)
        }
        
        let (data, response) = try await URLSession.shared.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse, (200...299).contains(httpResponse.statusCode) else {
            throw ApiError.serverError("HTTP status code: \((response as? HTTPURLResponse)?.statusCode ?? -1)")
        }
        
        do {
            let decoder = JSONDecoder()
            return try decoder.decode(T.self, from: data)
        } catch {
            print("Decoding error for \(path): \(error)")
            throw ApiError.decodingError
        }
    }
    
    private func post<T: Codable>(path: String) async throws -> T {
        let body: String? = nil
        return try await post(path: path, body: body)
    }
    
    private func delete<T: Codable>(path: String) async throws -> T {
        guard let url = URL(string: path, relativeTo: baseUrl) else {
            throw ApiError.invalidUrl
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "DELETE"
        
        let (data, response) = try await URLSession.shared.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse, (200...299).contains(httpResponse.statusCode) else {
            throw ApiError.serverError("HTTP status code: \((response as? HTTPURLResponse)?.statusCode ?? -1)")
        }
        
        do {
            let decoder = JSONDecoder()
            return try decoder.decode(T.self, from: data)
        } catch {
            print("Decoding error for \(path): \(error)")
            throw ApiError.decodingError
        }
    }
}
