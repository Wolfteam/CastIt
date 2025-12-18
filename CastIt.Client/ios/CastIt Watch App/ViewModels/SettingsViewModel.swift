import Foundation
import Observation

@Observable
class SettingsViewModel {
    var settings: ServerAppSettings?

    private let signalRService: SignalRService

    init(signalRService: SignalRService) {
        self.signalRService = signalRService
    }
}