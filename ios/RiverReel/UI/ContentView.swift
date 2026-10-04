import SwiftUI

/// App navigation: menu -> game -> game over.
struct ContentView: View {
    @AppStorage("clark.selected") private var selectedID = "popeye"

    enum Screen: Hashable {
        case game
        case gameOver(score: Int)
    }

    @State private var path: [Screen] = []

    var body: some View {
        NavigationStack(path: $path) {
            MenuView {
                path.append(.game)
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
                        onRetry: { path = [.game] },
                        onMenu: { path = [] }
                    )
                    .navigationBarBackButtonHidden(true)
                }
            }
        }
    }
}
