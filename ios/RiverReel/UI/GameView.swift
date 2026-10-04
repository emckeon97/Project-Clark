import SwiftUI

/// The flight itself: tap anywhere to flap.
/// Driven by TimelineView (a real display-link tick) instead of a manual
/// Timer publisher — the tick is tied to the view's lifecycle, so no timer
/// can outlive the game screen and pile up across retries.
struct GameView: View {
    let characterID: String
    var onGameOver: (Int) -> Void

    @StateObject private var engine = FlappyEngine()
    @State private var tSec: CGFloat = 0
    @State private var lastDate: Date?
    @State private var overFired = false
    @State private var sprite: UIImage?

    var body: some View {
        GeometryReader { geo in
            TimelineView(.animation) { timeline in
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
                .onChange(of: timeline.date) { _, now in
                    let dt: CGFloat
                    if let last = lastDate {
                        dt = min(0.05, now.timeIntervalSince(last))
                    } else {
                        dt = 1.0 / 60.0
                    }
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
        }
        .background(Color.black.ignoresSafeArea())
    }
}
