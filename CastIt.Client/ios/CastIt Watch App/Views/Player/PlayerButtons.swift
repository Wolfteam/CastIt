import SwiftUI

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
