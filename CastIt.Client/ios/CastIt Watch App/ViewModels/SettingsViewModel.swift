import Foundation
import Combine
import Observation
import SwiftUI

@Observable
class SettingsViewModel {
    var serverUrl: String = "" {
        didSet {
            UserDefaults.standard.set(serverUrl, forKey: "serverUrl")
        }
    }
    var isConnected: Bool = false
    var isLoading: Bool = false
    
    var startFilesFromTheStart: Bool = false
    var playNextFileAutomatically: Bool = false
    var forceVideoTranscode: Bool = false
    var forceAudioTranscode: Bool = false
    var videoScale: VideoScale = .original
    var enableHardwareAcceleration: Bool = false
    var webVideoQuality: Int = 0

    var currentSubtitleFgColor: SubtitleFgColor = .white
    var currentSubtitleBgColor: SubtitleBgColor = .transparent
    var currentSubtitleFontScale: SubtitleFontScale = .hundredAndFifty
    var currentSubtitleFontStyle: TextTrackFontStyle = .bold
    var currentSubtitleFontFamily: TextTrackFontGenericFamily = .casual
    var subtitleDelayInSeconds: Double = 0
    var loadFirstSubtitleFoundAutomatically: Bool = false

    private let signalRService: SignalRService
    private var cancellables = Set<AnyCancellable>()

    init(signalRService: SignalRService) {
        self.signalRService = signalRService
        self.serverUrl = UserDefaults.standard.string(forKey: "serverUrl") ?? ""
        bind()
    }

    private func bind() {
        signalRService.onPlayerSettingsChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] settings in
                guard let self else { return }
                self.update(from: settings)
            }
            .store(in: &cancellables)
        
        signalRService.onClientConnected
            .receive(on: DispatchQueue.main)
            .sink { [weak self] in
                self?.isConnected = true
            }
            .store(in: &cancellables)
        
        signalRService.onClientDisconnected
            .receive(on: DispatchQueue.main)
            .sink { [weak self] in
                self?.isConnected = false
            }
            .store(in: &cancellables)
    }

    private func update(from settings: ServerAppSettings) {
        self.startFilesFromTheStart = settings.startFilesFromTheStart
        self.playNextFileAutomatically = settings.playNextFileAutomatically
        self.forceVideoTranscode = settings.forceVideoTranscode
        self.forceAudioTranscode = settings.forceAudioTranscode
        self.videoScale = settings.videoScale
        self.enableHardwareAcceleration = settings.enableHardwareAcceleration
        self.webVideoQuality = settings.webVideoQuality
        self.currentSubtitleFgColor = settings.currentSubtitleFgColor
        self.currentSubtitleBgColor = settings.currentSubtitleBgColor
        self.currentSubtitleFontScale = settings.currentSubtitleFontScale
        self.currentSubtitleFontStyle = settings.currentSubtitleFontStyle
        self.currentSubtitleFontFamily = settings.currentSubtitleFontFamily
        self.subtitleDelayInSeconds = settings.subtitleDelayInSeconds
        self.loadFirstSubtitleFoundAutomatically = settings.loadFirstSubtitleFoundAutomatically
    }

    func updateSettings() {
        let newSettings = ServerAppSettings(
            startFilesFromTheStart: startFilesFromTheStart,
            playNextFileAutomatically: playNextFileAutomatically,
            forceVideoTranscode: forceVideoTranscode,
            forceAudioTranscode: forceAudioTranscode,
            videoScale: videoScale,
            enableHardwareAcceleration: enableHardwareAcceleration,
            webVideoQuality: webVideoQuality,
            currentSubtitleFgColor: currentSubtitleFgColor,
            currentSubtitleBgColor: currentSubtitleBgColor,
            currentSubtitleFontScale: currentSubtitleFontScale,
            currentSubtitleFontStyle: currentSubtitleFontStyle,
            currentSubtitleFontFamily: currentSubtitleFontFamily,
            subtitleDelayInSeconds: subtitleDelayInSeconds,
            loadFirstSubtitleFoundAutomatically: loadFirstSubtitleFoundAutomatically
        )

        signalRService.updateSettings(newSettings)
    }
    
    func applyServerUrl() {
        serverUrl = serverUrl.lowercased().trimmingCharacters(in: .whitespacesAndNewlines)
        guard !serverUrl.isEmpty, let _ = URL(string: serverUrl) else {
            return
        }
        
        isLoading = true
        isConnected = false
        signalRService.updateUrl(serverUrl)
        signalRService.disconnect()
        signalRService.connect()
        
        // Wait for 3 seconds as requested, then check connection
        DispatchQueue.main.asyncAfter(deadline: .now() + 3) { [weak self] in
            guard let self = self else { return }
            self.isLoading = false
            // isConnected is already being updated via signalRService.onClientConnected/Disconnected
        }
    }
}
