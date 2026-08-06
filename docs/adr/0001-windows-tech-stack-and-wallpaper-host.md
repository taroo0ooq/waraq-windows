# ADR 0001 — Windows tech stack and wallpaper host strategy

| Field | Value |
|-------|--------|
| Status | **Accepted** |
| Date | 2026-08-06 |
| Work ID | WRQ-WIN-001 (Phase 1a) |
| Deciders | Atlas Forge (Agency), owner-approved scope |
| Repo | taroo0ooq/waraq-windows |

## Context

Waraq (upstream [bahamut42/waraq](https://github.com/bahamut42/waraq)) is a **macOS 14+** live-wallpaper app (Swift / SwiftUI / AppKit, GPL-3.0). On macOS it places a borderless `NSWindow` one level below the desktop-icon window so icons stay clickable.

This derivative targets **Windows 10/11**. Almost none of the Swift/AppKit UI or window-level code is reusable. Engines (video, GIF, procedural) must be rewritten against Windows media and graphics stacks. Constraints:

- Remain **GPL-3.0** (source offered with binaries; no proprietary relicensing).
- Multi-monitor, per-display profiles (parity goal, later phases).
- Video / GIF / procedural wallpapers; optional gallery later (zero telemetry by default).
- Agency quality gates: GitHub + CI/CD, SAST, DAST, Playwright before stage advance (Pipeline Warden after this scaffold).
- Local toolchain: .NET 8 SDK available; Rust also available.

## Decision

### Primary stack: **C# / .NET 8 + WPF** (`net8.0-windows`)

| Choice | Detail |
|--------|--------|
| Language / runtime | C# 12, .NET 8 LTS, Windows Desktop |
| UI shell | **WPF** (tray + settings window first; not WinUI 3) |
| Solution root | `windows/` (macOS tree at repo root remains untouched) |
| Packaging (later) | MSIX and/or portable folder; code signing when Ship stage opens |

### Wallpaper host strategy: **WorkerW (Progman) child window**

Attach a layered, click-through host window as a child of the desktop **WorkerW** window created behind icons (classic approach used by Lively Wallpaper and similar open-source hosts).

High-level flow (to implement in Phase 2):

1. Find `Progman` (`Program Manager`).
2. Send `0x052C` to spawn/find the WorkerW hierarchy (version-sensitive on Win10 vs Win11).
3. Enumerate top-level windows; select the WorkerW that sits **behind** icons (sibling of `SHELLDLL_DefView` parent chain — exact match differs slightly by build).
4. Create a borderless WPF or Hwnd host window per monitor bounds; `SetParent` onto WorkerW (or position as layered child as required by the chosen path).
5. Make wallpaper surface **click-through** (`WS_EX_TRANSPARENT` / `WS_EX_LAYERED` as appropriate) so desktop icons and Explorer remain interactive.
6. React to display change (`WM_DISPLAYCHANGE`), Explorer restart, and virtual-desktop quirks; re-parent if WorkerW is destroyed.

Fallback (documented, not primary): fullscreen **under-desktop** window without WorkerW parenting — worse icon interaction and z-order stability; keep only as diagnostic/dev path.

### Media path (forward-looking, not Phase 1a)

| Kind | Planned approach |
|------|------------------|
| Video | MediaFoundation / WPF `MediaElement` initially; evaluate LibVLC or FFmpeg interop if MF codec gaps hurt |
| GIF | WPF + imaging or dedicated decoder |
| Procedural | WPF/`CompositionTarget` or D3D/Win2D surface hosted in the same WorkerW window |
| Pause policy | Mirror upstream: battery, fullscreen exclusive, thermal/load (Windows power + focus APIs) |

## Options considered

### A. C# / .NET 8 + WPF + WorkerW — **chosen**

**Pros**

- Proven open-source precedent (e.g. Lively Wallpaper ecosystem patterns) for desktop wallpaper hosting.
- First-class multi-monitor and HWND interop via P/Invoke.
- Strong GitHub Actions `windows-latest` support; trivial `dotnet build` / `dotnet test`.
- Rich media and UI ecosystem without bundling a full Chromium runtime.
- Aligns with Agency CI (restore → build → test → later SAST).

**Cons**

- Windows-only (acceptable; product is Windows port).
- WPF is mature, not the newest Microsoft UI; styling is less “modern” out of the box than WinUI 3.

### B. C# / .NET + WinUI 3 (Windows App SDK)

**Pros:** Modern controls, long-term Microsoft investment.  
**Cons:** Packaging/identity friction for always-on tray utilities; HWND/WorkerW hosting is more awkward than classic WPF; heavier bootstrap for a scaffold that must go green quickly. **Rejected for MVP host**; may revisit for settings UI only later if needed.

### C. Rust + `windows-rs` + egui/iced (or raw Win32)

**Pros:** Small binaries, excellent FFI control, cargo already on some dev machines.  
**Cons:** Slower UI iteration for settings/gallery; fewer turnkey video pipelines; CI and contributor pool for desktop UX thinner than .NET for this product shape. **Rejected as primary** for speed-to-MVP and CI ergonomics. Rust may appear later as an optional native engine crate if profiling demands it.

### D. Tauri / Electron

**Pros:** Fast settings UI with web tech.  
**Cons:** Wallpaper host still needs native HWND/WorkerW code; Chromium/WebView memory cost on every machine for a background wallpaper product conflicts with upstream’s performance ethos; GPL + dependency graph complexity. **Rejected** for the wallpaper host process. (A separate lightweight UI is not worth a second runtime in Phase 1–2.)

### E. Fullscreen under-desktop window only (no WorkerW)

**Pros:** Simpler first spike.  
**Cons:** Fragile z-order with icons and Explorer; poor multi-monitor edge cases. **Rejected as primary**; retain as debug fallback flag only.

## Architecture sketch (Windows tree)

```
windows/
  WaraqWindows.sln
  README.md
  Waraq.Windows/                 # tray + settings + host orchestration
    Host/                        # WorkerW discovery, per-monitor surfaces
    Shell/                       # tray icon, main settings window
    Engines/                     # (Phase 2+) video/gif/procedural
  Waraq.Windows.Tests/           # unit tests (logic without UI where possible)
```

macOS sources (`App/`, `Core/`, `Engines/`, …) stay at repo root as upstream provenance and behavioral reference. Do not delete them in Phase 1a.

## Consequences

- All new Windows application code lives under `windows/` and is **GPL-3.0** with project headers / NOTICE preserved.
- Phase 1a success = ADR merged + scaffold **builds** on Windows (`dotnet build`) + README accurate + handoff to Nova for Pipeline Warden (CI/SAST/DAST/Playwright stubs).
- Phase 2 (separate route) implements real WorkerW parenting and a minimal video surface — not claimed done here.
- DAST may be N/A until a network surface exists; still requires explicit gate sign-off later.
- Developers need **.NET 8 SDK** (not runtime-only).

## Build evidence commands

```powershell
cd windows
dotnet restore WaraqWindows.sln
dotnet build WaraqWindows.sln -c Release
dotnet test WaraqWindows.sln -c Release
```

## References

- Upstream wallpaper window level pattern: `Core/WallpaperWindow.swift` (desktop icon level − 1).
- WorkerW community knowledge (Progman `0x052C`, Win10/Win11 differences) as implemented by established open-source live wallpaper hosts.
- Agency standing policy: GitHub + CI/CD + SAST + DAST + Playwright; no stage advance on red/open blockers.
