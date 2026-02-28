import Foundation
import Combine
import Observation
import SwiftUI

@Observable
class PlayerViewModel {
    var player: PlayerStatusResponseDto?
    var playList: GetAllPlayListResponseDto?
    var playedFile: FileItemResponseDto?
    var thumbnailRanges: [FileThumbnailRangeResponseDto] = []

    var settings: ServerAppSettings?
    var devices: [Receiver] = []
    var selectedDevice: Receiver?

    // UI state
    var isLoading: Bool = false
    var isConnected: Bool = false

    var showMore: Bool = false
    var showServerAlert: Bool = false
    var serverAlertText: String = ""
    var backgroundColors: [Color] = []
    
    var skipValue: Double {
        get {
            AppSettings.shared.playerSkipSeconds
        }
        set {
            AppSettings.shared.playerSkipSeconds = newValue
        }
    }
    
    // Volume debouncing
    private let volumeSubject = PassthroughSubject<Double, Never>()

    // Derived state
    var isPlayingOrPaused: Bool { player?.isPlayingOrPaused ?? false }
    var isPlaying: Bool { player?.isPlaying ?? false }
    var playedPercentage: Double { player?.playedPercentage ?? 0 }

    private let signalRService: SignalRService
    private var cancellables = Set<AnyCancellable>()

    init(signalRService: SignalRService) {
        self.signalRService = signalRService
        bind()
        setupVolumeDebounce()
    }

    private func setupVolumeDebounce() {
        volumeSubject
            .debounce(for: .milliseconds(500), scheduler: RunLoop.main)
            .sink { [weak self] level in
                guard let self else { return }
                self.signalRService.setVolume(level: level, isMuted: self.player?.isMuted ?? false)
            }
            .store(in: &cancellables)
    }

    func setVolume(_ level: Double) {
        // Update local state immediately for responsiveness if possible, 
        // though it will be overwritten by server status later.
        if let p = player {
            player = PlayerStatusResponseDto(
                mrl: p.mrl,
                isPlaying: p.isPlaying,
                isPaused: p.isPaused,
                isPlayingOrPaused: p.isPlayingOrPaused,
                currentMediaDuration: p.currentMediaDuration,
                elapsedSeconds: p.elapsedSeconds,
                playedPercentage: p.playedPercentage,
                volumeLevel: level,
                isMuted: p.isMuted
            )
        }
        volumeSubject.send(level)
    }

    func toggleLoop() {
        guard let file = playedFile else { return }
        signalRService.loopFile(playListId: file.playListId, id: file.id, loop: !file.loop)
    }

    func setFileOptions(option: FileItemOptionsResponseDto) {
        signalRService.setFileOptions(
            streamIndex: option.id,
            isAudio: option.isAudio,
            isSubTitle: option.isSubTitle,
            isQuality: option.isQuality
        )
    }

    func skipSeconds(_ seconds: Double) {
        signalRService.skipSeconds(seconds)
    }

    func togglePlayBack() {
        signalRService.togglePlayBack()
    }

    func goTo(next: Bool, previous: Bool) {
        signalRService.goTo(next: next, previous: previous)
    }
    
    func stop() {
        signalRService.stopPlayBack()
    }

    private func resetState() {
        resetPlaybackState()
        settings = nil
        devices = []
        selectedDevice = nil
        isConnected = false
        showMore = false
        showServerAlert = false
        serverAlertText = ""
    }

    private func resetPlaybackState() {
        player = nil
        playList = nil
        playedFile = nil
        thumbnailRanges = []
        isLoading = false
        backgroundColors = []
    }

    private func isPlayerStatusDifferent(current: PlayerStatusResponseDto?, new: PlayerStatusResponseDto) -> Bool {
        guard let current else { return true }
        return current.mrl != new.mrl
            || current.isPlaying != new.isPlaying
            || current.isPaused != new.isPaused
            || current.isPlayingOrPaused != new.isPlayingOrPaused
            || current.currentMediaDuration != new.currentMediaDuration
            || current.elapsedSeconds != new.elapsedSeconds
            || current.playedPercentage != new.playedPercentage
            || current.volumeLevel != new.volumeLevel
            || current.isMuted != new.isMuted
    }

