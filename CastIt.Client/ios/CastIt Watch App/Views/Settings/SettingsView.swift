import SwiftUI

struct SettingsView: View {
    @Bindable private var viewModel: SettingsViewModel
    
    init(container: DependencyContainer) {
        _viewModel = Bindable(container.settingsViewModel)
    }

    var body: some View {
        NavigationStack {
            Form {
                Section(header: Text("Server")) {
                    TextField("Server URL", text: $viewModel.serverUrl)
                        .textContentType(.URL)
                        .autocorrectionDisabled()

                    Button(action: {
                        viewModel.applyServerUrl()
                    }) {
                        HStack {
                            Text(viewModel.isLoading ? "Connecting" : viewModel.isConnected && !viewModel.isLoading ? "Connected" : "Not connected")
                                .font(.caption)
                                .foregroundColor(viewModel.isConnected ? .green : !viewModel.isConnected && !viewModel.isLoading ? .red : nil)

                            Spacer()

                            if viewModel.isLoading {
                                ProgressView()
                                    .frame(width: 40, alignment: .trailing)
                            }
                        }
                    }
                    .onTapGesture {
                        viewModel.applyServerUrl()
                    }
                    .disabled(viewModel.serverUrl.isEmpty || viewModel.isLoading)
                }

                if viewModel.isConnected {
                    Section(header: Text("Video Settings")) {
                        Picker("Video Scale", selection: $viewModel.videoScale) {
                            Text("Original").tag(VideoScale.original)
                            Text("HD").tag(VideoScale.hd)
                            Text("Full HD").tag(VideoScale.fullHd)
                        }
                        .onChange(of: viewModel.videoScale) {
                            viewModel.updateSettings()
                        }

                        Picker("Web Video Quality", selection: $viewModel.webVideoQuality) {
                            Text("144p").tag(144)
                            Text("240p").tag(240)
                            Text("360p").tag(360)
                            Text("480p").tag(480)
                            Text("720p").tag(720)
                            Text("1080p").tag(1080)
                        }
                        .onChange(of: viewModel.webVideoQuality) {
                            viewModel.updateSettings()
                        }
                    }

                    Section(header: Text("Options")) {
                        Toggle("Start from beginning", isOn: $viewModel.startFilesFromTheStart)
                            .onChange(of: viewModel.startFilesFromTheStart) {
                                viewModel.updateSettings()
                            }
                        Toggle("Play next automatically", isOn: $viewModel.playNextFileAutomatically)
                            .onChange(of: viewModel.playNextFileAutomatically) {
                                viewModel.updateSettings()
                            }
                        Toggle("Force video transcode", isOn: $viewModel.forceVideoTranscode)
                            .onChange(of: viewModel.forceVideoTranscode) {
                                viewModel.updateSettings()
                            }
                        Toggle("Force audio transcode", isOn: $viewModel.forceAudioTranscode)
                            .onChange(of: viewModel.forceAudioTranscode) {
                                viewModel.updateSettings()
                            }
                        Toggle("Hardware acceleration", isOn: $viewModel.enableHardwareAcceleration)
                            .onChange(of: viewModel.enableHardwareAcceleration) {
                                viewModel.updateSettings()
                            }
                    }
                }
            }
            .navigationTitle("Settings")
        }
    }
}

#Preview {
    SettingsView(container: AppContainer())
}
