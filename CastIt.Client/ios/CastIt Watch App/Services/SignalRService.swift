import Foundation
import Combine
import SignalRClient

enum SignalRError: Error {
    case unknown
}

class SignalRService: HubConnectionDelegate {
    let onClientConnected = PassthroughSubject<Void, Never>()
    let onClientDisconnected = PassthroughSubject<Void, Never>()
    
    // Player
    let onPlayerStatusChanged = PassthroughSubject<ServerPlayerStatusResponseDto, Never>()
    let onPlayerSettingsChanged = PassthroughSubject<ServerAppSettings, Never>()
    let onCastDeviceSet = PassthroughSubject<Receiver, Never>()
    let onCastDevicesChanged = PassthroughSubject<[Receiver], Never>()
    let onCastDeviceDisconnected = PassthroughSubject<Void, Never>()
    let onStoppedPlayBack = PassthroughSubject<Void, Never>()

    // Playlists
    let onPlayListsLoaded = PassthroughSubject<[GetAllPlayListResponseDto], Never>()
    let onPlayListAdded = PassthroughSubject<GetAllPlayListResponseDto, Never>()
    let onPlayListChanged = PassthroughSubject<GetAllPlayListResponseDto, Never>()
    let onPlayListsChanged = PassthroughSubject<[GetAllPlayListResponseDto], Never>()
    let onPlayListDeleted = PassthroughSubject<Int, Never>()
    let onPlayListBusy = PassthroughSubject<PlayListBusy, Never>()

    // Files
    let onFileAdded = PassthroughSubject<FileItemResponseDto, Never>()
    let onFileChanged = PassthroughSubject<FileItemResponseDto, Never>()
    let onFilesChanged = PassthroughSubject<[FileItemResponseDto], Never>()
    let onFileDeleted = PassthroughSubject<FileDeleted, Never>()
    let onFileLoading = PassthroughSubject<FileItemResponseDto, Never>()
    let onFileLoaded = PassthroughSubject<FileItemResponseDto, Never>()
    let onFileEndReached = PassthroughSubject<FileItemResponseDto, Never>()

    // General
    let onServerMessage = PassthroughSubject<AppMessage, Never>()

    private var connection: HubConnection
    
    init() {
        let url = URL(string: "http://castit.home.internal/castithub")!
        self.connection = HubConnectionBuilder(url: url)
            .withLogging(minLogLevel: .debug)
            .withAutoReconnect()
            .build()
        self.connection.delegate = self
        
        // Player related event handlers
        connection.on(method: "PlayerStatusChanged", callback: { (serverPlayerStatus: ServerPlayerStatusResponseDto) in
            self.onPlayerStatusChanged.send(serverPlayerStatus)
        })

        connection.on(method: "PlayerSettingsChanged", callback: { (settings: ServerAppSettings) in
            self.onPlayerSettingsChanged.send(settings)
        })

        connection.on(method: "CastDeviceSet", callback: { (device: Receiver) in
            self.onCastDeviceSet.send(device)
        })

        connection.on(method: "CastDevicesChanged", callback: { (devices: [Receiver]) in
            self.onCastDevicesChanged.send(devices)
        })

        connection.on(method: "CastDeviceDisconnected", callback: {
            self.onCastDeviceDisconnected.send()
        })

        connection.on(method: "StoppedPlayBack", callback: {
            self.onStoppedPlayBack.send()
        })
        
        // Playlists related event handlers
        connection.on(method: "SendPlayLists", callback: { (playLists: [GetAllPlayListResponseDto]) in
            self.onPlayListsLoaded.send(playLists)
        })

        connection.on(method: "PlayListsChanged", callback: { (playLists: [GetAllPlayListResponseDto]) in
            self.onPlayListsChanged.send(playLists)
        })

        connection.on(method: "PlayListAdded", callback: { (newPlayList: GetAllPlayListResponseDto) in
            self.onPlayListAdded.send(newPlayList)
        })

        connection.on(method: "PlayListChanged", callback: { (updatedPlayList: GetAllPlayListResponseDto) in
            self.onPlayListChanged.send(updatedPlayList)
        })

        connection.on(method: "PlayListDeleted", callback: { (deletedPlayListId: Int) in
            self.onPlayListDeleted.send(deletedPlayListId)
        })
        
        connection.on(method: "PlayListIsBusy", callback: { (playListBusy: PlayListBusy) in
            self.onPlayListBusy.send(playListBusy)
        })

        // File related event handlers
        connection.on(method: "FileAdded", callback: { (file: FileItemResponseDto) in
            self.onFileAdded.send(file)
        })

        connection.on(method: "FileChanged", callback: { (file: FileItemResponseDto) in
            self.onFileChanged.send(file)
        })
        
        connection.on(method: "FilesChanged", callback: { (files: [FileItemResponseDto]) in
            self.onFilesChanged.send(files)
        })

        connection.on(method: "FileDeleted", callback: { (fileDeleted: FileDeleted) in
            self.onFileDeleted.send(fileDeleted)
        })

        connection.on(method: "FileLoading", callback: { (file: FileItemResponseDto) in
            self.onFileLoading.send(file)
        })

        connection.on(method: "FileLoaded", callback: { (file: FileItemResponseDto) in
            self.onFileLoaded.send(file)
        })

        connection.on(method: "FileEndReached", callback: { (file: FileItemResponseDto) in
            self.onFileEndReached.send(file)
        })

        // General server messages
        connection.on(method: "ServerMessage", callback: { (message: AppMessage) in
            self.onServerMessage.send(message)
        })
    }

