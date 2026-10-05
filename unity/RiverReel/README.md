# River Reel — Unity Edition

A 1930s rubber-hose flappy-style game, rebuilt as a proper Unity 6 project.
Tap to flap through gaps in riveted smokestack pairs over a moonlit river.
Built-in Render Pipeline, 2D sprites, orthographic camera — no extra packages.

## Opening the project

1. Install **Unity Hub**, then install **Unity 6 LTS** (6000.0.x — the project
   pins `6000.0.84f1` in `ProjectSettings/ProjectVersion.txt`; Hub will offer
   the closest installed 6.x if you have a different patch).
2. In Hub: **Add → Add project from disk**, select this folder.
3. Open it. The whole game world (background, player, UI, audio) is built
   **procedurally at runtime** by `Assets/Scripts/Bootstrap.cs` — the
   `Main` scene contains only a Main Camera, so there is nothing to wire up.
4. Press **Play**. It runs immediately with generated placeholder art and
   synthesized audio — no assets required.

## Adding the real assets (from your Mac)

Copy these into the project (exact filenames), then click back into Unity —
an editor script auto-wires them, no manual setup:

| File | Destination |
|---|---|
| `felix.png`, `popeye.png`, `oswald.png`, `koko.png`, `bimbo.png`, `pooh.png`, `olive.png`, `bosko.png`, `pete.png` | `Assets/Sprites/Characters/` |
| `delta_ragtime.mp3` | `Assets/Audio/Music/` |

- The PNGs are rear 3/4 run-pose sprites with transparency (the same nine
  from the iOS/Android builds). Import settings (Sprite, pivot center) are
  pre-configured via the `.meta` files already in that folder.
- Until the PNGs arrive, the game draws its own rubber-hose toon placeholders.
- Until the MP3 arrives, music is a procedurally generated ragtime-style loop.
  Music credit: **"The Entertainer" — Kevin MacLeod (CC-BY 4.0)**.

You can also force a rewire any time: menu bar → **River Reel → Rewire
Character + Music Libraries**.

## Building for Windows

1. **File → Build Profiles** (Unity 6).
2. If `Assets/Scenes/Main.unity` isn't listed, drag it into the scene list.
3. Platform: **PC, Mac & Linux Standalone** → Target: **Windows**, Architecture **x86_64**.
4. **Build** (or Build And Run).

Default window is 540×960 portrait, resizable.

## Controls

- **Click / Spacebar / tap** — flap
- Menu: click/tap anywhere (not on a button) to start; `<` `>` to change character
- Game over: click/tap to retry, or use the RETRY / MENU buttons

## How it's put together

- `Bootstrap.cs` — `[RuntimeInitializeOnLoadMethod]` entry point; creates the root GameObject.
- `GameManager.cs` — builds every subsystem, owns the Menu/Playing/GameOver
  state machine, input, scoring, difficulty ramp, high-score persistence.
- `Player.cs` — transform-based physics (no Rigidbody): gravity, flap impulse,
  tilt, squash-and-stretch; manual AABB collision.
- `StackPair.cs` / `StackSpawner.cs` — smokestack pairs; gap narrows and scroll
  speed ramps with score.
- `ParallaxBackground.cs` — moonlit river: gradient sky, stars, moon + glow,
  two cloud layers, 1930s skyline silhouette, animated water with moonlight
  shimmer and reflection.
- `UIManager.cs` / `UiKit.cs` — fully procedural UI: marquee title with chase
  lights, character select, HUD, game-over panel, score popups, film
  grain + vignette overlay.
- `AudioManager.cs`, `SfxSynth.cs`, `RagtimeGenerator.cs` — music (MP3 or
  procedural loop) + synthesized flap/score/die/splash/click SFX.
- `Effects.cs` — pooled sprite particles; `CameraShake.cs` — trauma shake.
- `CharacterLibrary.cs` / `MusicLibrary.cs` — ScriptableObjects holding the
  real assets once dropped in; `Scripts/Editor/RiverReelAssetWiring.cs`
  keeps them wired automatically.

## Notes

- High score and character choice persist via `PlayerPrefs`.
- No ads in this build (no AdMob for Unity Windows standalone in this setup).
- The 9 character likenesses are public-domain cartoon stars; the same
  trademark note applies as the other builds: fine to use in-game, keep them
  out of the icon/title.
