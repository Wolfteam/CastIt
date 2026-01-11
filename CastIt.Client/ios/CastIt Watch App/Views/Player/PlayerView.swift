import SwiftUI
import Combine

struct PlayerView: View {
    private let container: DependencyContainer
    @State private var viewModel: PlayerViewModel

    init(container: DependencyContainer) {
        self.container = container
        _viewModel = State(initialValue: container.playerViewModel)
    }

    var body: some View {
        NavigationStack {
            VStack (alignment: .center) {
                if !container.settingsViewModel.isConnected {
                    NotConnectedView()
                } else {
                    VStack (alignment: .center) {
                        PlayerImage(viewModel: viewModel)
                            .onTapGesture {
                                viewModel.showMore.toggle()
                            }
                            .sheet(isPresented: $viewModel.showMore) {
                                Button(role: .destructive, action: {
                                    viewModel.stop()
                                    viewModel.showMore = false
                                }) {
                                    Label("Stop", systemImage: "stop.fill")
                                }
                                .disabled(!viewModel.isPlayingOrPaused)
                            }

                        if (viewModel.playList?.name != nil) {
                            Text(viewModel.playList?.name ?? "")
                                .font(.headline)
                                .lineLimit(1)
                        }
                        
                        if (viewModel.playedFile?.name != nil) {
                            Text(viewModel.playedFile?.name ?? "")
                                .font(.caption)
                                .fontWeight(.ultraLight)
                                .lineLimit(1)
                        }
                        
                        // Loading overlay
                        if viewModel.isLoading {
                            ProgressView()
                                .progressViewStyle(.circular)
                        }
                    }
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
                    Spacer()
                    
                    if viewModel.isPlayingOrPaused {
                        PlayerButtons(viewModel: viewModel)
                    } else if !viewModel.isLoading {
                        Text("Nothing is being played")
                            .font(.footnote)
                            .foregroundStyle(.secondary)
                            .padding(.top, 8)
                    }
                }
            }
            .padding(.horizontal, 8)
            .frame(maxWidth: .infinity, maxHeight: .infinity)
            .background(playerBackground)
            // Listen for server messages to show alerts
            .onReceive(container.signalRService.onServerMessage.receive(on: RunLoop.main)) { message in
                viewModel.serverAlertText = message.localizedDescription
                viewModel.showServerAlert = true
            }
            // Present alert
            .alert("Server message", isPresented: $viewModel.showServerAlert) {
                Button("Close", role: .cancel) { viewModel.showServerAlert = false }
            } message: {
                Text(viewModel.serverAlertText)
            }
        }
    }

    private var playerBackground: some View {
        Group {
            if viewModel.isPlayingOrPaused, !viewModel.backgroundColors.isEmpty {
                let colors = viewModel.backgroundColors.count == 1 ? [viewModel.backgroundColors[0], viewModel.backgroundColors[0].opacity(0.7)] : viewModel.backgroundColors
                LinearGradient(colors: colors, startPoint: .topLeading, endPoint: .bottomTrailing)
                    .ignoresSafeArea()
                    .opacity(0.35)
            } else {
                Color.black.ignoresSafeArea()
            }
        }
    }
}

struct PlayerButtons: View {
    private let mainButtonSize: Double = 55
    private let otherButtonSize: Double = 45
    @State private var viewModel: PlayerViewModel
    
    init(viewModel: PlayerViewModel) {
        _viewModel = State(initialValue: viewModel)
    }
    
    var body: some View {
        HStack (alignment: .center) {
            Button(action: { viewModel.goTo(next: false, previous: true) }) {
                Image(systemName: "backward.end.fill")
            }
            .disabled(!viewModel.isPlayingOrPaused)
            //.glassEffect()
            .frame(maxWidth: otherButtonSize, maxHeight: otherButtonSize)
            .clipShape(Circle())

            Spacer()

            // Play/Pause with circular progress when active
            Button(action: { viewModel.togglePlayBack() }) {
                ZStack {
                    if viewModel.isPlayingOrPaused {
                        ProgressView(value: viewModel.playedPercentage, total: 100)
                            .progressViewStyle(.circular)
                            .tint(.white)
                    }
                    Image(systemName: viewModel.isPlaying ? "pause.fill" : "play.fill")
                }
            }
            .disabled(!viewModel.isPlayingOrPaused)
            //.glassEffect()
            .controlSize(.large)
            .clipShape(Circle())

            Spacer()

            Button(action: { viewModel.goTo(next: true, previous: false) }) {
                Image(systemName: "forward.end.fill")
            }
            .disabled(!viewModel.isPlayingOrPaused)
            //.glassEffect()
            .frame(maxWidth: otherButtonSize, maxHeight: otherButtonSize)
            .clipShape(Circle())
        }
    }
}

struct PlayerImage: View {
    private let imageSize: Double = 80;
    @State private var viewModel: PlayerViewModel
    
    init(viewModel: PlayerViewModel) {
        _viewModel = State(initialValue: viewModel)
    }
    
    var body: some View {
        if let urlStr = viewModel.playedFile?.thumbnailUrl, let url = URL(string: urlStr) {
            AsyncImage(url: url) { phase in
                switch phase {
                case .empty:
                    ProgressView()
                case .success(let image):
                    image
                        .resizable()
                        .aspectRatio(contentMode: .fill)
                case .failure:
                    Image(systemName: "photo")
                        .resizable()
                        .aspectRatio(contentMode: .fill)
                @unknown default:
                    Image(systemName: "photo")
                        .resizable()
                        .aspectRatio(contentMode: .fill)
                }
            }
            .frame(width: imageSize, height: imageSize)
            .clipShape(RoundedRectangle(cornerRadius: 8))
        } else {
            Image(systemName: "photo")
                .resizable()
                .aspectRatio(contentMode: .fill)
                .frame(width: imageSize, height: imageSize)
        }
    }
}

#Preview {
    PlayerView(container: AppContainer())
}
