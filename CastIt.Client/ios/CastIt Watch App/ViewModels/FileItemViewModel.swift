import Foundation
import Combine
import Observation

@Observable
class FileItemViewModel {
    var id: Int
    var name: String
    var description: String
    var totalSeconds: Double
    var path: String
    var position: Int
    var playedPercentage: Double
    var playListId: Int
    var loop: Bool
    var lastPlayedDate: String?

    var isBeingPlayed: Bool
    var isLocalFile: Bool
    var isUrlFile: Bool
    var playedSeconds: Double
    var canStartPlayingFromCurrentPercentage: Bool
    var wasPlayed: Bool
    var isCached: Bool

    var exists: Bool
    var filename: String
    var size: String
    var mediaExtension: String

    var subTitle: String
    var resolution: String?
    var duration: String
    var playedTime: String
    var totalDuration: String
    var fullTotalDuration: String
    var thumbnailUrl: String?

    var currentFileVideos: [FileItemOptionsResponseDto]
    var currentFileAudios: [FileItemOptionsResponseDto]
    var currentFileSubTitles: [FileItemOptionsResponseDto]
    var currentFileQualities: [FileItemOptionsResponseDto]
    var currentFileVideoStreamIndex: Int
    var currentFileAudioStreamIndex: Int
    var currentFileSubTitleStreamIndex: Int
    var currentFileQuality: Int

    var showDeleteConfirmation: Bool = false

    private let signalRService: SignalRService
    private var cancellables = Set<AnyCancellable>()

    init(signalRService: SignalRService, file: FileItemResponseDto) {
        self.signalRService = signalRService
        
        self.id = file.id
        self.name = file.name
        self.description = file.description
        self.totalSeconds = file.totalSeconds
        self.path = file.path
        self.position = file.position
        self.playedPercentage = file.playedPercentage
        self.playListId = file.playListId
        self.loop = file.loop
        self.lastPlayedDate = file.lastPlayedDate
        self.isBeingPlayed = file.isBeingPlayed
        self.isLocalFile = file.isLocalFile
        self.isUrlFile = file.isUrlFile
        self.playedSeconds = file.playedSeconds
        self.canStartPlayingFromCurrentPercentage = file.canStartPlayingFromCurrentPercentage
        self.wasPlayed = file.wasPlayed
        self.isCached = file.isCached
        self.exists = file.exists
        self.filename = file.filename
        self.size = file.size
        self.mediaExtension = file.extension
        self.subTitle = file.subTitle
        self.resolution = file.resolution
        self.duration = file.duration
        self.playedTime = file.playedTime
        self.totalDuration = file.totalDuration
        self.fullTotalDuration = file.fullTotalDuration
        self.thumbnailUrl = file.thumbnailUrl
        self.currentFileVideos = file.currentFileVideos
        self.currentFileAudios = file.currentFileAudios
        self.currentFileSubTitles = file.currentFileSubTitles
        self.currentFileQualities = file.currentFileQualities
        self.currentFileVideoStreamIndex = file.currentFileVideoStreamIndex
        self.currentFileAudioStreamIndex = file.currentFileAudioStreamIndex
        self.currentFileSubTitleStreamIndex = file.currentFileSubTitleStreamIndex
        self.currentFileQuality = file.currentFileQuality

        bind()
    }

    private func bind() {
        // FilesChanged: find this file and replace
        signalRService.onFilesChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] files in
                guard let self else { return }
                if let updated = files.first(where: { $0.id == self.id }) {
                    self.update(from: updated)
                }
            }
            .store(in: &cancellables)

        // PlayerStatusChanged: if this is the played file update it, otherwise clear playing flag
        signalRService.onPlayerStatusChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] status in
                guard let self else { return }
                guard let played = status.playedFile else {
                    if self.isBeingPlayed {
                        self.isBeingPlayed = false
                    }
                    return
                }
                if played.id == self.id {
                    self.update(from: played)
                } else if self.isBeingPlayed {
                    self.isBeingPlayed = false
                }
            }
            .store(in: &cancellables)

        // FileEndReached: mark 100%
        signalRService.onFileEndReached
            .receive(on: DispatchQueue.main)
            .sink { [weak self] ended in
                guard let self, ended.id == self.id else { return }
                self.playedPercentage = 100
            }
            .store(in: &cancellables)
    }

    private func update(from file: FileItemResponseDto) {
        self.id = file.id
        self.name = file.name
        self.description = file.description
        self.totalSeconds = file.totalSeconds
        self.path = file.path
        self.position = file.position
        self.playedPercentage = file.playedPercentage
        self.playListId = file.playListId
        self.loop = file.loop
        self.lastPlayedDate = file.lastPlayedDate
        self.isBeingPlayed = file.isBeingPlayed
        self.isLocalFile = file.isLocalFile
        self.isUrlFile = file.isUrlFile
        self.playedSeconds = file.playedSeconds
        self.canStartPlayingFromCurrentPercentage = file.canStartPlayingFromCurrentPercentage
        self.wasPlayed = file.wasPlayed
        self.isCached = file.isCached
        self.exists = file.exists
        self.filename = file.filename
        self.size = file.size
        self.mediaExtension = file.extension
        self.subTitle = file.subTitle
        self.resolution = file.resolution
        self.duration = file.duration
        self.playedTime = file.playedTime
        self.totalDuration = file.totalDuration
        self.fullTotalDuration = file.fullTotalDuration
        self.thumbnailUrl = file.thumbnailUrl
        self.currentFileVideos = file.currentFileVideos
        self.currentFileAudios = file.currentFileAudios
        self.currentFileSubTitles = file.currentFileSubTitles
        self.currentFileQualities = file.currentFileQualities
        self.currentFileVideoStreamIndex = file.currentFileVideoStreamIndex
        self.currentFileAudioStreamIndex = file.currentFileAudioStreamIndex
        self.currentFileSubTitleStreamIndex = file.currentFileSubTitleStreamIndex
        self.currentFileQuality = file.currentFileQuality
    }

    func play(force: Bool = false) {
        signalRService.play(playListId: playListId, fileId: id, force: force)
    }

    func toggleLoop() {
        signalRService.loopFile(playListId: playListId, id: id, loop: !loop)
    }

    func delete() {
        signalRService.deleteFile(playListId: playListId, id: id)
    }

    deinit {
        debugPrint("FileItemViewModel deinit: \(id) - \(name)")
    }
}
