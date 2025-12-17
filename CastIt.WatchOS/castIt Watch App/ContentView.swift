//
//  ContentView.swift
//  castIt Watch App
//
//  Created by Efrain Bastidas on 14/12/25.
//

import SwiftUI

struct ContentView: View {
    private let container: DependencyContainer
    init(container: DependencyContainer) {
        self.container = container
    }
    var body: some View {
        TabView {
            PlayerView(container: container)
                .tabItem {
                    Label("Player", systemImage: "play.circle")
                }
            PlaylistsView(container: container)
                .tabItem {
                    Label("Playlists", systemImage: "list.bullet")
                }
            SettingsView(container: container)
                .tabItem {
                    Label("Settings", systemImage: "gear")
                }
        }
    }
}

#Preview {
    ContentView(container: AppContainer())
}
