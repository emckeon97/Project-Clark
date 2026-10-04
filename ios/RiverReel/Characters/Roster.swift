import SwiftUI

/// The playable toons — the Project Delta sprite roster.
struct Toon: Identifiable {
    let id: String
    let name: String
    var identifier: String { id }
}

let roster: [Toon] = [
    Toon(id: "popeye", name: "Popeye"),
    Toon(id: "felix", name: "Felix the Cat"),
    Toon(id: "oswald", name: "Oswald"),
    Toon(id: "koko", name: "Koko"),
    Toon(id: "bimbo", name: "Bimbo"),
    Toon(id: "pooh", name: "Winnie the Pooh"),
    Toon(id: "olive", name: "Olive Oyl"),
    Toon(id: "bosko", name: "Bosko"),
    Toon(id: "pete", name: "Peg-Leg Pete"),
]

/// The sprite image for a toon, or nil when the art isn't bundled.
func spriteImage(for id: String) -> Image? {
    guard let ui = UIImage(named: id) else { return nil }
    return Image(uiImage: ui)
}
