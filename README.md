# River Reel (Project Clark)

A 1930s-cartoon flappy-style game for Android, starring the public-domain
toon roster. Tap to flap your flyer through the smokestack gaps over a
moonlit river — don't kiss the pier.

## Play

- **Tap** anywhere to flap.
- Thread the gap between each smokestack pair: +1 per stack cleared.
- The gap narrows and the river speeds up as your score climbs.
- Bonking the ceiling just stalls you; the stacks and the pier end the run.

## Project layout

- `app/src/main/java/com/emckeon97/projectclark/`
  - `game/FlappyEngine.kt` — physics, stacks, scoring, collisions (dp units)
  - `game/FlappyCanvas.kt` — night-river rendering (Compose Canvas)
  - `game/MusicManager.kt` — ragtime loop + persisted mute
  - `ui/` — Menu (marquee + flyer picker), Game, Game Over ("THE END" card)
  - `ads/AdManager.kt` — AdMob; TEST ids behind `useTestIDs`
  - `characters/Roster.kt` — the 9 sprite toons
- `app/src/main/res/drawable/` — character sprite PNGs (see Assets below)
- `app/src/main/res/raw/clark_ragtime.mp3` — music (see Assets below)

## Assets

Binary assets can't ride the git push, so they live in
`app/src/main/res/` on the dev machine:

- `drawable/felix.png`, `popeye.png`, `oswald.png`, `koko.png`, `bimbo.png`,
  `pooh.png`, `olive.png`, `bosko.png`, `pete.png`
- `raw/clark_ragtime.mp3` — "The Entertainer" by Kevin MacLeod (CC-BY 4.0,
  attribution in the menu)

## Build

Open in Android Studio and run `assembleDebug` (same toolchain recipe as the
Project Delta Android port: AGP 9.4.1 + Gradle 9.6.0 + Kotlin 2.4.20).

## Ads

Google TEST ad units while `AdManager.useTestIDs` is true: banner on the menu
only, interstitial on every 5th game over (max one per 60s), nothing during
gameplay. Fill in `REAL_*` and flip the flag for release.
