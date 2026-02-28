import SwiftUI

struct PlayerSheet: View {
    @Bindable var viewModel: PlayerViewModel
    @State private var volume: Double = 0

    var body: some View {
        ScrollView {
            VStack(spacing: 12) {
                // Played file, loop and stop buttons
                VStack (alignment: .leading) {
                    Label("Player", systemImage: "play")
                        .font(.caption2)
                    
                    if (viewModel.playedFile != nil) {
                        Text(viewModel.playedFile!.filename)
                            .font(.caption2)
                        
                        HStack(alignment: .center) {
                            Text(viewModel.playedFile!.playedTime)
                                .font(.footnote)
                                .lineLimit(1)
                            Spacer()
                            Text(viewModel.playedFile!.duration)
                                .font(.footnote)
                                .lineLimit(1)
                        }
                    }
               
                    HStack {
                        // Loop Button
                        Button(action: {
                            viewModel.toggleLoop()
                        }) {
                            Label(
                                viewModel.playedFile?.loop ?? false ? "Looping" : "Loop",
                                systemImage: viewModel.playedFile?.loop ?? false ? "repeat.1" : "repeat"
                            )
                        }
                        .tint(viewModel.playedFile?.loop ?? false ? .blue : .gray)
                        
                        // Stop Button
                        Button(role: .destructive, action: {
                            viewModel.stop()
                            viewModel.showMore = false
                        }) {
                            Label("Stop", systemImage: "stop.fill")
                        }
                        .disabled(!viewModel.isPlayingOrPaused)
                    }
                }
                                
                // Skip Seconds Picker
                VStack(alignment: .leading) {
                    Label("Skip seconds", systemImage: "timer")
                        .font(.caption2)
                    HStack {
                        Button(action: {
                            viewModel.skipSeconds(-viewModel.skipValue)
                        }) {
                            Image(systemName: "backward")
                        }
                        
                        Picker(selection: $viewModel.skipValue, label: EmptyView()) {
                            Text("30").tag(30.0)
                            Text("60").tag(60.0)
                            Text("90").tag(90.0)
                        }
                        .pickerStyle(.navigationLink)
                        
                        Button(action: {
                            viewModel.skipSeconds(viewModel.skipValue)
                        }) {
                            Image(systemName: "forward")
                        }
                    }
                }
                
                // Volume Slider
                VStack(alignment: .leading) {
                    Label("Volume", systemImage: "speaker.wave.3.fill")
                        .font(.caption2)
                    Slider(value: $volume, in: 0...1, step: 0.1) {
                        Text("Volume")
                    }
                    .onChange(of: volume) { _, newValue in
                        viewModel.setVolume(newValue)
                    }
                }
                .onAppear {
                    volume = viewModel.player?.volumeLevel ?? 0
                }
                
                // Audio Picker
                if let audios = viewModel.playedFile?.currentFileAudios, !audios.isEmpty {
                    PlayerOptionPicker(
                        title: "Audio",
                        systemImage: "headphones",
                        options: audios,
                        onOptionSelected: { viewModel.setFileOptions(option: $0) }
                    )
                }

                // Quality Picker
                if let qualities = viewModel.playedFile?.currentFileQualities, !qualities.isEmpty {
                    PlayerOptionPicker(
                        title: "Quality",
                        systemImage: "video",
                        options: qualities,
                        onOptionSelected: { viewModel.setFileOptions(option: $0) }
                    )
                }

                // Subtitles Picker
                if let subtitles = viewModel.playedFile?.currentFileSubTitles, !subtitles.isEmpty {
                    PlayerOptionPicker(
                        title: "Subtitles",
                        systemImage: "text.document",
                        options: subtitles,
                        onOptionSelected: { viewModel.setFileOptions(option: $0) }
                    )
                }
            }
            .padding(.horizontal)
        }
    }
}


#Preview {
    PlayerSheet(viewModel: AppContainer().playerViewModel)
}
