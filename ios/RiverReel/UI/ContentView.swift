import SwiftUI

/// App navigation: menu -> game -> game over.
struct ContentView: View {
    @AppStorage("clark.selected") private var selectedID = "popeye"

    enum Screen: Hashable {
        // id makes every run a unique destination so FLY AGAIN always builds
        // a fresh GameView (fresh engine + timer). Without it, path = [.game]
        // reuses the dead view and the game appears frozen.
        case game(id: UUID)
        case gameOver(score: Int)
    }

    @State private var path: [Screen] = []

    var body: some View {
        NavigationStack(path: $path) {
            MenuView {
                path.append(.game(id: UUID()))
            }
            .navigationDestination(for: Screen.self) { screen in
                switch screen {
                case .game:
                    GameView(characterID: selectedID) { score in
                        path.append(.gameOver(score: score))
                    }
                    .navigationBarBackButtonHidden(true)
                case .gameOver(let score):
                    GameOverView(
                        score: score,
                        onRetry: { path = [.game(id: UUID())] },
                        onMenu: { path = [] }
                    )
                    .navigationBarBackButtonHidden(true)
                }
            }
        }
    }
}
