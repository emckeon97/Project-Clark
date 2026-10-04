import SwiftUI

@main
struct RiverReelApp: App {
    @Environment(\.scenePhase) private var scenePhase

    var body: some Scene {
        WindowGroup {
            ContentView()
                .preferredColorScheme(.dark)
        }
        .onChange(of: scenePhase) { phase in
            switch phase {
            case .background:
                MusicManager.shared.pause()
            case .active:
                MusicManager.shared.play()
            default:
                break
            }
        }
    }
}
