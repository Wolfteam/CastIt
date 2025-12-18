import Foundation
import Combine
import Observation

@Observable
class FileItemViewModel {
    var file: FileItemResponseDto

    private let signalRService: SignalRService
    private var cancellables = Set<AnyCancellable>()

    init(signalRService: SignalRService, file: FileItemResponseDto) {
        self.signalRService = signalRService
        self.file = file
        bind()
    }

    private func bind() {
        // FilesChanged: find this file and replace
        signalRService.onFilesChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] files in
                guard let self else { return }
                if let updated = files.first(where: { $0.id == self.file.id }) {
                    self.file = updated
                }
            }
            .store(in: &cancellables)

        // PlayerStatusChanged: if this is the played file update it, otherwise clear playing flag
        signalRService.onPlayerStatusChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] status in
                guard let self else { return }
                guard let played = status.playedFile else {
                    if self.file.isBeingPlayed {
                        // Replace only the playing flag when stopping
                        self.file = FileItemViewModel.copy(self.file, isBeingPlayed: false)
                    }
                    return
                }
                if played.id == self.file.id {
                    self.file = played
                } else if self.file.isBeingPlayed {
                    self.file = FileItemViewModel.copy(self.file, isBeingPlayed: false)
                }
            }
            .store(in: &cancellables)

        // FileEndReached: mark 100%
        signalRService.onFileEndReached
            .receive(on: DispatchQueue.main)
            .sink { [weak self] ended in
                guard let self, ended.id == self.file.id else { return }
                self.file = FileItemViewModel.copy(self.file, playedPercentage: 100)
            }
            .store(in: &cancellables)
    }

    func play(force: Bool = false) {
        signalRService.play(playListId: file.playListId, fileId: file.id, force: force)
    }

    // Helper to rebuild FileItemResponseDto with minimal changes
    private static func copy(_ f: FileItemResponseDto,
                             playedPercentage: Double? = nil,
                             isBeingPlayed: Bool? = nil) -> FileItemResponseDto {
        return FileItemResponseDto(
            id: f.id,
            name: f.name,
            description: f.description,
            totalSeconds: f.totalSeconds,
            path: f.path,
            position: f.position,
            playedPercentage: playedPercentage ?? f.playedPercentage,
            playListId: f.playListId,
            loop: f.loop,
            lastPlayedDate: f.lastPlayedDate,

            isBeingPlayed: isBeingPlayed ?? f.isBeingPlayed,
            isLocalFile: f.isLocalFile,
            isUrlFile: f.isUrlFile,
            playedSeconds: f.playedSeconds,
            canStartPlayingFromCurrentPercentage: f.canStartPlayingFromCurrentPercentage,
            wasPlayed: f.wasPlayed,
            isCached: f.isCached,

            exists: f.exists,
            filename: f.filename,
            size: f.size,
            extension: f.extension,

            subTitle: f.subTitle,
            resolution: f.resolution,
            duration: f.duration,
            playedTime: f.playedTime,
            totalDuration: f.totalDuration,
            fullTotalDuration: f.fullTotalDuration,
            thumbnailUrl: f.thumbnailUrl,

            currentFileVideos: f.currentFileVideos,
            currentFileAudios: f.currentFileAudios,
            currentFileSubTitles: f.currentFileSubTitles,
            currentFileQualities: f.currentFileQualities,
            currentFileVideoStreamIndex: f.currentFileVideoStreamIndex,
            currentFileAudioStreamIndex: f.currentFileAudioStreamIndex,
            currentFileSubTitleStreamIndex: f.currentFileSubTitleStreamIndex,
            currentFileQuality: f.currentFileQuality
        )
    }
}
