//
//  ContentView.swift
//  castIt Watch App
//
//  Created by Efrain Bastidas on 14/12/25.
//

import SwiftUI
import Observation

struct ContentView: View {
    private let container: DependencyContainer
    
    @Environment(\.scenePhase) private var scenePhase
    @Bindable private var router: AppRouter
    
    init(container: DependencyContainer) {
        self.container = container
        self._router = Bindable(wrappedValue: container.router)
    }
    var body: some View {
        TabView(selection: $router.selectedTab) {
            PlayerView(container: container)
                .tabItem {
                    Label("Player", systemImage: "play.circle")
                }
                .tag(AppRouter.Tab.player)
            PlaylistsView(container: container)
                .tabItem {
                    Label("Playlists", systemImage: "list.bullet")
                }
                .tag(AppRouter.Tab.playlists)
            SettingsView(container: container)
                .tabItem {
                    Label("Settings", systemImage: "gear")
                }
                .tag(AppRouter.Tab.settings)
        }
        .tabViewStyle(.verticalPage)
        .onAppear {
            if container.settingsViewModel.serverUrl.isEmpty {
                router.selectedTab = .settings
            }
        }
        .onChange(of: scenePhase) { _, phase in
            switch phase {
            case .inactive, .background:
                container.signalRService.disconnect()
            case .active:
                if !container.settingsViewModel.serverUrl.isEmpty {
                    container.signalRService.connect()
                }
            @unknown default:
                break
            }
        }
    }
}

#Preview {
    ContentView(container: AppContainer())
}
