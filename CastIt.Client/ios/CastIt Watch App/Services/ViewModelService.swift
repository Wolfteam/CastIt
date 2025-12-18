import Foundation
import Combine

class ViewModelService {
    private var cancellables = Set<AnyCancellable>()

    init(
        signalRService: SignalRService,
        playerViewModel: PlayerViewModel,
        playlistsViewModel: PlaylistsViewModel,
        settingsViewModel: SettingsViewModel
    ) {
        // Player
        signalRService.onPlayerStatusChanged
            .receive(on: DispatchQueue.main)
            .sink { playerViewModel.status = $0 }
            .store(in: &cancellables)

        signalRService.onPlayerSettingsChanged
            .receive(on: DispatchQueue.main)
            .sink {
                playerViewModel.settings = $0
                settingsViewModel.settings = $0
            }
            .store(in: &cancellables)

        signalRService.onCastDeviceSet
            .receive(on: DispatchQueue.main)
            .sink { playerViewModel.selectedDevice = $0 }
            .store(in: &cancellables)

        signalRService.onCastDevicesChanged
            .receive(on: DispatchQueue.main)
            .sink { playerViewModel.devices = $0 }
            .store(in: &cancellables)

        signalRService.onCastDeviceDisconnected
            .receive(on: DispatchQueue.main)
            .sink {
                playerViewModel.selectedDevice = nil
                if var status = playerViewModel.status {
                    playerViewModel.status = status
                }
            }
            .store(in: &cancellables)

        signalRService.onStoppedPlayBack
            .receive(on: DispatchQueue.main)
            .sink {
                if var status = playerViewModel.status {
                    playerViewModel.status = status
                }
            }
            .store(in: &cancellables)

        // Playlists
        signalRService.onPlayListsLoaded
            .receive(on: DispatchQueue.main)
            .sink { playlistsViewModel.playlists = $0 }
            .store(in: &cancellables)

        signalRService.onPlayListAdded
            .receive(on: DispatchQueue.main)
            .sink { playlistsViewModel.playlists.append($0) }
            .store(in: &cancellables)

        signalRService.onPlayListChanged
            .receive(on: DispatchQueue.main)
            .sink { updatedPlaylist in
                if let index = playlistsViewModel.playlists.firstIndex(where: { $0.id == updatedPlaylist.id }) {
                    playlistsViewModel.playlists[index] = updatedPlaylist
                }
            }
            .store(in: &cancellables)
            
        signalRService.onPlayListsChanged
            .receive(on: DispatchQueue.main)
            .sink { updatedPlaylists in
                for updatedPlaylist in updatedPlaylists {
                    if let index = playlistsViewModel.playlists.firstIndex(where: { $0.id == updatedPlaylist.id }) {
                        playlistsViewModel.playlists[index] = updatedPlaylist
                    }
                }
            }
            .store(in: &cancellables)

        signalRService.onPlayListDeleted
            .receive(on: DispatchQueue.main)
            .sink { deletedPlaylistId in
                playlistsViewModel.playlists.removeAll(where: { $0.id == deletedPlaylistId })
            }
            .store(in: &cancellables)

        signalRService.onPlayListBusy
            .receive(on: DispatchQueue.main)
            .sink { busyInfo in
                playlistsViewModel.busyPlaylists[busyInfo.playListId] = busyInfo.isBusy
            }
            .store(in: &cancellables)

        // Files: handled inside non-shared PlaylistViewModel instances
    }
}
