import SwiftUI

struct PlaylistsView: View {
    @State private var viewModel: PlaylistsViewModel
    private let container: DependencyContainer

    init(container: DependencyContainer) {
        self.container = container
        _viewModel = State(initialValue: container.playlistsViewModel)
    }

    var body: some View {
        NavigationView {
            List(viewModel.playlists) { playlist in
                NavigationLink(destination: PlaylistDetailView(container: container, playlist: playlist)) {
                    HStack (alignment: .center) {
                        VStack(alignment: .leading) {
                            Text(playlist.name)
                                .font(.headline)
                                .lineLimit(1)
                            Text("\(playlist.totalDuration)")
                                .font(.footnote)
                                .lineLimit(1)
                            Text(playlist.lastPlayedDate ?? "N/A")
                                .font(.footnote)
                                .lineLimit(1)
                        }
                        Spacer()
                        ZStack {
                            RoundedRectangle(cornerRadius: 30, style: .continuous)
                                .fill(Color.red)
                            Text("\(playlist.numberOfFiles)")
                                .foregroundColor(.white)
                                .font(.footnote)
                                .fontWeight(.bold)
                                .lineLimit(1)
                        }
                        .frame(width: 40, height: 40)
                    }
                }
            }
            .navigationTitle("Playlists")
            .onAppear {
                viewModel.getPlaylists()
            }
        }
    }
}

#Preview {
    PlaylistsView(container: AppContainer())
}
