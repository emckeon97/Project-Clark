import SwiftUI

/// 1930s cartoon-marquee palette (same house style as the Android port).
enum ClarkTheme {
    static let ink = Color(red: 0x0A / 255, green: 0x0A / 255, blue: 0x0C / 255)
    static let cream = Color(red: 0xF5 / 255, green: 0xEF / 255, blue: 0xE0 / 255)
    static let gold = Color(red: 0xD4 / 255, green: 0xA9 / 255, blue: 0x42 / 255)
    static let red = Color(red: 0xB0 / 255, green: 0x3A / 255, blue: 0x2E / 255)
}

/// A row of marquee chase lights, animated in classic chase sequence.
struct MarqueeLights: View {
    var count: Int = 18

    var body: some View {
        TimelineView(.periodic(from: .now, by: 0.3)) { context in
            let phase = Int(context.date.timeIntervalSinceReferenceDate / 0.3) % 3
            HStack(spacing: 9) {
                ForEach(0..<count, id: \.self) { i in
                    let lit = (i + phase) % 3 != 0
                    Circle()
                        .fill(lit ? ClarkTheme.gold : ClarkTheme.gold.opacity(0.22))
                        .frame(width: 7, height: 7)
                        .shadow(color: lit ? ClarkTheme.gold.opacity(0.9) : .clear, radius: 5)
                }
            }
        }
    }
}

/// Big gold ticket-stub button.
struct DeltaButtonStyle: ButtonStyle {
    func makeBody(configuration: Configuration) -> some View {
        configuration.label
            .font(.system(size: 22, weight: .black, design: .serif))
            .tracking(3)
            .foregroundColor(ClarkTheme.ink)
            .padding(.vertical, 14)
            .frame(maxWidth: .infinity)
            .background(ClarkTheme.gold)
            .cornerRadius(10)
            .opacity(configuration.isPressed ? 0.85 : 1)
    }
}

struct DeltaSecondaryButtonStyle: ButtonStyle {
    func makeBody(configuration: Configuration) -> some View {
        configuration.label
            .font(.system(size: 18, weight: .bold, design: .serif))
            .tracking(2)
            .foregroundColor(ClarkTheme.cream)
            .padding(.vertical, 12)
            .frame(maxWidth: .infinity)
            .overlay(
                RoundedRectangle(cornerRadius: 10)
                    .stroke(ClarkTheme.cream.opacity(0.5), lineWidth: 2)
            )
            .opacity(configuration.isPressed ? 0.85 : 1)
    }
}
