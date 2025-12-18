import SwiftUI
import Combine

struct PlayerView: View {
    private let container: DependencyContainer
    @State private var viewModel: PlayerViewModel
    @State private var backgroundColors: [Color] = []
    @State private var showMore: Bool = false
    @State private var showServerAlert: Bool = false
    @State private var serverAlertText: String = ""

    init(container: DependencyContainer) {
        self.container = container
        _viewModel = State(initialValue: container.playerViewModel)
    }

    var body: some View {
        NavigationView {
            VStack (alignment: .center) {
                VStack (alignment: .center) {
                    PlayerImage(viewModel: viewModel)
                        .onTapGesture {
                            showMore.toggle()
                        }
                        .sheet(isPresented: $showMore) {
                            Button(role: .destructive, action: {
                                viewModel.stop()
                                showMore = false
                            }) {
                                Label("Stop", systemImage: "stop.fill")
                            }
                            .disabled(!viewModel.isPlayingOrPaused)
                        }

                    if (viewModel.status?.playList?.name != nil) {
                        Text(viewModel.status?.playList?.name ?? "")
                            .font(.headline)
                            .lineLimit(1)
                    }
                    
                    if (viewModel.status?.playedFile?.name != nil) {
                        Text(viewModel.status?.playedFile?.name ?? "")
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
            .padding(.horizontal, 8)
            .frame(maxWidth: .infinity, maxHeight: .infinity)
            .background(playerBackground)
            // Listen for server messages to show alerts
            .onReceive(container.signalRService.onServerMessage.receive(on: RunLoop.main)) { message in
                serverAlertText = message.localizedDescription
                showServerAlert = true
            }
            // Present alert
            .alert("Server message", isPresented: $showServerAlert) {
                Button("Close", role: .cancel) { showServerAlert = false }
            } message: {
                Text(serverAlertText)
            }
            .onChange(of: viewModel.status?.playedFile?.thumbnailUrl) { _, newValue in
                guard viewModel.isPlayingOrPaused, let s = newValue, let url = URL(string: s) else {
                    backgroundColors = []
                    return
                }
                Task.detached(priority: .background) {
                    if let (data, _) = try? await URLSession.shared.data(from: url) {
                        let colors = await DominantColorsExtractor.dominantColors(from: data)
                        await MainActor.run { backgroundColors = colors }
                    } else {
                        await MainActor.run { backgroundColors = [] }
                    }
                }
            }
            //.ignoresSafeArea(.all, edges: .bottom)
        }
    }

    private var playerBackground: some View {
        Group {
            if viewModel.isPlayingOrPaused, backgroundColors.count >= 1 {
                let colors = backgroundColors.count == 1 ? [backgroundColors[0], backgroundColors[0].opacity(0.7)] : backgroundColors
                LinearGradient(colors: colors, startPoint: .topLeading, endPoint: .bottomTrailing)
                    .ignoresSafeArea()
                    .opacity(0.35)
            } else {
                Color.clear
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
                            .tint(.accentColor)
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
        if let urlStr = viewModel.status?.playedFile?.thumbnailUrl, let url = URL(string: urlStr) {
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
