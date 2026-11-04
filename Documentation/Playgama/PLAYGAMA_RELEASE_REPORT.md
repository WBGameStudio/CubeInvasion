# Cube Invasion — Playgama WebGL release report

Branch `playgama` (from `main` @ `373eac4`). Date 2026-11-01. Unity 6000.4.8f1, URP 17.4, Playgama Bridge Unity **v2.4.0**.
Sources checked: the Playgama wiki requirement pages (technical, advertising, UX, content, Unity engine-specific),
Bridge setup/config/API pages, and the `bridge-unity` v2.4.0 source (C# modules, jslib, WebGL template JS).

> **Verdict: technically ready for the Playgama console's Test Game step.** The 3.3.2 audio blocker is resolved
> (recorded CC0 audio, see `AUDIO_LICENSES.md`). Real ads, real devices and other browsers still have to be checked in the
> console before submitting.

## A. What was done
**Platform layer (new):** `Assets/CubeInvasion/Runtime/Platform/`
- `PlaygamaService`: creates the Bridge receiver before the first scene; loads `ci_profile`, `ci_profile_bak`,
  `ci_settings` in one `storage.get` before play; batches writes and retries failed ones; reads `platform.language`
  and `isAudioEnabled` once; handles `audioStateChanged` and `pauseStateChanged`; sends `game_ready`,
  `level_started/completed/failed` and analytics.
- `GamePrefs`: replaces `PlayerPrefs` in game code (PlayerPrefs on device, one Bridge key on web).
- `AdManager` / `IAdService`: `BridgeAdService` (WebGL) and `NoAdService` (devices, editor checks).

**Game code changes**
- `MetaProfileStore`: split into `IProfileBackend` (`FileProfileBackend`, which is the old file logic moved unchanged,
  and `BridgeProfileBackend`). An unreadable platform store blocks writes, so it never overwrites real progress.
  Added idempotent `DoubleRun` for the rewarded bonus.
- `InvasionPrototype.Playgama.cs` (new): `game_ready`, external pause, result-screen ad flow, rewarded x2.
- `InvasionPrototype.cs`: waits for Bridge storage before building the world. No orientation lock on web.
  WASD/arrows on WebGL. Esc → **P** on web. Tick for write retries. Focus mutes audio on web.
- `P2MissionHud`: rewarded "WATCH AD: DOUBLE GOLD (+N)" button on wins.
- `P4ValidationLog`: no file telemetry on web.
- `MobileCombatHud`: canvas matches height on windows wider than 9:16 (web only).
- `ArcadeFollowCamera`: guards `Update` until `Follow()` ran. This fixed an NRE on web that also broke the menu refresh.
- `InvasionPrototype.Validation.cs`: web hint "MOVE: WASD / ARROWS OR DRAG"; on web the duplicate route hint that
  covered a route card is removed.
- `InvasionPrototype.Content.cs`: leftover Turkish HUD strings (FAZ/CAN/HEDEF) → English. `VoidToyPalette.FormatGold`
  uses English digit grouping on web.
- Editor: `PlaygamaWebBuild` (configure/build/ZIP), `PlaygamaPlatformCheck`. `FrontEndSmokeCheck` now reads settings
  through `GamePrefs` (same on device).

**Packages / template / settings**
- `Packages/manifest.json`: `com.playgama.bridge` = `bridge-unity.git#v2.4.0`.
- `Assets/WebGLTemplates/Bridge/`: official template plus title "Cube Invasion", no scroll / selection / context menu /
  iOS callout, and a `visibilitychange` audio-context suspend backstop. Config declares the 4 placements.
- WebGL-only player settings: Bridge template, Gzip + decompression fallback, data caching **off**, 64→1024 MB memory,
  Medium stripping, IL2CPP size/Master. Android/iOS settings unchanged; the project is left on the Android target.

**Unchanged:** core mechanics (top-down camera, single joystick with auto-fire, exact 20 s cubes, collapse on timeout,
timer stop on clear, RIGHT/FORWARD gates with reward/risk, chapters, weapons, bosses, Gold Rush) and the mobile save path.

## B. Ad integration
| Placement | Type | Trigger | Fallback |
|---|---|---|---|
| `level_completed` | Interstitial | Win screen → NEXT MISSION / Play again | Unsupported / no state in 0.6 s / load >8 s → continue |
| `level_failed` | Interstitial | Fail screen → RETRY | same |
| `main_menu` | Interstitial | Result → Back to path | same |
| `double_gold` | Rewarded | Win screen → WATCH AD: DOUBLE GOLD (+N) | Hidden if unsupported / already claimed / fail screen |

- Bridge decides frequency. There is no game-side timer or counter, and no ads during a cube.
- `Opened` → `AudioListener.pause`, live gameplay frozen. `Closed`/`Failed` → restore, then continue.
- The reward is granted only on `RewardedState.Rewarded`, once per request. `DoubleRun(runId)` persists
  `lastResult.doubled` in the same commit as the gold, so a reload or a repeated event can't pay twice.
- No banners. No other ad SDK exists in the project. Details: `PLAYGAMA_AD_PLACEMENTS.md`.

## C. Performance
| Metric | Value |
|---|---|
| ZIP | **20.9 MB** (20,939,676 bytes); limit 300 MB. It was 16.8 MB with the synthesized audio. |
| Uncompressed build | 21.4 MB: wasm 8.9 MB, data 12.0 MB (audio 6.3 MB, of which 5.3 MB is the five music loops), framework 94 KB, Bridge JS 0.41 MB |
| Before/after | No earlier WebGL build exists; no "before" figure |
| Render path | WebGL quality *Medium* → `MobilePipeline` URP (no realtime shadows, HDR off, MSAA 2×), mostly unlit materials |
| Pools | Projectiles, enemy shots, VFX rings/sparks/debris already pooled; 6 audio voices |
| Ships | 1 scene. Resources total ≈ 2.7 MB (TMP 2.3 MB) |
| FPS / memory / load time | **NOT TESTED**: the browser pane was hidden (≈1.5 fps throttle), so no valid measurement. Max heap is capped at 1 GB, under the 2 GB start-up limit. |

Optimizations applied: size-optimized IL2CPP, Medium stripping, Gzip, no data cache. No asset deletions or package
removals were done without a measured reason.

## D. Requirement compliance
| # | Requirement | Status |
|---|---|---|
| 1.1.1–1.1.2 | Current Bridge + required steps | PASS (v2.4.0; config, init-before-engine via official template, language, storage, pause/audio, game_ready, interstitials, unsupported features hidden) |
| 1.2.1–1.2.3 | No forced sign-in | PASS (no auth used) |
| 1.2.4 | No external analytics | PASS (Bridge analytics only) |
| 1.2.5 | No external resources | PASS with note: the only external request is Bridge's own CDN script from the official template, with a packaged local fallback |
| 1.2.6–1.2.7 | External links / URL locks | PASS (none) |
| 1.3.1 | ≤ 300 MB | PASS (16.8 MB) |
| 1.3.2 | `index.html` at root, Latin names | PASS |
| 1.3.3 | < 2 GB at start | PASS by configuration (1 GB cap); not measured |
| 1.4.1 | Full screen | PASS (canvas fills page) |
| 1.4.3 | Single orientation declared | ACTION: select **Portrait** in the draft |
| 1.4.4–1.4.5 | Touch, no long-press menu/selection | Implemented; NOT TESTED on a real phone |
| 1.4.6 | No engine warning on mobile | NOT TESTED on device (Unity's mobile warning is not in the Bridge template) |
| 1.5.1–1.5.3 | Keyboard/mouse, no Esc, both schemes | PASS |
| 1.6.1–1.6.4, 1.6.8 | Adaptive, no scroll, no overlap, 1:1 | PASS at 9:16, 9:19.5, 16:9, 4:3, 1:1 (landscape menu is a widened portrait layout) |
| 1.6.5 | Pop-ups closable | PASS (existing UI) |
| 1.7.1 | No errors/freezes | PASS for clean load and tested flows; long sessions NOT TESTED |
| 1.7.3 | All major browsers | NOT TESTED beyond Chromium |
| 2.1.x | Ads only via Bridge, in pauses, pause audio, never blocks | PASS (implementation + fallback); real ads NOT TESTED |
| 2.2.x | Rewarded opt-in, honest, reward on completion only | PASS (logic + unit checks); real ad NOT TESTED |
| 2.3 / 2.4 | Banners / payments | N/A (none) |
| 3.1.x | Progress, rising difficulty, short onboarding | PASS (existing design) |
| 3.2.1–3.2.3 | Bridge storage only; survives reload | PASS (no game IndexedDB/localStorage/PlayerPrefs on web; reload verified) |
| 3.3.1, 3.3.3 | Music + SFX; mute button | PASS (pause overlay + settings) |
| 3.3.2 | Real produced audio, no code-synthesized sound | PASS: 70 recorded CC0 OGG clips (Kenney ×3, OpenGameArt SRG774); synthesis code and the 72 generated WAVs removed; `AudioComplianceCheck` passes |
| 3.3.4 | `isAudioEnabled` at start + events | PASS (implemented); real platform NOT TESTED |
| 3.3.5 | Sound stops when hidden | Implemented twice (Bridge pause + page backstop); NOT TESTED |
| 3.4.1–3.4.2 | English default, `platform.language` only | PASS (English-only release) |
| 3.5.1 | Title matches | PASS ("Cube Invasion" in page title and listing) |
| 4.x | Content | No issues seen (toy robots, no realistic violence); rights to third-party art packs in `Assets/Assets/` are not shipped (not referenced by the build scene) |
| 5.1.1 | Decompression fallback | PASS |

## E. Tests
See `PLAYGAMA_QA_CHECKLIST.md`. Summary:
- RUN and passing: platform checks 16/16 (Android + WebGL), mobile profile check, UI smoke on Android and WebGL,
  route / boss / Gold Rush / countdown play-mode checks, release build, browser load / boot / storage / reload /
  pause / resume / keyboard / 5 aspect ratios.
- RUN and failing: `PrototypeSmokeCheck`, which fails on a camera-tilt expectation that is pre-existing on `main`.
- Not run: real ads, real devices, other browsers, FPS/memory.

## F. Release output
- `Builds/Playgama/CubeInvasion_Playgama_v1.0.zip`: **20,939,676 bytes**, build *Succeeded*, 0 errors.
  (`Builds/` is git-ignored; rebuild with `PlaygamaWebBuild.Build`.)
- Steps left for the developer: upload a draft (Portrait, English, title
  "Cube Invasion") → **Test Game** in the console → run the manual list in the QA checklist → submit.
  The game was not submitted or published.

## G. Open issues and risks
1. Audio (3.3.2) resolved: see `AUDIO_LICENSES.md`. WebGL rejects per-platform audio quality overrides, so the
   music ships at source quality. Re-encoding the five loops at a lower bitrate, or using fewer tracks, would save up to ~3 MB.
2. Real ad behaviour, the hidden-tab mute, and the platform audio state are implemented against the v2.4.0 source,
   but not exercised on the Playgama platform.
3. Landscape/desktop: the level path is now centred. The PLAY button and tab bar still span the window width.
4. Unity creates an empty `/idbfs` IndexedDB at start-up (engine behaviour). The game writes nothing there.
5. Pre-existing: 18 GameObjects carry a missing-script component (unknown deleted script; console warning only).
   `PrototypeSmokeCheck` expects 45° tilt vs 50° in the scene.
6. YouTube Playables (30 MB, multi-orientation) is a separate future target. This build is 16.8 MB but portrait-only.
7. Disk space: two unused Unity editors (6000.0.58f2, 6000.6.0f1) were removed at the owner's request to install
   WebGL support.

## Audio update (2026-11-01)
- Sources: Kenney Sci-Fi / Impact / Interface Sounds (CC0) and OpenGameArt "Dark Sci-Fi Audio Pack" by SRG774 (CC0).
  Licenses were verified on the source pages and in the bundled `License.txt` files. Files are unedited (not trimmed).
- All 31 existing cues have two recorded variants. New cues: `Victory` (level won, OGA jingle) and `UiClick` (menu
  buttons, through `CasualButtonMotion.Pressed`).
- Music: menu `airy`; biomes `sector` / `pulse` / `transmission`; Gold Rush `pulse`; boss `urgent`; low-time pressure
  layer `urgent`. A 0.8 s fade-in plays on every track change.
- Levels: each cue's loudness is matched by measured RMS (`Entry.gain`). After play-testing feedback, effects were
  lowered about 5 dB and music about 4.4 dB (music bed 0.16).
- Mute, Bridge audio state, hidden-tab and ad pauses are unchanged. The music and UI-click paths still go through
  `CombatFeedback` / `AudioListener`.
- Removed: `CombatFeedback.Synthesize`, the WAV writers in `P2ContentBuilder`, `P3MetaBuilder`, `CampaignSoundBuilder`
  and `CampaignAudioBuilder`, and 72 generated WAVs. The builders now call `CubeAudioLibrary.Install()`.
- Browser (release build): menu and combat music decode and play, 10+ SFX decode, 0 decode errors, 0 console errors,
  AudioContext `running`. Listening quality was judged by the owner in the browser pane.
