import SwiftUI

struct PlayerSheet: View {
    @Bindable var viewModel: PlayerViewModel
    @State private var volume: Double = 0
    @State private var skipValue: Double = 30

    var body: some View {
        ScrollView {
            VStack(spacing: 12) {
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
                            viewModel.skipSeconds(-skipValue)
                        }) {
                            Image(systemName: "backward")
                        }
                        
                        Picker(selection: $skipValue, label: EmptyView()) {
                            Text("30").tag(30.0)
                            Text("60").tag(60.0)
                            Text("90").tag(90.0)
                        }
                        .pickerStyle(.navigationLink)
                        
                        Button(action: {
                            viewModel.skipSeconds(skipValue)
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
                    VStack(alignment: .leading) {
                        Label("Audio", systemImage: "headphones")
                            .font(.caption2)
                        Picker(selection: Binding(
                            get: { audios.first(where: { $0.isSelected })?.id ?? -1 },
                            set: { id in
                                if let option = audios.first(where: { $0.id == id }) {
                                    viewModel.setFileOptions(option: option)
                                }
                            }
                        ), label: EmptyView()) {
                            ForEach(audios) { option in
                                Text(option.text)
                                    .tag(option.id)
                                    .disabled(!option.isEnabled)
                            }
                        }
                        .pickerStyle(.navigationLink)
                    }
                }

                // Quality Picker
                if let qualities = viewModel.playedFile?.currentFileQualities, !qualities.isEmpty {
                    VStack(alignment: .leading) {
                        Label("Quality", systemImage: "video")
                            .font(.caption2)
                        Picker(selection: Binding(
                            get: { qualities.first(where: { $0.isSelected })?.id ?? -1 },
                            set: { id in
                                if let option = qualities.first(where: { $0.id == id }) {
                                    viewModel.setFileOptions(option: option)
                                }
                            }
                        ), label: EmptyView()) {
                            ForEach(qualities) { option in
                                Text(option.text)
                                    .tag(option.id)
                                    .disabled(!option.isEnabled)
                            }
                        }
                        .pickerStyle(.navigationLink)
                    }
                }

                // Subtitles Picker
                if let subtitles = viewModel.playedFile?.currentFileSubTitles, !subtitles.isEmpty {
                    VStack(alignment: .leading) {
                        Label("Subtitles", systemImage: "text.document")
                            .font(.caption2)
                        Picker(selection: Binding(
                            get: { subtitles.first(where: { $0.isSelected })?.id ?? -1 },
                            set: { id in
                                if let option = subtitles.first(where: { $0.id == id }) {
                                    viewModel.setFileOptions(option: option)
                                }
                            }
                        ), label: EmptyView()) {
                            ForEach(subtitles) { option in
                                Text(option.text)
                                    .tag(option.id)
                                    .disabled(!option.isEnabled)
                            }
                        }
                        .pickerStyle(.navigationLink)
                    }
                }
            }
            .padding(.horizontal)
        }
    }
}


#Preview {
    PlayerSheet(viewModel: AppContainer().playerViewModel)
}