    private func isPlayListDifferent(current: GetAllPlayListResponseDto?, new: GetAllPlayListResponseDto?) -> Bool {
        switch (current, new) {
        case (nil, nil):
            return false
        case (nil, _), (_, nil):
            return true
        case (let current?, let new?):
            return current != new
        }
    }

    private func isFileItemDifferent(current: FileItemResponseDto?, new: FileItemResponseDto?) -> Bool {
        switch (current, new) {
        case (nil, nil):
            return false
        case (nil, _), (_, nil):
            return true
        case (let current?, let new?):
            return current.id != new.id
                || current.totalSeconds != new.totalSeconds
                || current.path != new.path
                || current.position != new.position
                || current.playedPercentage != new.playedPercentage
                || current.playListId != new.playListId
                || current.loop != new.loop
                || current.lastPlayedDate != new.lastPlayedDate
                || current.isBeingPlayed != new.isBeingPlayed
                || current.isLocalFile != new.isLocalFile
                || current.isUrlFile != new.isUrlFile
                || current.playedSeconds != new.playedSeconds
                || current.canStartPlayingFromCurrentPercentage != new.canStartPlayingFromCurrentPercentage
                || current.wasPlayed != new.wasPlayed
                || current.isCached != new.isCached
                || current.exists != new.exists
                || current.filename != new.filename
                || current.size != new.size
                || current.`extension` != new.`extension`
                || current.subTitle != new.subTitle
                || current.resolution != new.resolution
                || current.duration != new.duration
                || current.playedTime != new.playedTime
                || current.totalDuration != new.totalDuration
                || current.fullTotalDuration != new.fullTotalDuration
                || current.thumbnailUrl != new.thumbnailUrl
                || isFileItemOptionsDifferent(current: current.currentFileVideos, new: new.currentFileVideos)
                || isFileItemOptionsDifferent(current: current.currentFileAudios, new: new.currentFileAudios)
                || isFileItemOptionsDifferent(current: current.currentFileSubTitles, new: new.currentFileSubTitles)
                || isFileItemOptionsDifferent(current: current.currentFileQualities, new: new.currentFileQualities)
                || current.currentFileVideoStreamIndex != new.currentFileVideoStreamIndex
                || current.currentFileAudioStreamIndex != new.currentFileAudioStreamIndex
                || current.currentFileSubTitleStreamIndex != new.currentFileSubTitleStreamIndex
                || current.currentFileQuality != new.currentFileQuality
        }
    }

    private func isFileItemOptionsDifferent(current: [FileItemOptionsResponseDto], new: [FileItemOptionsResponseDto]) -> Bool {
        guard current.count == new.count else { return true }
        for (currentItem, newItem) in zip(current, new) {
            if isFileItemOptionDifferent(current: currentItem, new: newItem) {
                return true
            }
        }
        return false
    }

    private func isFileItemOptionDifferent(current: FileItemOptionsResponseDto, new: FileItemOptionsResponseDto) -> Bool {
        return current.id != new.id
            || current.isVideo != new.isVideo
            || current.isAudio != new.isAudio
            || current.isSubTitle != new.isSubTitle
            || current.isQuality != new.isQuality
            || current.path != new.path
            || current.text != new.text
            || current.isSelected != new.isSelected
            || current.isEnabled != new.isEnabled
    }

    private func isThumbnailRangesDifferent(current: [FileThumbnailRangeResponseDto], new: [FileThumbnailRangeResponseDto]) -> Bool {
        guard current.count == new.count else { return true }
        for (currentRange, newRange) in zip(current, new) {
            if isThumbnailRangeDifferent(current: currentRange, new: newRange) {
                return true
            }
        }
        return false
    }

    private func isThumbnailRangeDifferent(current: FileThumbnailRangeResponseDto, new: FileThumbnailRangeResponseDto) -> Bool {
        return current.previewThumbnailUrl != new.previewThumbnailUrl
            || isNumericRangeDifferent(current: current.thumbnailRange, new: new.thumbnailRange)
            || isThumbnailPositionsDifferent(current: current.thumbnailPositions, new: new.thumbnailPositions)
    }

