import SwiftUI

struct DevicesView: View {
    @Bindable private var viewModel: PlayerViewModel
    @State private var endpointToEdit: Receiver?
    @State private var showAddForm = false

    init(container: DependencyContainer) {
        _viewModel = Bindable(container.playerViewModel)
    }

    var body: some View {
        Group {
            if viewModel.devices.isEmpty {
                Text("No devices found")
                    .font(.caption)
                    .foregroundColor(.secondary)
            } else {
                List(viewModel.devices) { device in
                    Button {
                        viewModel.connectToDevice(device)
                    } label: {
                        HStack {
                            VStack(alignment: .leading) {
                                Text(device.friendlyName)
                                    .font(.headline)
                                    .lineLimit(1)
                                Text("\(device.host):\(device.port)")
                                    .font(.caption)
                                    .foregroundColor(.secondary)
                                    .lineLimit(1)
                            }
                            Spacer()
                            Image(systemName: device.isConnected ? "wifi" : "wifi.slash")
                                .foregroundColor(device.isConnected ? .green : .secondary)
                        }
                        .contentShape(Rectangle())
                    }
                    .buttonStyle(.plain)
                    .swipeActions(edge: .trailing, allowsFullSwipe: false) {
                        if device.isCustom {
                            Button(role: .destructive) {
                                if let id = Int(device.id) {
                                    viewModel.deleteCustomEndpoint(id: id)
                                }
                            } label: {
                                Label("Delete", systemImage: "trash")
                            }
                            .tint(.red)

                            Button {
                                endpointToEdit = device
                            } label: {
                                Label("Edit", systemImage: "pencil")
                            }
                            .tint(.blue)
                        }
                    }
                }
            }
        }
        .navigationTitle("Devices")
        .toolbar {
            ToolbarItem(placement: .topBarTrailing) {
                Button {
                    showAddForm = true
                } label: {
                    Image(systemName: "plus")
                }
            }
            ToolbarItem(placement: .topBarLeading) {
                Button {
                    viewModel.refreshDevices()
                } label: {
                    Image(systemName: "arrow.clockwise")
                }
            }
        }
        .sheet(isPresented: $showAddForm) {
            AddEditEndpointView(viewModel: viewModel, endpoint: nil)
        }
        .sheet(item: $endpointToEdit) { endpoint in
            AddEditEndpointView(viewModel: viewModel, endpoint: endpoint)
        }
    }
}

#Preview {
    NavigationStack {
        DevicesView(container: AppContainer())
    }
}
