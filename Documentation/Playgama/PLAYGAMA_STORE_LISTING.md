# Cube Invasion — Playgama store listing (draft)

## Title
**Cube Invasion** (matches the in-game title and `<title>` of `index.html`, requirement 3.5.1)

## Short description (≤ 120 chars)
Clear each floating cube in 20 seconds, pick your route, upgrade your blaster and beat the bosses.

## Long description
Robots have taken over the floating cubes — and every cube gives you exactly 20 seconds.

Steer your little scout with one thumb (or WASD / arrow keys). Your weapon fires on its own at the nearest
target, so all you have to think about is where to stand. Down every enemy before the clock hits zero, or the
cube collapses under your feet.

Cleared a cube? Two gates open. Walk through **RIGHT** or **FORWARD** — each route shows its reward and its risk
before you commit: more health, a damage boost, extra gold, or a tougher elite fight that pays more.

- 60 missions across 6 worlds, with rising difficulty
- 12 weapons to unlock and upgrade: from Pulse Pistol to Orbit Drones and Nova Cannon
- 7 multi-phase boss fights and 4 Gold Rush missions
- Gold from every cleared cube is banked — even when a later cube fails
- Short, fair sessions: every fight lasts at most 20 seconds

## How to play
- **Move:** drag the joystick (touch / mouse) or use **WASD / arrow keys**.
- **Shoot:** automatic — your scout aims at the nearest enemy.
- **Goal:** defeat every enemy on the cube within **20 seconds**.
- **Route:** after clearing, walk through the right or forward gate to pick the next cube's reward.
- **Upgrade:** spend banked gold on weapons and upgrades between missions.
- **Pause:** pause button (top right) or **P** on keyboard.

## Category and tags
- Category: **Action** (alternative: Arcade)
- Tags: shooter, top-down, arcade, robots, auto-fire, upgrades, boss fights, roguelite routes, casual, one-hand, timer

## Platform data for the publication request
| Field | Value |
|---|---|
| Devices | Mobile and desktop |
| Orientation | Select **Portrait** only. The field covers mobile devices; landscape phones get Playgama's rotate placeholder (1.4.3). Desktop is always playable, and the layout adapts to wide windows. |
| Languages | **English only** (default and fallback). Do not declare other languages. |
| Controls | Touch joystick; keyboard (WASD/arrows, P to pause) and mouse on desktop |
| Saves | Playgama Bridge Storage |
| In-app purchases | None |
| Ads | Interstitial (result screen), rewarded (continue, double gold, weapon trial), banner on the menu only |

## Covers (ready to upload)
Playgama requires three covers, PNG or JPEG, at exactly these sizes, max 10 MB each:

| Slot | File | Size |
|---|---|---|
| Square | `Covers/cover_square_800x800.png` | 800×800 |
| Portrait | `Covers/cover_portrait_1080x1920.png` | 1080×1920 |
| Landscape | `Covers/cover_landscape_1920x1080.png` | 1920×1080 |

Character art is rendered from the game's own 3D models (`Covers/layers`, made by `PlaygamaCoverLayers.Run`); the scene and logo are
composed by `python3 Tools/compose_covers.py` (needs Pillow and numpy). The earlier gameplay-capture covers are in `Covers/previous/`.
Screenshots, icon and video are optional in the console, and their sizes are
not documented.
