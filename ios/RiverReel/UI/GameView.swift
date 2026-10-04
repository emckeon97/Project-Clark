import SwiftUI

/// The flight itself: tap anywhere to flap.
/// The 60fps loop lives in the engine (started/stopped with the view),
/// so no tick can outlive the game screen.
struct GameView: View {
    let characterID: String
    var onGameOver: (Int) -> Void

    @StateObject private var engine = FlappyEngine()
    @State private var sprite: UIImage?

    var body: some View {
        GeometryReader { geo in
            Canvas { context, size in
                var gc = context
                FlappyRenderer.draw(engine: engine, sprite: sprite, tSec: engine.tSec,
                                    in: &gc, size: size)
            }
            .onTapGesture { engine.flap() }
            .onAppear {
                sprite = UIImage(named: characterID)
                engine.reset(w: geo.size.width, h: geo.size.height)
                MusicManager.shared.play()
                engine.startLoop(
                    sizeProvider: { geo.size },
                    onGameOver: { score in
                        AdManager.shared.gameOverOccurred()
                        onGameOver(score)
                    }
                )
            }
            .onDisappear {
                engine.stopLoop()
            }
        }
        .background(Color.black.ignoresSafeArea())
    }
}
