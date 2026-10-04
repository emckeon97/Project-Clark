import SwiftUI

/// Night-flight rendering for SwiftUI Canvas: moonlit sky, drifting riverboat,
/// riveted smokestack pairs, the flapping toon, pier deck, score, get-ready.
enum FlappyRenderer {

    /// Deterministic pseudo-random in 0...1 (stable starfield).
    private static func hash(_ i: Int) -> CGFloat {
        let x = sin(CGFloat(i) * 12.9898) * 43758.5453
        return x - floor(x)
    }

    static func draw(engine: FlappyEngine, sprite: UIImage?, tSec: CGFloat,
                     in gc: inout GraphicsContext, size: CGSize) {
        let w = size.width
        let h = size.height
        let groundY = engine.groundY

        // ---- night sky ----
        gc.fill(
            Path(CGRect(x: 0, y: 0, width: w, height: groundY)),
            with: .linearGradient(
                Gradient(colors: [Color(red: 0x05/255, green: 0x07/255, blue: 0x0F/255),
                                  Color(red: 0x0D/255, green: 0x13/255, blue: 0x30/255)]),
                startPoint: CGPoint(x: 0, y: 0),
                endPoint: CGPoint(x: 0, y: groundY)
            )
        )

        // ---- stars ----
        for i in 0..<42 {
            let sx = hash(i * 3 + 1) * w
            let sy = hash(i * 3 + 2) * groundY * 0.85
            let sr = 0.8 + hash(i * 3 + 3) * 1.4
            let tw = 0.35 + 0.65 * abs(sin(tSec * (1 + hash(i * 7) * 2) + CGFloat(i)))
            gc.fill(
                Path(ellipseIn: CGRect(x: sx - sr, y: sy - sr, width: sr * 2, height: sr * 2)),
                with: .color(.white.opacity(0.7 * tw))
            )
        }

        // ---- moon + halo ----
        let moon = CGPoint(x: w * 0.80, y: h * 0.13)
        let moonR = h * 0.042
        let cream = ClarkTheme.cream
        gc.fill(Path(ellipseIn: CGRect(x: moon.x - moonR * 2.6, y: moon.y - moonR * 2.6,
                                       width: moonR * 5.2, height: moonR * 5.2)),
                with: .color(cream.opacity(0.10)))
        gc.fill(Path(ellipseIn: CGRect(x: moon.x - moonR * 1.7, y: moon.y - moonR * 1.7,
                                       width: moonR * 3.4, height: moonR * 3.4)),
                with: .color(cream.opacity(0.16)))
        gc.fill(Path(ellipseIn: CGRect(x: moon.x - moonR, y: moon.y - moonR,
                                       width: moonR * 2, height: moonR * 2)),
                with: .color(Color(red: 0xF2/255, green: 0xEB/255, blue: 0xD8/255)))

        // ---- distant riverboat silhouette, drifting ----
        let boatW: CGFloat = 150
        let boatX = w - (tSec * 14).truncatingRemainder(dividingBy: w + boatW * 2) - boatW
        let boatY = groundY - 46
        let hull = Color(red: 0x0B/255, green: 0x0E/255, blue: 0x18/255)
        gc.fill(Path(roundedRect: CGRect(x: boatX, y: boatY, width: boatW, height: 26),
                     cornerRadius: 6), with: .color(hull))
        gc.fill(Path(CGRect(x: boatX + boatW * 0.3, y: boatY - 20, width: boatW * 0.4, height: 20)),
                with: .color(hull))
        for i in 0..<4 {
            let wx = boatX + boatW * (0.18 + CGFloat(i) * 0.21)
            gc.fill(Path(ellipseIn: CGRect(x: wx - 3, y: boatY + 10, width: 6, height: 6)),
                    with: .color(ClarkTheme.gold.opacity(0.75)))
        }

        // ---- smokestack pairs ----
        for s in engine.stacks {
            let sx = s.x
            let sw = FlappyEngine.pipeW
            let gapTop = s.gapY - s.gap / 2
            let gapBot = s.gapY + s.gap / 2
            drawStack(in: &gc, x: sx, w: sw, top: 0, bottom: gapTop, isTop: true)
            drawStack(in: &gc, x: sx, w: sw, top: gapBot, bottom: groundY, isTop: false)
            // lantern glow marking the gap
            let lamp = CGPoint(x: sx + sw / 2, y: 0)
            for (ly, alpha) in [(gapTop - 4, 0.28), (gapBot + 4, 0.28)] as [(CGFloat, Double)] {
                gc.fill(Path(ellipseIn: CGRect(x: lamp.x - 16, y: ly - 16, width: 32, height: 32)),
                        with: .color(ClarkTheme.gold.opacity(alpha)))
                gc.fill(Path(ellipseIn: CGRect(x: lamp.x - 5, y: ly - 5, width: 10, height: 10)),
                        with: .color(Color(red: 0xE8/255, green: 0xC2/255, blue: 0x5A/255)))
            }
        }

        // ---- pier deck (the ground) ----
        gc.fill(Path(CGRect(x: 0, y: groundY, width: w, height: h - groundY)),
                with: .color(Color(red: 0x24/255, green: 0x1C/255, blue: 0x12/255)))
        gc.fill(Path(CGRect(x: 0, y: groundY, width: w, height: 10)),
                with: .color(Color(red: 0x2E/255, green: 0x25/255, blue: 0x17/255)))
        var path = Path()
        path.move(to: CGPoint(x: 0, y: groundY + 1))
        path.addLine(to: CGPoint(x: w, y: groundY + 1))
        gc.stroke(path, with: .color(ClarkTheme.gold.opacity(0.5)), lineWidth: 2)
        // plank seams
        let seamMod = (engine.stacks.first?.x ?? 0).truncatingRemainder(dividingBy: 48)
        var sxp = -seamMod
        while sxp < w {
            var sp = Path()
            sp.move(to: CGPoint(x: sxp, y: groundY + 10))
            sp.addLine(to: CGPoint(x: sxp, y: h))
            gc.stroke(sp, with: .color(.black.opacity(0.35)), lineWidth: 2)
            sxp += 48
        }

        // ---- the toon ----
        let bx = engine.birdX
        let by = engine.birdY
        let birdPx: CGFloat = 64
        let pulse = 1 + 0.13 * exp(-engine.flapT * 7)
        gc.saveGState()
        gc.translateBy(x: bx, y: by)
        gc.rotate(by: .degrees(engine.rotation))
        gc.scaleBy(x: pulse, y: pulse)
        if let sprite {
            let iw = sprite.size.width
            let ih = sprite.size.height
            let dh = birdPx
            let dw = birdPx * iw / ih
            gc.draw(Image(uiImage: sprite),
                    in: CGRect(x: -dw / 2, y: -dh / 2, width: dw, height: dh))
        } else {
            // fallback round bird
            gc.fill(Path(ellipseIn: CGRect(x: -birdPx * 0.42, y: -birdPx * 0.42,
                                            width: birdPx * 0.84, height: birdPx * 0.84)),
                    with: .color(Color(red: 0xF2/255, green: 0xEB/255, blue: 0xD8/255)))
            gc.fill(Path(ellipseIn: CGRect(x: 4, y: -10, width: 8, height: 8)),
                    with: .color(.black))
        }
        gc.restoreGState()

        // ---- score ----
        let scoreText = Text("\(engine.score)")
            .font(.system(size: 64, weight: .black, design: .serif))
        gc.draw(scoreText.foregroundColor(.black.opacity(0.6)),
                at: CGPoint(x: w / 2 + 3, y: 113), anchor: .top)
        gc.draw(scoreText.foregroundColor(ClarkTheme.cream),
                at: CGPoint(x: w / 2, y: 110), anchor: .top)

        // ---- get ready ----
        if !engine.started && !engine.gameOver {
            let blink = 0.65 + 0.35 * sin(tSec * 5)
            gc.draw(Text("GET READY!")
                .font(.system(size: 30, weight: .black, design: .serif))
                .tracking(3)
                .foregroundColor(ClarkTheme.gold.opacity(blink)),
                at: CGPoint(x: w / 2, y: h * 0.30), anchor: .top)
            gc.draw(Text("TAP TO FLAP")
                .font(.system(size: 18, weight: .bold, design: .serif))
                .tracking(2)
                .foregroundColor(ClarkTheme.cream.opacity(0.85 * blink)),
                at: CGPoint(x: w / 2, y: h * 0.30 + 52), anchor: .top)
        }

        // ---- film vignette ----
        gc.fill(
            Path(CGRect(x: 0, y: 0, width: w, height: h)),
            with: .radialGradient(
                Gradient(colors: [.clear, .black.opacity(0.38)]),
                center: CGPoint(x: w / 2, y: h / 2),
                startRadius: 0,
                endRadius: max(w, h) * 0.62
            )
        )
    }