    func connect() {
        connection.start()
    }

    func disconnect() {
        connection.stop()
    }

    // Player methods
    func togglePlayBack() {
        connection.invoke(method: "TogglePlayBack") { error in }
    }

    func stopPlayBack() {
        connection.invoke(method: "StopPlayBack") { error in
            if let error = error {
                print("Error stopping playback: \(error)")
            }
        }
    }
    
    func goTo(next: Bool, previous: Bool) {
        connection.invoke(method: "GoTo", next, previous) { error in }
    }
    
    func play(playListId: Int, fileId: Int, force: Bool, fileOptionsChanged: Bool = false) {
        let request = PlayFileRequestDto(id: fileId, playListId: playListId, force: force, fileOptionsChanged: fileOptionsChanged)
        connection.invoke(method: "Play", request) { error in }
    }

    // Playlist methods
    func getPlaylists() {
        connection.invoke(method: "GetAllPlayLists", resultType: [GetAllPlayListResponseDto].self) { result, error in
            if let playlists = result {
                DispatchQueue.main.async {
                    self.onPlayListsLoaded.send(playlists)
                }
            } else if let error = error {
                print("Error getting playlists: \(error)")
            }
        }
    }
    
    func getPlayList(playlistId: Int) async throws -> PlayListItemResponseDto {
        return try await withCheckedThrowingContinuation { continuation in
            connection.invoke(method: "GetPlayList", playlistId, resultType: PlayListItemResponseDto.self) { result, error in
                if let playlist = result {
                    continuation.resume(returning: playlist)
                } else if let error = error {
                    continuation.resume(throwing: error)
                } else {
                    continuation.resume(throwing: SignalRError.unknown)
                }
            }
        }
    }

    func addNewPlayList() async throws -> PlayListItemResponseDto {
        return try await withCheckedThrowingContinuation { continuation in
            connection.invoke(method: "AddNewPlayList", resultType: PlayListItemResponseDto.self) { result, error in
                if let newPlaylist = result {
                    continuation.resume(returning: newPlaylist)
                } else if let error = error {
                    continuation.resume(throwing: error)
                } else {
                    continuation.resume(throwing: SignalRError.unknown)
                }
            }
        }
    }

    func deletePlayList(id: Int) {
        connection.invoke(method: "DeletePlayList", id) { error in
            if let error = error {
                print("Error deleting playlist \(id): \(error)")
            }
        }
    }

    // MARK: - Player extended methods (parity with ClientApp)
    func gotoSeconds(_ seconds: Double) {
        connection.invoke(method: "GoToSeconds", seconds) { _ in }
    }

    func gotoPosition(_ position: Double) {
        connection.invoke(method: "GoToPosition", position) { _ in }
    }

    func skipSeconds(_ seconds: Double) {
        connection.invoke(method: "SkipSeconds", seconds) { _ in }
    }

    func setVolume(level: Double, isMuted: Bool) {
        let request = SetVolumeRequestDto(isMuted: isMuted, volumeLevel: level)
        connection.invoke(method: "SetVolume", request) { _ in }
    }

    func updateSettings(_ settings: ServerAppSettings) {
        connection.invoke(method: "UpdateSettings", settings) { _ in }
    }

    func connectToCastDevice(id: String) {
        connection.invoke(method: "ConnectToCastDevice", id) { _ in }
    }

    func refreshCastDevices() {
        connection.invoke(method: "RefreshCastDevices") { _ in }
    }

    func setFileSubtitlesFromPath(_ path: String) {
        connection.invoke(method: "SetFileSubtitlesFromPath", path) { _ in }
    }

