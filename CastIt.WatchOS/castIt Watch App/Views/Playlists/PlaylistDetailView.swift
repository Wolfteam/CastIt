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
                FileItemView(item: item)
            }
        }
        .navigationTitle(playlist.name)
        .onAppear {
            Task {
                await viewModel.getPlaylist(id: playlist.id)
            }
        }
    }
}


struct FileItemView: View {
    @State var item: FileItemResponseDto
    //todo: view model
    
    var body: some View {
        VStack(alignment: .leading) {
            Text(item.name)
                .font(.caption2)
                .fontWeight(.bold)
                .lineLimit(2)
            Text(item.subTitle)
                .font(.footnote)
                .fontWeight(.ultraLight)
                .lineLimit(1)
            HStack (alignment: .center) {
                Text("00:10")
                    .font(.footnote)
                    .lineLimit(1)
                Spacer()
                Text("30:20")
                    .font(.footnote)
                    .lineLimit(1)
            }
            ProgressView(value: item.playedPercentage, total: 100)
                .frame(height: 2)
                .clipShape(Capsule())
                .progressViewStyle(LinearProgressViewStyle())
        }
        .onTapGesture {
            //viewModel.play(file: fileItem)
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
