import Foundation
import Observation

@Observable
class AppSettings {
    static let shared = AppSettings()
    
    private let defaults = UserDefaults.standard
    
    private enum Keys {
        static let serverUrl = "serverUrl"
        static let playerSkipSeconds = "player_skip_seconds"
    }
    
    var serverUrl: String {
        get { defaults.string(forKey: Keys.serverUrl) ?? "" }
        set { defaults.set(newValue, forKey: Keys.serverUrl) }
    }
    
    var playerSkipSeconds: Double {
        get {
            let value = defaults.double(forKey: Keys.playerSkipSeconds)
            return value == 0 ? 30 : value
        }
        set { defaults.set(newValue, forKey: Keys.playerSkipSeconds) }
    }
    
    private init() {}
}
