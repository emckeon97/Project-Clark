import Foundation

/// Flappy-style flight through the smokestacks. All units are points.
/// Tap to flap, thread the gaps, don't kiss the pier.
/// (Direct port of the Android FlappyEngine — same tuning.)
final class FlappyEngine: ObservableObject {

    struct Stack {
        var x: CGFloat
        var gapY: CGFloat
        var gap: CGFloat
        var scored = false
    }

    // MARK: - Tuning (points)

    static let gravity: CGFloat = 2300
    static let flapVY: CGFloat = -780
    static let maxFall: CGFloat = 1150
    static let pipeW: CGFloat = 86
    static let gapStart: CGFloat = 235
    static let gapMin: CGFloat = 185
    static let speedStart: CGFloat = 270
    static let speedMax: CGFloat = 440
    static let spacing: CGFloat = 370

    // MARK: - State

    @Published var frame = 0          // bumped every update to redraw the Canvas
    @Published var score = 0
    @Published var started = false
    @Published var gameOver = false

    var screenW: CGFloat = 400
    var screenH: CGFloat = 800

    var birdX: CGFloat = 120
    var birdY: CGFloat = 400
    var vy: CGFloat = 0
    var rotation: CGFloat = 0        // degrees, nose-up negative
    var flapT: CGFloat = 99           // seconds since last flap (wing pulse)
    var deadT: CGFloat = 0

    var stacks: [Stack] = []

    let groundH: CGFloat = 96
    var groundY: CGFloat { screenH - groundH }
    let birdR: CGFloat = 26

    private var speed: CGFloat { min(Self.speedMax, Self.speedStart + CGFloat(score) * 3.5) }
    private func gapFor(_ s: Int) -> CGFloat { max(Self.gapMin, Self.gapStart - CGFloat(s) * 1.2) }

    // MARK: - Control

    func reset(w: CGFloat, h: CGFloat) {
        screenW = w; screenH = h
        birdX = w * 0.30
        birdY = h * 0.42
        vy = 0; rotation = 0; flapT = 99
        started = false; gameOver = false; deadT = 0
        score = 0
        stacks = []
        // First pillar starts on-screen (not off the right edge) so the
        // opening isn't a long empty flight — about 0.30·w from the bird.
        var x = w * 0.60
        while x < w + Self.spacing * 3 {
            stacks.append(newStack(x: x))
            x += Self.spacing
        }
    }

    private func newStack(x: CGFloat) -> Stack {
        let lo: CGFloat = 170
        let hi = max(lo + 1, groundY - 170)
        return Stack(
            x: x,
            gapY: lo + CGFloat.random(in: 0...1) * (hi - lo),
            gap: gapFor(score)
        )
    }

    /// Tap!
    func flap() {
        guard !gameOver else { return }
        started = true
        vy = Self.flapVY
        flapT = 0
    }

    // MARK: - Simulation

    func update(dt: CGFloat, size: CGSize) {
        guard dt > 0 else { return }
        // adopt the real canvas size on first frames
        if size.width > 0 && (screenW != size.width || screenH != size.height) {
            reset(w: size.width, h: size.height)
        }
        flapT += dt
        guard started else { frame += 1; return }
        if gameOver {
            // tumble to the pier
            deadT += dt
            vy = min(vy + Self.gravity * dt, Self.maxFall * 1.3)
            birdY += vy * dt
            rotation = min(90, rotation + dt * 260)
            if birdY > groundY - birdR {
                birdY = groundY - birdR
                vy = 0
            }
            frame += 1
            return
        }
        vy = min(vy + Self.gravity * dt, Self.maxFall)
        birdY += vy * dt
        // tilt: nose up on flap, nose down in a fall
        let targetRot = max(-1, min(1, vy / Self.maxFall)) * 38
        rotation += (targetRot - rotation) * min(1, dt * 10)
        // ceiling: bonk, don't die
        if birdY < birdR {
            birdY = birdR
            vy = 0
        }

        let dx = speed * dt
        for i in stacks.indices { stacks[i].x -= dx }
        stacks.removeAll { $0.x < -Self.pipeW - 40 }
        let last = stacks.map(\.x).max() ?? 0
        if last < screenW + 40 { stacks.append(newStack(x: last + Self.spacing)) }

        for i in stacks.indices {
            if !stacks[i].scored && stacks[i].x + Self.pipeW < birdX - birdR {
                stacks[i].scored = true
                score += 1
            }
        }
        checkCollisions()
        frame += 1
    }

    private func checkCollisions() {
        // the pier deck
        if birdY + birdR >= groundY {
            gameOver = true
            return
        }
        // smokestack slabs (circle vs. vertical slabs — corners are forgiving)
        for s in stacks {
            if birdX + birdR < s.x || birdX - birdR > s.x + Self.pipeW { continue }
            let top = s.gapY - s.gap / 2
            let bot = s.gapY + s.gap / 2
            if birdY - birdR < top || birdY + birdR > bot {
                gameOver = true
                return
            }
        }
    }
}
