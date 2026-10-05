# River Reel — Windows Port (.NET MAUI)

A Windows port of **River Reel**, the 1930s flappy-style cartoon game.
Direct port of the iOS version (`ios/`) — same tuning, same art direction,
same Kevin MacLeod ragtime loop.

## Tech

- **.NET 10 MAUI**, Windows-only target (`net10.0-windows10.0.19041.0`)
- **SkiaSharp** for the 60fps game canvas (night sky, smokestacks, toon, vignette)
- MAUI XAML for menu / game-over screens
- `Windows.Media.Playback.MediaPlayer` for the music loop (no extra packages)

## Build it (on Windows)

You need the **.NET 10 SDK** and **Visual Studio 2026** with the
“.NET Multi-platform App UI development” workload.

```powershell
cd windows\RiverReel.Windows
dotnet restore
dotnet build -c Release
# Run (unpackaged):
dotnet run -c Release -f net10.0-windows10.0.19041.0
```

Or open `RiverReel.Windows.csproj` in Visual Studio and press F5.
Target: **Windows Machine**.

> MAUI Windows targets must be built on Windows (your Parallels VM works).

## Art & music (copy these in)

Binaries don't live in git. Copy from the iOS project on your Mac:

| File | From (Mac) | To (here) |
|------|-----------|-----------|
| `popeye.png` … `pete.png` (9 sprites) | `~/Project-Clark/ios/RiverReel/Sprites/` | `Resources/Raw/` |
| `clark_ragtime.mp3` | `~/Project-Clark/ios/RiverReel/Resources/` | `Resources/Raw/` |

The game runs without them (fallback round bird, silent), but it's
better with them.

## Project map

| File | What |
|------|------|
| `Game/FlappyEngine.cs` | Physics + smokestack spawning + collisions (port of `FlappyEngine.swift`, same tuning) |
| `Game/FlappyRenderer.cs` | SkiaSharp night-flight renderer (port of `FlappyRenderer.swift`) |
| `Views/MenuPage` | Marquee menu, character picker, best score |
| `Views/GamePage` | 60fps loop + tap-to-flap canvas |
| `Views/GameOverPage` | "THE END" card, retry/lobby |
| `Services/MusicService.cs` | Ragtime loop + mute (persisted) |
| `Models/Roster.cs` | The 9 public-domain toons |

## Notes

- **No ads on Windows.** The iOS/Android ports use AdMob; there is no
  AdMob for MAUI Windows, so the Windows port just plays (consider it
  the premium edition).
- Best score, selected toon, and mute state persist via MAUI Preferences
  (same keys as iOS: `clark.best`, `clark.selected`, `clark.musicMuted`).
- Tap = click/tap on the canvas. Keyboard (spacebar) isn't wired — the
  canvas tap covers mouse + touch.
