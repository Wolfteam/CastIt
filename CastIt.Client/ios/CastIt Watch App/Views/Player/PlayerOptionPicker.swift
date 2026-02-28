import SwiftUI

struct PlayerOptionPicker: View {
    let title: String
    let systemImage: String
    let options: [FileItemOptionsResponseDto]
    let onOptionSelected: (FileItemOptionsResponseDto) -> Void

    var body: some View {
        VStack(alignment: .leading) {
            Label(title, systemImage: systemImage)
                .font(.caption2)
            Picker(selection: Binding(
                get: { options.first(where: { $0.isSelected })?.id ?? -1 },
                set: { id in
                    if let option = options.first(where: { $0.id == id }) {
                        onOptionSelected(option)
                    }
                }
            ), label: EmptyView()) {
                ForEach(options) { option in
                    Text(option.text)
                        .tag(option.id)
                        .disabled(!option.isEnabled)
                }
            }
            .pickerStyle(.navigationLink)
        }
    }
}
