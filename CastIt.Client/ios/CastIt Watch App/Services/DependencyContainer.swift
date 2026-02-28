import Foundation
import Observation

@MainActor
protocol DependencyContainer {
    // Shared ViewModels
    var playerViewModel: PlayerViewModel { get }
    var settingsViewModel: SettingsViewModel { get }
    var playlistsViewModel: PlaylistsViewModel { get }

    // App routing
    var router: AppRouter { get }

    // Factories (non-shared)
    func makePlaylistViewModel(id: Int, name: String) -> PlaylistViewModel
    func makePlaylistItemViewModel(playlist: GetAllPlayListResponseDto) -> PlaylistItemViewModel
    func makeFileItemViewModel(file: FileItemResponseDto) -> FileItemViewModel

    // Low-level services (exposed only if strictly necessary)
    var signalRService: SignalRService { get }
    var apiService: ApiService { get }
}

@MainActor
final class AppContainer: DependencyContainer {
    // Services
    let signalRService: SignalRService
    let apiService: ApiService
    let router: AppRouter

    // Shared VMs
    let playerViewModel: PlayerViewModel
    let settingsViewModel: SettingsViewModel
    let playlistsViewModel: PlaylistsViewModel

    // Internal wiring helper
    private let viewModelService: ViewModelService

    init() {
        self.signalRService = SignalRService()
        self.apiService = ApiService()
        self.router = AppRouter()

        // Create shared VMs
        self.playerViewModel = PlayerViewModel(signalRService: signalRService)
        self.settingsViewModel = SettingsViewModel(signalRService: signalRService)
        self.playlistsViewModel = PlaylistsViewModel(signalRService: signalRService)

        // Wire SignalR → shared VMs
        self.viewModelService = ViewModelService(
            signalRService: signalRService,
            playerViewModel: playerViewModel,
            playlistsViewModel: playlistsViewModel,
            settingsViewModel: settingsViewModel
        )

        // Start connection if URL is set
        if !settingsViewModel.serverUrl.isEmpty {
            self.signalRService.connectWithUrl(settingsViewModel.serverUrl)
        }
    }

    func makePlaylistViewModel(id: Int, name: String) -> PlaylistViewModel {
        PlaylistViewModel(signalRService: signalRService, id: id, name: name)
    }

    func makePlaylistItemViewModel(playlist: GetAllPlayListResponseDto) -> PlaylistItemViewModel {
        PlaylistItemViewModel(signalRService: signalRService, playlist: playlist)
    }

    func makeFileItemViewModel(file: FileItemResponseDto) -> FileItemViewModel {
        FileItemViewModel(signalRService: signalRService, file: file)
    }
}
