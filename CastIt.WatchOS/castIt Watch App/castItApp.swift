import SwiftUI

@main
struct castIt_Watch_AppApp: App {
    private let container: AppContainer

    init() {
        self.container = AppContainer()
    }

    var body: some Scene {
        WindowGroup {
            ContentView(container: container)
        }
    }
}
