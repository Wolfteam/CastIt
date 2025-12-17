import Foundation
import Observation

@Observable
class PlayerViewModel {
    var status: ServerPlayerStatusResponseDto?
    var settings: ServerAppSettings?
    var devices: [Receiver] = []
    var selectedDevice: Receiver?

    private let signalRService: SignalRService

    init(signalRService: SignalRService) {
        self.signalRService = signalRService
    }

    func togglePlayBack() {
        signalRService.togglePlayBack()
    }

    func goTo(next: Bool, previous: Bool) {
        signalRService.goTo(next: next, previous: previous)
    }
    
    func stop() {
        signalRService.stopPlayBack()
    }
}
