# WRQ-WIN-002-HF1-D1 — Host Apply fail-closed

## Defect
Apply activated a WinUI window, resized to the **full virtual desktop** (black), then called `SetParent` to WorkerW. On attach failure or WinUI ignoring reparent, the top-level black window stayed → **all-monitor blank freeze**.

## Fix
1. Create surface at **1×1 off-screen** (`-32000,-32000`), hide from switchers, `AppWindow.Hide` when possible.
2. `DesktopWallpaperHost.TryAttachHwnd` verifies `GetParent(hwnd) == WorkerW` after `SetParent` and after size/show.
3. On any failure: **SW_HIDE**, unparent best-effort, **Close** surface — never leave a fullscreen top-level overlay.
4. Only after successful attach expand to virtual screen via `SetWindowPos` under WorkerW.
5. Content starts **transparent**; black fill only when media installs post-attach.
6. `WallpaperController.LastAttachError` surfaces message for UI footer.

## Spike note
If WinUI continues to refuse SetParent on some builds, `TryAttachHwnd` returns a clear message. A native HWND/WPF child host remains a follow-up option (not required if verify path holds).

## Tests
`HostAttachFailClosedTests` — zero HWND fail-closed, probe, virtual screen.
