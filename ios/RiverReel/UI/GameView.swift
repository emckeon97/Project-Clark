import SwiftUI

/// The flight itself: tap anywhere to flap.
struct GameView: View {
    let characterID: String
    var onGameOver: (Int) -> Void

    @StateObject private var engine = FlappyEngine()
    @State private var tSec: CGFloat = 0
    @State private var lastDate = Date()
    @State private var overFired = false
    @State private var sprite: UIImage?

    private let timer = Timer.publish(every: 1.0 / 60.0, on: .main, in: .common).autoconnect()

    var body: some View {
        GeometryReader { geo in
            Canvas { context, size in
                var gc = context
                FlappyRenderer.draw(engine: engine, sprite: sprite, tSec: tSec,
                                    in: &gc, size: size)
            }
            .onTapGesture { engine.flap() }
            .onAppear {
                sprite = UIImage(named: characterID)
                engine.reset(w: geo.size.width, h: geo.size.height)
                MusicManager.shared.play()
            }
            .onReceive(timer) { now in
                let dt = min(0.05, now.timeIntervalSince(lastDate))
                lastDate = now
                tSec += dt
                engine.update(dt: dt, size: geo.size)
                if engine.gameOver && engine.deadT > 0.8 && !overFired {
                    overFired = true
                    AdManager.shared.gameOverOccurred()
                    onGameOver(engine.score)
                }
            }
        }
        .background(Color.black.ignoresSafeArea())
        .onDisappear { timer.upstream.connect().cancel() }
    }
}
