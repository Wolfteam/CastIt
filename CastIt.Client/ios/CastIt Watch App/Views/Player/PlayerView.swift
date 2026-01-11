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
                                if viewModel.playedFile != nil {
                                    viewModel.showMore.toggle()
                                }
                            }
                            .sheet(isPresented: $viewModel.showMore) {
                                PlayerSheet(viewModel: viewModel)
                            }

                        if (viewModel.playList?.name != nil) {
                            Text(viewModel.playList?.name ?? "")
                                .font(.headline)
                                .lineLimit(1)
                        }
                        
                        if (viewModel.playedFile?.filename != nil) {
                            Text(viewModel.playedFile!.filename)
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

#Preview {
    PlayerView(container: AppContainer())
}
