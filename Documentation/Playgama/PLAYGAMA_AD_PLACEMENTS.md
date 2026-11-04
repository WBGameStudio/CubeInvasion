# Cube Invasion — Playgama ad placements

Chosen by the product owner (2026-11-01): interstitials on the three result-screen exits, one rewarded
"double gold" offer after a win, no banners. All ads go through Playgama Bridge (`com.playgama.bridge` v2.4.0).
No other ad SDK is in the project (AdMob / Unity Ads / others: none found).

| Placement id | Type | Trigger (real game event) | Code |
|---|---|---|---|
| `level_completed` | Interstitial | Result screen after a **win**: player taps **NEXT MISSION** or **Play again** | `InvasionPrototype.ResultAdvance` / `ResultRetry` → `AfterBreakAd` |
| `level_failed` | Interstitial | Result screen after a **fail** (cube collapsed, out of HP, fell): player taps **RETRY** | `InvasionPrototype.ResultRetry` |
| `main_menu` | Interstitial | Result screen: player taps **Back to path** (returns to the mission map/menu) | `InvasionPrototype.ResultMenu` |
| `double_gold` | Rewarded | Result screen after a **win**: player taps **WATCH AD: DOUBLE GOLD (+N)** | `InvasionPrototype.WatchDoubleGold` |
| — | Banner | Not used | — |

Placement ids are declared in `Assets/WebGLTemplates/Bridge/playgama-bridge-config.json`.

## Rules applied

- **Never during a cube.** All four placements sit on result-screen buttons. The 20 s combat clock, route choice
  and cube transfer have no ad call. `START MISSION` from the menu has no ad.
- **No own frequency logic.** The game calls `Bridge.advertisement.ShowInterstitial(placement)` at every natural break
  and lets Bridge decide (initial delay, minimum interval). There is no timer or counter in game code.
- **Pause during ads.** On `Opened`, `PlaygamaService.SetAdOpen(true)` sets `AudioListener.pause`; any live
  gameplay is frozen through `ExternalPauseChanged` (not reachable from the result screen, kept as a guard).
  On `Closed`/`Failed` audio is restored and the button's action runs.
- **Never blocks.** If interstitials are unsupported the action runs immediately. If Bridge emits no state within
  0.6 s (request skipped by frequency rules) or stays in `Loading` for 8 s, the flow continues.
  A second tap while an ad flow is running is ignored (`adFlow` guard).
- **Rewarded is opt-in and honest.** The button says it shows an ad and the exact gold amount. It is hidden when
  `isRewardedSupported` is false, after the bonus was claimed, on fails, and while another ad flow runs.
- **Reward only on `Rewarded`.** `BridgeAdService.Rewarded` invokes the grant only on `RewardedState.Rewarded`,
  at most once per request; `Closed`/`Failed` without `Rewarded` grant nothing.
- **No double payout.** `MetaProfileStore.DoubleRun(runId)` is idempotent: it adds `lastResult.gold` once, sets
  `lastResult.doubled` and persists in the same commit. A repeated SDK event, double tap, or reload after the ad
  cannot pay again (`CanDoubleRun` is false once `doubled` is saved).
- **Save consistency.** The bonus is a normal profile commit, so wallet, result and Bridge Storage stay in sync. If the
  write fails, the save-retry screen re-runs the same idempotent grant.
- **Not required to progress.** Declining the rewarded button changes nothing; the next mission is unlocked by the win.

## Update 2026-11-04 (metrics-driven; these rows replace the matching rules above)

| Change | Detail | Code |
|---|---|---|
| Frequency | `initialInterstitialDelay` and `minimumDelayBetweenInterstitial` are both 150 s (Bridge default gap was 60 s) | `playgama-bridge-config.json` |
| Retry | Fail → RETRY shows no interstitial in onboarding or for the first 3 retries of a session | `ResultRetry` |
| Wait screen | "Loading ad…" after 0.25 s; interstitial load limit 4 s (rewarded stays 8 s) | `PlatformOverlays`, `AdManager` |
| Return from ad | The next cube holds its clock and enemies until the player moves | `startHold` |
| `revive` (rewarded) | One per run at the moment of failure: keeps the cube, 60% HP, at least 10 s | `OfferRevive` |
| `double_gold` (rewarded) | Also offered on fails that banked gold | `MetaProfileStore.CanDoubleRun` |
| `weapon_trial` (rewarded) | Locked weapon, no upgrades, one run; ownership and saved loadout never change | `WatchWeaponTrial` |
| `menu_banner` (banner) | Menu only, after mission 1; the layout moves up 110 CSS px while it shows | `PlatformBanner` |

New analytics events: `session_start`, `first_input`, `first_kill`, `result_shown`, `ad_requested`, `ad_result`, `rewarded_offer_shown`,
`rewarded_clicked`, `rewarded_revive`, `rewarded_weapon_trial`, `daily_reward`, `session_hidden` (best effort), `load_error`.

## Mobile builds

Android/iOS compile `NoAdService` (no ad network existed before this change): every interstitial request completes
immediately and the rewarded button never appears. Behaviour on devices is unchanged.

## Analytics

`rewarded_double_gold` (mission, gold) is sent through `Bridge.analytics` when the bonus is granted.
