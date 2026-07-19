import SwiftUI

struct AddEditEndpointView: View {
    let viewModel: PlayerViewModel
    let endpoint: Receiver?

    @Environment(\.dismiss) private var dismiss
    @State private var name: String
    @State private var host: String
    @State private var port: String

    init(viewModel: PlayerViewModel, endpoint: Receiver?) {
        self.viewModel = viewModel
        self.endpoint = endpoint
        _name = State(initialValue: endpoint?.friendlyName ?? "")
        _host = State(initialValue: endpoint?.host ?? "")
        _port = State(initialValue: endpoint.map { String($0.port) } ?? "8009")
    }

    private var isValid: Bool {
        let trimmedName = name.trimmingCharacters(in: .whitespacesAndNewlines)
        let trimmedHost = host.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmedName.isEmpty, !trimmedHost.isEmpty else { return false }
        guard let portValue = Int(port), (1...65535).contains(portValue) else { return false }
        return true
    }

    var body: some View {
        NavigationStack {
            Form {
                TextField("Name", text: $name)
                    .autocorrectionDisabled()
                TextField("Host", text: $host)
                    .autocorrectionDisabled()
                TextField("Port", text: $port)
                    .autocorrectionDisabled()
            }
            .navigationTitle(endpoint == nil ? "Add device" : "Edit device")
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("Cancel") {
                        dismiss()
                    }
                }
                ToolbarItem(placement: .confirmationAction) {
                    Button("Save") {
                        save()
                    }
                    .disabled(!isValid)
                }
            }
        }
    }

    private func save() {
        guard isValid, let portValue = Int(port) else { return }
        let trimmedName = name.trimmingCharacters(in: .whitespacesAndNewlines)
        let trimmedHost = host.trimmingCharacters(in: .whitespacesAndNewlines)

        if let endpoint {
            if let endpointId = Int(endpoint.id) {
                viewModel.updateCustomEndpoint(
                    id: endpointId,
                    name: trimmedName,
                    host: trimmedHost,
                    port: portValue
                )
            }
        } else {
            viewModel.addCustomEndpoint(name: trimmedName, host: trimmedHost, port: portValue)
        }
        dismiss()
    }
}
