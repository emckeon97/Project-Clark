import SwiftUI

/// Silent-film "THE END" card with score, best, and retry.
struct GameOverView: View {
    let score: Int
    var onRetry: () -> Void
    var onMenu: () -> Void

    @AppStorage("clark.best") private var best = 0

    private var isRecord: Bool { score > best }

    var body: some View {
        ZStack {
            ClarkTheme.ink.ignoresSafeArea()
            VStack(spacing: 0) {
                MarqueeLights(count: 14)
                    .padding(.bottom, 12)
                Text("THE END")
                    .font(.system(size: 40, weight: .black, design: .serif))
                    .tracking(4)
                    .foregroundColor(ClarkTheme.cream)
                if isRecord && score > 0 {
                    Text("\u{2605} NEW REEL RECORD! \u{2605}")
                        .font(.system(size: 15, weight: .bold, design: .serif))
                        .tracking(2)
                        .foregroundColor(ClarkTheme.gold)
                        .padding(.top, 8)
                }
                Text("SCORE")
                    .font(.system(size: 13, weight: .bold, design: .serif))
                    .tracking(4)
                    .foregroundColor(ClarkTheme.cream.opacity(0.6))
                    .padding(.top, 18)
                Text("\(score)")
                    .font(.system(size: 64, weight: .black, design: .serif))
                    .foregroundColor(ClarkTheme.gold)
                Text("BEST \(isRecord ? score : best)")
                    .font(.system(size: 17, weight: .bold, design: .serif))
                    .tracking(2)
                    .foregroundColor(ClarkTheme.cream.opacity(0.75))
                    .padding(.top, 4)

                Button("FLY AGAIN", action: onRetry)
                    .buttonStyle(DeltaButtonStyle())
                    .padding(.top, 24)
                Button("LOBBY", action: onMenu)
                    .buttonStyle(DeltaSecondaryButtonStyle())
                    .padding(.top, 10)
            }
            .padding(28)
            .background(ClarkTheme.ink)
            .border(ClarkTheme.gold, width: 2)
            .padding(.horizontal, 40)
        }
        .onAppear {
            if isRecord { best = score }
        }
    }
}
