import SwiftUI

struct DevicesView: View {
    //A single item-driven sheet: stacking two `.sheet` modifiers on one view is unreliable.
    private enum EndpointForm: Identifiable {
        case add
        case edit(Receiver)

        var id: String {
            switch self {
            case .add: return "add"
            case .edit(let device): return device.id
            }
        }
    }

    @Bindable private var viewModel: PlayerViewModel
    @State private var activeForm: EndpointForm?

    init(container: DependencyContainer) {
        _viewModel = Bindable(container.playerViewModel)
    }

    var body: some View {
        //Note: do NOT put a toolbar item at `.topBarLeading` — on watchOS that replaces the
        //automatic back button, which strands the user on this pushed view. Refresh lives in
        //the list content instead.
        List {
            if viewModel.devices.isEmpty {
                Text("No devices found")
                    .font(.caption)
                    .foregroundColor(.secondary)
            } else {
                ForEach(viewModel.devices) { device in
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
                                activeForm = .edit(device)
                            } label: {
                                Label("Edit", systemImage: "pencil")
                            }
                            .tint(.blue)
                        }
                    }
                }
            }

            Button {
                viewModel.refreshDevices()
            } label: {
                Label("Refresh", systemImage: "arrow.clockwise")
            }
        }
        .navigationTitle("Devices")
        .toolbar {
            ToolbarItem(placement: .topBarTrailing) {
                Button {
                    activeForm = .add
                } label: {
                    Image(systemName: "plus")
                }
            }
        }
        .sheet(item: $activeForm) { form in
            switch form {
            case .add:
                AddEditEndpointView(viewModel: viewModel, endpoint: nil)
            case .edit(let device):
                AddEditEndpointView(viewModel: viewModel, endpoint: device)
            }
        }
    }
}

#Preview {
    NavigationStack {
        DevicesView(container: AppContainer())
    }
}
