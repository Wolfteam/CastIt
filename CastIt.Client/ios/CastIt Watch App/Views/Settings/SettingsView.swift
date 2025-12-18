
import SwiftUI

struct SettingsView: View {
    @State private var viewModel: SettingsViewModel
    init(container: DependencyContainer) {
        _viewModel = State(initialValue: container.settingsViewModel)
    }
    @AppStorage("serverUrl") private var serverUrl: String = ""

    var body: some View {
        NavigationView {
            Form {
                Section(header: Text("Server")) {
                    TextField("Server URL", text: $serverUrl)
                        .textContentType(.URL)
                    //.keyboardType(.URL)
                }
            }
        }
        .navigationTitle("Settings")
    }
}

#Preview {
    SettingsView(container: AppContainer())
}
