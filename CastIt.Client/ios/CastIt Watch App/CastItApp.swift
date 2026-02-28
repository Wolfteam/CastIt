//
//  castitApp.swift
//  castit Watch App
//
//  Created by Efrain Bastidas on 16/12/25.
//

import SwiftUI

@main
struct Watch_App: App {
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
