# Cube Invasion — Playgama QA checklist

Status as of 2026-11-01, build `CubeInvasion_Playgama_v1.0.zip` (branch `playgama`).
**RUN** = executed with the result shown. **NOT RUN** = not executed; still needs a person or the Playgama console.

Browser runs used the Claude desktop browser pane (Chromium) against `python3 -m http.server` on 127.0.0.1, with
Bridge on its `mock` platform. That pane was hidden, which throttles the page to ~1.5 fps, so **no timing, FPS or
feel results are valid from it**.

## Automated checks
| Check | Target | Result |
|---|---|---|
| C# compile (Bridge code excluded) | Android | RUN — 0 errors |
| C# compile (Bridge code active) | WebGL | RUN — 0 errors |
| `PlaygamaPlatformCheck` (16 checks: backend save, cube reward once, x2 once, reload, corrupt/unreadable save, failed write, ad contract) | Editor, Android + WebGL | RUN — 16/16 PASS |
| `P3ProfileCheck` (device file save, unchanged path) | Android | RUN — PASS |
| `FrontEndSmokeCheck` (51 UI/flow checks, incl. launch → combat, result retry, settings) | Android | RUN — PASS |
| `FrontEndSmokeCheck` | WebGL (Bridge editor mock) | RUN — PASS |
| `BranchRoutesCheck` (route reward/risk) | Android | RUN — PASS |
| `CrusherPhaseCheck` (boss phases) | Android | RUN — PASS |
| `GoldRushVariantCheck` | Android | RUN — PASS |
| `CountdownTensionCheck` (20 s clock presentation) | Android | RUN — PASS |
| `PrototypeSmokeCheck` | Android | RUN — **FAIL, pre-existing**: expects a 45° camera tilt; the scene on `main` authors 50°. Not caused by this branch (scene untouched). |
| WebGL release build | — | RUN — Succeeded, 0 errors |
| `AudioComplianceCheck` (every cue/music slot recorded; all 70 build clips are licensed OGG and documented; no synthesis code) | Android + WebGL | RUN — PASS |

## Browser (release and development builds, localhost, Bridge mock)
| Scenario | Result |
|---|---|
| Fresh load: console errors | RUN — none (release build, final) |
| Audio: menu + combat music and SFX decode/play in browser | RUN — PASS (0 decode errors, context running) |
| `index.html`, config, Bridge scripts served; ZIP has `index.html` at root | RUN — PASS |
| Bridge init before Unity; language + audio state read once | RUN — PASS (`getPlatformLanguage`, `getIsPlatformAudioEnabled`) |
| All saves loaded in **one** storage request before play | RUN — PASS (`ci_profile`, `ci_profile_bak`, `ci_settings`) |
| `game_ready` sent once, after the menu is up | RUN — PASS |
| Early Bridge events not dropped | RUN — PASS after fix (`PlaygamaBridge` receiver created before scene load) |
| Mission start: one batched profile write, `level_started` message + analytics | RUN — PASS |
| Cube cleared: one write (banked gold), `cube_cleared` analytics | RUN — PASS |
| Page reload restores gold / progress | RUN — PASS (gold 25 restored) |
| Interrupted run on reload is closed, banked gold kept | RUN — PASS (profile revision advanced, gold kept) |
| No IndexedDB/localStorage writes by the game | RUN — PASS: `/idbfs` empty (engine-created), `UnityCache` removed by disabling data caching; localStorage keys only from Bridge's mock storage |
| Platform pause (`OnPauseStateChanged true`) freezes the cube and its timer | RUN — PASS (timer held at 19.7 for 5 s) |
| Resume with **P** (Esc not used) | RUN — PASS |
| Keyboard movement (WASD) | RUN — PASS (hero moves, camera follows) |
| Mouse menu buttons | RUN — PASS (needs a normal press; automation's zero-length click is ignored by Unity) |
| Desktop hint text "MOVE: WASD / ARROWS OR DRAG" | RUN — PASS |
| Aspect 9:16 (405×720) menu | RUN — PASS, no scroll/clip/overlap |
| Aspect 9:19.5 (360×780) menu | RUN — PASS |
| Aspect 16:9 (1280×720) menu + combat | RUN — PASS (level path centred) |
| Aspect 4:3 (800×600) combat + route choice | RUN — PASS after removing the duplicate route hint that sat on a route card |
| Aspect 1:1 (720×720) menu | RUN — PASS, fills, no bars |
| Interstitial on NEXT / RETRY / Back to path | NOT RUN in browser (mock reports interstitials unsupported → flow continues immediately; that fallback is covered). Real ad display: **Playgama console** |
| Rewarded x2 button and grant | Logic: RUN (unit checks). Button visual and real ad: NOT RUN (mock: rewarded unsupported → hidden, as designed) |
| Ad open/close pauses audio and resumes | NOT RUN with a real ad |
| Tab hidden → sound stops | NOT RUN (pane can't toggle visibility); Bridge `pause_state_changed` + page `visibilitychange` audio suspend are implemented |
| Platform audio disabled → silent, survives reload | NOT RUN with a real platform |
| 20 s timeout → cube collapses → fail screen → RETRY (browser) | NOT RUN in browser (10× slow); covered by play-mode checks |
| Last enemy dies exactly at 0.0 s | Covered by `CubeCombatClock` (damage resolved before timeout in the same frame); no dedicated browser run |
| Gold Rush / boss in browser | NOT RUN in browser; play-mode checks PASS |
| Touch joystick, long-press, pull-to-refresh on a real phone | NOT RUN — needs iOS Safari / Android Chrome device |
| Chrome / Firefox / Safari / Edge / Opera | Only Chromium (desktop app pane) RUN |
| FPS, memory, load time | NOT RUN (pane hidden) |

## Before submitting (manual)
- [x] Recorded CC0 audio installed (`AudioComplianceCheck` PASS; sources in `AUDIO_LICENSES.md`).
- [ ] Upload ZIP as a draft; run **Test Game** in the Playgama Developer Console.
- [ ] In the console test: interstitial on NEXT, RETRY, Back to path; rewarded x2 grants once, cancel grants nothing.
- [ ] Tab switch / minimize silences the game and pauses an active cube.
- [ ] Real phones: iOS Safari and Android Chrome — joystick, no long-press menu, no page scroll, portrait.
- [ ] Desktop: Chrome, Firefox, Safari, Edge — keyboard + mouse, window resize, 1:1.
- [ ] Capture store screenshots from the real build (see store listing).