    // MARK: - Playlist methods (parity with ClientApp)
    func updatePlayList(id: Int, name: String) {
        let request = UpdatePlayListRequestDto(name: name)
        connection.invoke(method: "UpdatePlayList", id, request) { _ in }
    }

    func updatePlayListPosition(playListId: Int, newIndex: Int) {
        connection.invoke(method: "UpdatePlayListPosition", playListId, newIndex) { _ in }
    }

    func setPlayListOptions(playListId: Int, loop: Bool, shuffle: Bool) {
        let request = SetPlayListOptionsRequestDto(loop: loop, shuffle: shuffle)
        connection.invoke(method: "SetPlayListOptions", playListId, request) { _ in }
    }

    func deleteAllPlayLists(exceptId: Int) {
        connection.invoke(method: "DeleteAllPlayLists", exceptId) { _ in }
    }

    func removeFiles(playListId: Int, ids: [Int]) {
        connection.invoke(method: "RemoveFiles", playListId, ids) { _ in }
    }

    func removeFilesThatStartsWith(playListId: Int, path: String) {
        connection.invoke(method: "RemoveFilesThatStartsWith", playListId, path) { _ in }
    }

    func removeAllMissingFiles(playListId: Int) {
        connection.invoke(method: "RemoveAllMissingFiles", playListId) { _ in }
    }

    func addFolders(playListId: Int, includeSubFolders: Bool, folders: [String]) {
        let request = AddFolderOrFilesToPlayListRequestDto(folders: folders, files: [], includeSubFolders: includeSubFolders)
        connection.invoke(method: "AddFolders", playListId, request) { _ in }
    }

    func addFiles(playListId: Int, files: [String]) {
        let request = AddFolderOrFilesToPlayListRequestDto(folders: [], files: files, includeSubFolders: false)
        connection.invoke(method: "AddFiles", playListId, request) { _ in }
    }

    func addUrlFile(playListId: Int, url: String, onlyVideo: Bool) {
        let request = AddUrlToPlayListRequestDto(url: url, onlyVideo: onlyVideo)
        connection.invoke(method: "AddUrlFile", playListId, request) { _ in }
    }

    func addFolderOrFileOrUrl(playListId: Int, path: String, includeSubFolders: Bool, onlyVideo: Bool) {
        // Use same DTO shape as client (union modeled by fields)
        struct AddFolderOrFileOrUrlToPlayListRequestDto: Codable { let path: String; let includeSubFolders: Bool; let onlyVideo: Bool }
        let request = AddFolderOrFileOrUrlToPlayListRequestDto(path: path, includeSubFolders: includeSubFolders, onlyVideo: onlyVideo)
        connection.invoke(method: "AddFolderOrFileOrUrl", playListId, request) { _ in }
    }

    func sortFiles(playListId: Int, sortMode: Int) {
        // SortMode mirrors TS enum; pass rawValue/int from UI
        connection.invoke(method: "SortFiles", playListId, sortMode) { _ in }
    }

    // MARK: - File methods (parity with ClientApp)
    func loopFile(playListId: Int, id: Int, loop: Bool) {
        connection.invoke(method: "LoopFile", playListId, id, loop) { _ in }
    }

    func deleteFile(playListId: Int, id: Int) {
        connection.invoke(method: "DeleteFile", playListId, id) { _ in }
    }

    func setFileOptions(streamIndex: Int, isAudio: Bool, isSubTitle: Bool, isQuality: Bool) {
        let request = SetFileOptionsRequestDto(streamIndex: streamIndex, isAudio: isAudio, isSubTitle: isSubTitle, isQuality: isQuality)
        connection.invoke(method: "SetFileOptions", request) { _ in }
    }

    func updateFilePosition(playListId: Int, id: Int, newIndex: Int) {
        connection.invoke(method: "UpdateFilePosition", playListId, id, newIndex) { _ in }
    }
}

// MARK: - HubConnectionDelegate
extension SignalRService {
    func connectionDidOpen(hubConnection: HubConnection) {
        DispatchQueue.main.async { self.onClientConnected.send(()) }
    }

    func connectionDidFailToOpen(error: Error) {
        DispatchQueue.main.async { self.onClientDisconnected.send(()) }
    }

    func connectionDidClose(error: Error?) {
        DispatchQueue.main.async { self.onClientDisconnected.send(()) }
    }

    func connectionWillReconnect(error: Error) {
        // transient disconnect
        DispatchQueue.main.async { self.onClientDisconnected.send(()) }
    }

    func connectionDidReconnect() {
        DispatchQueue.main.async { self.onClientConnected.send(()) }
    }
}