    /// One riveted iron smokestack slab, with a cap lip on the gap end.
    private static func drawStack(in gc: inout GraphicsContext,
                                  x: CGFloat, w: CGFloat,
                                  top: CGFloat, bottom: CGFloat, isTop: Bool) {
        guard bottom > top else { return }
        let iron = Color(red: 0x1A/255, green: 0x1D/255, blue: 0x24/255)
        gc.fill(Path(CGRect(x: x, y: top, width: w, height: bottom - top)), with: .color(iron))
        // vertical highlight
        gc.fill(Path(CGRect(x: x + w * 0.14, y: top, width: w * 0.12, height: bottom - top)),
                with: .color(Color(red: 0x2E/255, green: 0x34/255, blue: 0x40/255).opacity(0.8)))
        // gold bands + rivets
        var y = top + 26
        while y < bottom - 10 {
            gc.fill(Path(CGRect(x: x, y: y, width: w, height: 7)),
                    with: .color(ClarkTheme.gold.opacity(0.85)))
            var rx = x + 10
            while rx < x + w - 6 {
                gc.fill(Path(ellipseIn: CGRect(x: rx - 2.6, y: y + 0.9, width: 5.2, height: 5.2)),
                        with: .color(Color(red: 0x0B/255, green: 0x0D/255, blue: 0x12/255)))
                rx += 18
            }
            y += 64
        }
        // cap lip on the gap end
        let lipH: CGFloat = 15
        let lipY = isTop ? bottom - lipH : top
        gc.fill(Path(CGRect(x: x - 7, y: lipY, width: w + 14, height: lipH)),
                with: .color(Color(red: 0x0B/255, green: 0x0D/255, blue: 0x12/255)))
        gc.fill(Path(CGRect(x: x - 7, y: isTop ? lipY : lipY + lipH - 3, width: w + 14, height: 3)),
                with: .color(ClarkTheme.gold.opacity(0.7)))
    }
}
