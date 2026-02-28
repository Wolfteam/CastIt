import SwiftUI

struct NotConnectedView: View {
    var body: some View {
        VStack (alignment: .center) {
            Text("Not connected")
                .font(.headline)
                .foregroundColor(.red)
            Text("Please configure the server URL in Settings")
                .font(.caption)
                .multilineTextAlignment(.center)
                .padding()
        }
    }
}

#Preview {
    NotConnectedView()
}
