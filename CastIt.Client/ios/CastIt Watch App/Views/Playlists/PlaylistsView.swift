import SwiftUI

struct PlaylistsView: View {
    @State private var viewModel: PlaylistsViewModel
    private let container: DependencyContainer
    @Bindable private var router: AppRouter

    init(container: DependencyContainer) {
        self.container = container
        _viewModel = State(initialValue: container.playlistsViewModel)
        _router = Bindable(wrappedValue: container.router)
    }

    var body: some View {
        if !container.settingsViewModel.isConnected {
            NotConnectedView()
                .navigationTitle("Playlists")
        } else {
            List(viewModel.playlists) { playlist in
                Button {
                    router.selectedPlaylist = playlist
                } label: {
                    PlaylistItemView(viewModel: container.makePlaylistItemViewModel(playlist: playlist))
                }
                .buttonStyle(.plain)
            }
            .navigationTitle("Playlists")
            .onAppear {
                viewModel.getPlaylists()
            }
        }
    }
}

struct PlaylistItemView: View {
    @State private var viewModel: PlaylistItemViewModel

    init(viewModel: PlaylistItemViewModel) {
        _viewModel = State(initialValue: viewModel)
    }

    var body: some View {
        HStack (alignment: .center) {
            VStack(alignment: .leading) {
                Text(viewModel.name)
                    .font(.headline)
                    .lineLimit(1)
                Text("\(viewModel.totalDuration)")
                    .font(.footnote)
                    .lineLimit(1)
                Text(viewModel.lastPlayedDate ?? "N/A")
                    .font(.footnote)
                    .lineLimit(1)
            }
            Spacer()
            ZStack {
                RoundedRectangle(cornerRadius: 30, style: .continuous)
                    .fill(Color.red)
                Text("\(viewModel.numberOfFiles)")
                    .foregroundColor(.white)
                    .font(.footnote)
                    .fontWeight(.bold)
                    .lineLimit(1)
            }
            .frame(width: 40, height: 40)
        }
        .contentShape(Rectangle())
        .swipeActions(edge: .trailing, allowsFullSwipe: false) {
            Button {
                viewModel.showDeleteConfirmation = true
            } label: {
                Label("Delete", systemImage: "trash")
            }
            .tint(.red)
            
            Button {
                viewModel.newName = viewModel.name
                viewModel.showRename = true
            } label: {
                Label("Rename", systemImage: "pencil")
            }
            .tint(.blue)
            
        }
        .swipeActions(edge: .leading, allowsFullSwipe: false) {
            Button {
                viewModel.toggleLoop()
            } label: {
                Label(viewModel.loop ? "Disable Loop" : "Enable Loop", systemImage: viewModel.loop ? "repeat.circle.fill" : "repeat.circle")
            }
            .tint(viewModel.loop ? .green : .gray)

            Button {
                viewModel.toggleShuffle()
            } label: {
                Label(viewModel.shuffle ? "Disable Shuffle" : "Enable Shuffle", systemImage: viewModel.shuffle ? "shuffle.circle.fill" : "shuffle.circle")
            }
            .tint(viewModel.shuffle ? .orange : .gray)
        }
        .sheet(isPresented: $viewModel.showRename) {
            ScrollView {
                VStack {
                    TextField("Playlist Name", text: $viewModel.newName)
                        .onSubmit {
                            viewModel.rename(newName: viewModel.newName)
                            viewModel.showRename = false
                        }
                    Button("Cancel") {
                        viewModel.showRename = false
                    }
                }
            }
        }
        .confirmationDialog("Delete Playlist?", isPresented: $viewModel.showDeleteConfirmation) {
            Button("Delete", role: .destructive) {
                viewModel.delete()
            }
            Button("Cancel", role: .cancel) {}
        } message: {
            Text("Are you sure you want to delete '\(viewModel.name)'?")
        }
    }
}

#Preview {
    PlaylistsView(container: AppContainer())
}
