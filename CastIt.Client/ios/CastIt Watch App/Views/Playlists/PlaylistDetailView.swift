import SwiftUI

struct PlaylistDetailView: View {
    private let container: DependencyContainer
    let playlist: GetAllPlayListResponseDto
    @State private var viewModel: PlaylistViewModel

    init(container: DependencyContainer, playlist: GetAllPlayListResponseDto) {
        self.container = container
        self.playlist = playlist
        _viewModel = State(initialValue: container.makePlaylistViewModel())
    }

    var body: some View {
        List {
            ForEach(viewModel.files) { item in
                FileItemView(container: container, item: item)
            }
        }
        .navigationTitle(playlist.name)
        .onAppear {
            Task {
                await viewModel.loadPlaylist(id: playlist.id)
            }
        }
    }
}




#Preview {
    PlaylistDetailView(
        container: AppContainer(),
        playlist: GetAllPlayListResponseDto(
            id: 1,
            name: "Anime Favorites",
            position: 1,
            loop: false,
            shuffle: false,
            numberOfFiles: 10,
            playedTime: "03:29:51",
            totalDuration: "03:29:51 / 05:49:13",
            imageUrl: "http://localhost/img.png",
            lastPlayedDate: "2025/12/14",
            //iles: []
        )
    )
}