    private func isNumericRangeDifferent(current: NumericRange, new: NumericRange) -> Bool {
        return current.minimum != new.minimum
            || current.maximum != new.maximum
    }

    private func isThumbnailPositionsDifferent(current: [FileThumbnailPositionResponseDto], new: [FileThumbnailPositionResponseDto]) -> Bool {
        guard current.count == new.count else { return true }
        for (currentPosition, newPosition) in zip(current, new) {
            if isThumbnailPositionDifferent(current: currentPosition, new: newPosition) {
                return true
            }
        }
        return false
    }

    private func isThumbnailPositionDifferent(current: FileThumbnailPositionResponseDto, new: FileThumbnailPositionResponseDto) -> Bool {
        return current.x != new.x
            || current.y != new.y
            || current.second != new.second
    }

    private func updateBackgroundColors() {
        guard let thumbnailUrl = playedFile?.thumbnailUrl, let url = URL(string: thumbnailUrl) else {
            self.backgroundColors = []
            return
        }

        Task {
            do {
                let (data, _) = try await URLSession.shared.data(from: url)
                let colors = DominantColorsExtractor.dominantColors(from: data)
                await MainActor.run {
                    self.backgroundColors = colors
                }
            } catch {
                await MainActor.run {
                    self.backgroundColors = []
                }
            }
        }
    }

    private func bind() {
        // Player status changed
        signalRService.onPlayerStatusChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] status in
                guard let self else { return }
                if isPlayerStatusDifferent(current: self.player, new: status.player) {
                    self.player = status.player
                }
                if isPlayListDifferent(current: self.playList, new: status.playList) {
                    self.playList = status.playList
                }
                let playedFileChanged = isFileItemDifferent(current: self.playedFile, new: status.playedFile)
                if playedFileChanged {
                    self.playedFile = status.playedFile
                }
                if isThumbnailRangesDifferent(current: self.thumbnailRanges, new: status.thumbnailRanges) {
                    self.thumbnailRanges = status.thumbnailRanges
                }

                if playedFileChanged {
                    self.updateBackgroundColors()
                }
            }
            .store(in: &cancellables)

        // Settings changed
        signalRService.onPlayerSettingsChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] settings in
                self?.settings = settings
            }
            .store(in: &cancellables)

        // Loading states for player
        signalRService.onFileLoading
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.isLoading = true }
            .store(in: &cancellables)

        signalRService.onFileLoaded
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.isLoading = false }
            .store(in: &cancellables)

        signalRService.onStoppedPlayBack
            .receive(on: DispatchQueue.main)
            .sink { [weak self] in
                self?.resetPlaybackState()
            }
            .store(in: &cancellables)

        // End reached – keep percentage at 100% if possible
        signalRService.onFileEndReached
            .receive(on: DispatchQueue.main)
            .sink { [weak self] file in
                guard let self, let current = self.playedFile, current.id == file.id else { return }
                // Force an update by touching status; actual percentage will come from server soon
                // Since we flattened it, we might need to manually update player percentage if we want immediate UI feedback
                if let p = self.player {
                    self.player = PlayerStatusResponseDto(
                        mrl: p.mrl,
                        isPlaying: p.isPlaying,
                        isPaused: p.isPaused,
                        isPlayingOrPaused: p.isPlayingOrPaused,
                        currentMediaDuration: p.currentMediaDuration,
                        elapsedSeconds: p.elapsedSeconds,
                        playedPercentage: 100,
                        volumeLevel: p.volumeLevel,
                        isMuted: p.isMuted
                    )
                }
            }
            .store(in: &cancellables)

        // Connection status
        signalRService.onClientConnected
            .receive(on: DispatchQueue.main)
            .sink { [weak self] in self?.isConnected = true }
            .store(in: &cancellables)

        signalRService.onClientDisconnected
            .receive(on: DispatchQueue.main)
            .sink { [weak self] in
                self?.resetState()
            }
            .store(in: &cancellables)
    }
}
