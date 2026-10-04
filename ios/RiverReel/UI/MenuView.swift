import SwiftUI

/// 1930s movie-poster marquee menu.
struct MenuView: View {
    var onPlay: () -> Void

    @ObservedObject private var music = MusicManager.shared
    @AppStorage("clark.selected") private var selectedID = "popeye"
    @AppStorage("clark.best") private var best = 0

    var body: some View {
        VStack(spacing: 0) {
            VStack {
                Spacer()
                Text("NOW SHOWING")
                    .font(.system(size: 13, weight: .semibold, design: .serif))
                    .tracking(6)
                    .foregroundColor(ClarkTheme.cream.opacity(0.75))
                MarqueeLights()
                    .padding(.vertical, 10)
                Text("RIVER REEL")
                    .font(.system(size: 46, weight: .black, design: .serif))
                    .tracking(2)
                    .foregroundColor(ClarkTheme.cream)
                Text("A 1930s FLAPPY CARTOON")
                    .font(.system(size: 14, weight: .bold, design: .serif))
                    .tracking(5)
                    .foregroundColor(ClarkTheme.gold)
                    .padding(.top, 8)

                Text("CHOOSE YOUR FLYER")
                    .font(.system(size: 12, weight: .bold, design: .serif))
                    .tracking(3)
                    .foregroundColor(ClarkTheme.cream.opacity(0.7))
                    .padding(.top, 18)

                // character picker
                ScrollView(.horizontal, showsIndicators: false) {
                    HStack(spacing: 12) {
                        ForEach(roster) { toon in
                            let isSel = toon.id == selectedID
                            Button {
                                selectedID = toon.id
                            } label: {
                                ZStack {
                                    Circle()
                                        .fill(ClarkTheme.cream.opacity(0.06))
                                    if let img = spriteImage(for: toon.id) {
                                        img
                                            .resizable()
                                            .scaledToFit()
                                            .frame(width: 60, height: 60)
                                    }
                                }
                                .frame(width: 72, height: 72)
                                .overlay(
                                    Circle()
                                        .stroke(isSel ? ClarkTheme.gold : ClarkTheme.cream.opacity(0.25),
                                                lineWidth: isSel ? 3 : 1)
                                )
                            }
                        }
                    }
                    .padding(.horizontal, 8)
                }
                .padding(.top, 10)

                Text(roster.first(where: { $0.id == selectedID })?.name.uppercased() ?? "")
                    .font(.system(size: 14, weight: .bold, design: .serif))
                    .tracking(2)
                    .foregroundColor(ClarkTheme.cream.opacity(0.9))
                    .padding(.top, 6)

                // best pill
                Text("\u{2605} BEST \(best)")
                    .font(.system(size: 17, weight: .bold, design: .serif))
                    .foregroundColor(ClarkTheme.gold)
                    .padding(.horizontal, 18)
                    .padding(.vertical, 8)
                    .overlay(
                        RoundedRectangle(cornerRadius: 14)
                            .stroke(ClarkTheme.gold.opacity(0.35), lineWidth: 1)
                    )
                    .background(ClarkTheme.cream.opacity(0.07))
                    .cornerRadius(14)
                    .padding(.top, 14)

                // ticket-stub PLAY button
                Button(action: onPlay) {
                    VStack {
                        Text("\u{2605} ADMIT ONE \u{2605}")
                            .font(.system(size: 11, weight: .bold, design: .serif))
                            .tracking(3)
                        Text("PLAY")
                            .font(.system(size: 30, weight: .black, design: .serif))
                            .tracking(5)
                    }
                }
                .buttonStyle(DeltaButtonStyle())
                .padding(.top, 16)

                Text("Music: \"The Entertainer\" by Kevin MacLeod (incompetech.com) \u{00B7} CC BY 4.0")
                    .font(.system(size: 9))
                    .foregroundColor(ClarkTheme.cream.opacity(0.35))
                    .padding(.top, 16)
                Spacer()
            }
            .padding(.horizontal, 24)

            AdBannerView()
        }
        .background(ClarkTheme.ink.ignoresSafeArea())
        .overlay(alignment: .topTrailing) {
            Button { music.toggleMute() } label: {
                Text(music.isMuted ? "\u{1F507}" : "\u{1F50A}")
                    .font(.system(size: 20))
                    .padding(12)
                    .background(ClarkTheme.cream.opacity(0.06))
                    .cornerRadius(12)
            }
            .padding(.top, 54)
            .padding(.trailing, 16)
        }
        .onAppear { MusicManager.shared.play() }
    }
}
