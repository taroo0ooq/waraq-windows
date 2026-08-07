# WRQ-WIN-002 Phase 7 — Governor / Diagnostics / Onboarding

## Governor
- Settings: `%AppData%\Waraq\governor.json`
- Policies: low battery threshold, other-app fullscreen heuristic, high process working set
- User pause wins over auto-resume (mac DisplayManager invariant)
- Poll every 5s via `GovernorRuntime` → `WallpaperController.SetGovernorPaused`

## Windows honesty
- **No** IOKit thermal API analogue claimed
- Battery via `GetSystemPowerStatus`
- Fullscreen = foreground HWND ≈ covers virtual desktop and not our PID
- CPU % best-effort process delta; **no** GPU %

## Diagnostics
Advanced pane: local samples only, zero telemetry.

## Onboarding
5-step wizard on first launch; re-run from General. Flag: `onboarding.json`.
