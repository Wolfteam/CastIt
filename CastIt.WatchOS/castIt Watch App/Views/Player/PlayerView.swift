import SwiftUI

struct PlayerView: View {
    @State private var viewModel: PlayerViewModel
    @State private var showMore: Bool = false;

    init(container: DependencyContainer) {
        _viewModel = State(initialValue: container.playerViewModel)
    }

    var body: some View {
        NavigationView {
            ZStack (alignment: .topTrailing) {
                Button(action: {
                    showMore.toggle()
                }) {
                    Image(systemName: "ellipsis")
                }
                .frame(width: 35, height: 35)
                .clipShape(Circle())
                .glassEffect()
                .sheet(isPresented: $showMore) {
                    Button(role: .destructive, action: { viewModel.stop() }) {
                        Label("Stop", systemImage: "stop.fill")
                    }
                    .disabled(viewModel.status?.player.isPlaying == false)
                }
                VStack {
                    Image(systemName: "photo")
                        .resizable()
                        .aspectRatio(contentMode: .fill)
                        .frame(width: 80, height: 80)
                    
                    Text(viewModel.status?.playList?.name ?? "")
                        .font(.headline)
                        .lineLimit(1)
                    Text(viewModel.status?.playedFile?.name ?? "")
                        .font(.caption)
                        .fontWeight(.ultraLight)
                        .lineLimit(1)
                    
                    HStack (alignment: .center) {
                        Button(action: { viewModel.goTo(next: false, previous: true) }) {
                            Image(systemName: "backward.end.fill")
                        }
                        .disabled(viewModel.status?.player.isPlaying == false)
                        .glassEffect()
                        .frame(width: 45, height: 45)
                        .clipShape(Circle())
                        
                        Spacer()
                        
                        Button(action: { viewModel.togglePlayBack() }) {
                            Image(systemName: viewModel.status?.player.isPlaying == true ? "pause.fill" : "play.fill")
                        }
                        .disabled(viewModel.status?.player.isPlaying == false)
                        .glassEffect()
                        .frame(width: 60, height: 60)
                        .clipShape(Circle())
                        
                        Spacer()
                        
                        Button(action: { viewModel.goTo(next: true, previous: false) }) {
                            Image(systemName: "forward.end.fill")
                        }
                        .disabled(viewModel.status?.player.isPlaying == false)
                        .glassEffect()
                        .frame(width: 45, height: 45)
                        .clipShape(Circle())
                    }
                }
            }
        }
    }
}

#Preview {
    PlayerView(container: AppContainer())
}
