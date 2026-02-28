import SwiftUI

struct PlaylistDetailView: View {
    private let container: DependencyContainer
    private let id: Int
    private let name: String
    @State private var viewModel: PlaylistViewModel

    init(container: DependencyContainer, id: Int, name: String) {
        self.container = container
        self.id = id
        self.name = name
        _viewModel = State(initialValue: container.makePlaylistViewModel(id: id, name: name))
    }

    var body: some View {
        Group {
            if !container.settingsViewModel.isConnected {
                NotConnectedView()
            } else if viewModel.isLoading {
                ProgressView()
            } else {
                List {
                    ForEach(viewModel.files) { item in
                        FileItemView(container: container, item: item)
                    }
                }
            }
        }
        .navigationTitle(name)
        .onAppear {
            Task {
                debugPrint("loading playlist")
                await viewModel.loadPlaylist(id: id)
                debugPrint("loaded")
            }
        }
    }
}




#Preview {
    PlaylistDetailView(
        container: AppContainer(),
        id: 1,
        name: "Anime Favorites"
    )
}
