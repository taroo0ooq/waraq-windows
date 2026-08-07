# WRQ-WIN-002 — macOS v1.0.0 ↔ Windows parity matrix

| Field | Value |
|-------|--------|
| **work_id** | WRQ-WIN-002 |
| **phase** | 0 — Parity audit |
| **desk** | Atlas Forge |
| **date** | 2026-08-07 |
| **mac baseline** | bahamut42/waraq v1.0.0 (sources at repo root) |
| **Windows product baseline** | Clean rebuild (`src/` in Phase 1); WRQ-WIN-001 code lives in `archive/wrq-win-001/` as reference only |
| **screenshots** | `docs/screenshots/*.png` |
| **design specs** | `docs/design/*` |

**Status legend**

| Code | Meaning |
|------|---------|
| **P0** | Must ship for “recognizable Waraq” parity slice (program ≥95% target counts these first) |
| **P1** | Full v1 parity (after P0 core) |
| **P2** | Stretch / post-parity |
| **DELTA** | Honest platform difference (document; do not fake) |
| **OOS** | Out of scope (same as mac: no `.we` scenes, no YouTube DL) |

**Windows status (Phase 0)** = planning only. No UI/engines claimed done.

---

## A. Shell & chrome

| # | Feature (mac v1) | Pri | mac evidence | Windows target | Phase map | Notes |
|---|------------------|-----|--------------|----------------|-----------|-------|
| A1 | Tray / menu-bar icon + popover | P0 | `App/MenuBarController.swift`, `menubar.md` | NotifyIcon + WinUI tray flyout (pause, settings, quit) | 2 | DELTA: no global menu bar; tray is analogue |
| A2 | Settings window shell (sidebar + detail) | P0 | `settings-shell.md`, `02-displays.png` etc. | WinUI `NavigationView` / `NavigationView` left nav 190dip | 2 | Owner visual accept Phase 2 |
| A3 | Sidebar panes: General, Displays, Library, Performance, Wallpapers, About | P0 | `App/Settings/*` | Same pane set as stubs then full | 2→7 | Gallery may sit under Library or own nav item (match mac if Gallery is tab) |
| A4 | Gallery pane | P0 | `GalleryPane.swift`, `04-gallery.png` | Same | 6 | Network only on search |
| A5 | Browse Web cards | P0 | `05-gallery-browse-web.png` | Open default browser only | 6 | No scrape |
| A6 | Advanced mode toggle + Diagnostics pane | P1 | settings-shell Advanced | Same | 2 stub / 7 full | |
| A7 | Settings search (sidebar + deep link) | P1 | settings-shell | WinUI AutoSuggestBox | 2→8 | |
| A8 | Design tokens (type, space, semantic colors, dark/light) | P0 | `design-tokens.md` | `docs/design/windows/DESIGN.md` ResourceDictionary | 1–2 | See DESIGN.md |
| A9 | App icon + tray icon Windows sizes | P0 | `app-icon.md`, Resources | `.ico` multi-size + tray 16/20/24/32 | 2 | Brand crimson content-only |
| A10 | Onboarding 5-step + Run setup again | P0 | `onboarding.md`, App/Onboarding | WinUI ContentDialog / Window wizard | 7 | |
| A11 | Basic/Advanced global mode | P1 | settings-shell | Same | 2 | |

---

## B. Playback engines

| # | Feature | Pri | mac evidence | Windows target | Phase | Notes |
|---|---------|-----|--------------|----------------|-------|-------|
| B1 | Video wallpaper (MP4/MOV/M4V) | P0 | `Engines/VideoEngine.swift` | Media Foundation / Win2D or LibVLC fallback | 3 | DELTA: MOV codec pack varies; document supported list |
| B2 | GIF wallpaper | P0 | `GifEngine.swift` | WIC / custom decoder (archive GIF path OK reference) | 3 | |
| B3 | Static image engine | P1 | `ImageEngine.swift` | WIC | 3–4 | |
| B4 | Procedural: Aurora | P0 | `Engines/Procedural/AuroraView.swift` | Win2D/Skia composition | 5 | |
| B5 | Procedural: Matrix Rain | P0 | MatrixRainView | same | 5 | |
| B6 | Procedural: Synthwave Drive | P0 | SynthwaveView | same | 5 | |
| B7 | Procedural: Starfield | P0 | StarfieldView | same | 5 | |
| B8 | Procedural: Neural Network | P0 | NeuralNetworkView | same | 5 | |
| B9 | Procedural: Animated Gradient | P0 | GradientWallpaper / factory | same | 5 | |
| B10 | Fit modes (fill/fit/stretch) | P0 | DisplaySettings | same | 3 | |
| B11 | Mute / volume / loop per display | P0 | DisplaySettings | same | 3–4 | |
| B12 | Wallpaper Engine **video subset** import | P1 | WallpaperEngineImporter | same rules | 4 | OOS: `.we` scenes |
| B13 | Crossfade transition 0–3s | P1 | design-tokens | Composition animation | 8 | Respect Reduce Motion |
| B14 | YouTube download | OOS | README | — | — | Same as mac |
| B15 | Wallpaper Engine scenes runtime | OOS | README | — | — | |

---

## C. Displays & profiles

| # | Feature | Pri | mac evidence | Windows target | Phase | Notes |
|---|---------|-----|--------------|----------------|-------|-------|
| C1 | Per-monitor enable + independent wallpaper | P0 | DisplayManager | EnumDisplayMonitors + per-surface host | 3–4 | |
| C2 | Profiles keyed by stable hardware ID | P0 | DisplayProfile, WaraqPrimaryStore | EDID/hash + fallback adapter LUID | 4 | DELTA: no CGDirectDisplayID; design stable key early |
| C3 | Show Numbers overlay | P1 | Displays pane | Topmost numbered labels | 4 | |
| C4 | Waraq Primary override | P1 | WaraqPrimaryStore | same concept | 4 | |
| C5 | Hotplug restore | P0 | DisplayManager | WM_DISPLAYCHANGE | 3–4 | |
| C6 | Per-monitor DPI (PerMonitorV2) | P0 | NSScreen | Application manifest + WinUI | 1–3 | |

---

## D. Library

| # | Feature | Pri | mac evidence | Windows target | Phase | Notes |
|---|---------|-----|--------------|----------------|-------|-------|
| D1 | Local library under app data | P0 | WallpaperLibrary | `%AppData%\Waraq\{Wallpapers,Thumbnails,library.json}` | 4 | |
| D2 | Import files / folders / drag-drop | P0 | LibraryPane | same | 4 | Local paths only; path gate from archive |
| D3 | Thumbnails | P0 | ThumbnailGenerator | WIC / MF thumbnail | 4 | |
| D4 | Custom thumbnail / reset | P1 | Library pane | same | 4 | |
| D5 | External library path | P2 | roadmap mac | Phase later | — | mac post-v1 |

---

## E. Gallery & Browse Web

| # | Feature | Pri | mac evidence | Windows target | Phase | Notes |
|---|---------|-----|--------------|----------------|-------|-------|
| E1 | Pixabay client | P0 | PixabayClient | HttpClient + user key DPAPI | 6 | |
| E2 | Pexels client | P0 | PexelsClient | same | 6 | |
| E3 | NASA client (no key) | P0 | NASAClient | same | 6 | |
| E4 | 24h search cache | P1 | GalleryCache | disk cache under AppData | 6 | |
| E5 | Add to library download | P0 | GalleryDownloader | same | 6 | |
| E6 | Browse Web curated links | P0 | ExternalSources | Process.Start browser | 6 | No scrape |
| E7 | In-gallery motion preview | P2 | known limitation mac | static thumbs OK first | — | |

---

## F. Performance & privacy

| # | Feature | Pri | mac evidence | Windows target | Phase | Notes |
|---|---------|-----|--------------|----------------|-------|-------|
| F1 | Pause on battery (configurable) | P0 | PerformanceGovernor | System Power Status | 7 | |
| F2 | Pause behind fullscreen | P0 | PerformanceGovernor | foreground fullscreen heuristics | 7 | DELTA: no exact mac fullscreen API |
| F3 | Thermal / resource pressure pause | P1 | thermal | CPU/GPU heuristics + power throttling | 7 | DELTA |
| F4 | Quality / FPS caps | P0 | Performance pane | same | 7 | |
| F5 | Memory limit per wallpaper | P1 | Performance | working set guard | 7 | |
| F6 | Diagnostics CPU/GPU/RAM | P1 | ResourceMonitor | PDH / DXGI best-effort | 7 | |
| F7 | Zero telemetry | P0 | README contract | architecture + tests | 1+ | Hard invariant |
| F8 | Network only on Gallery action | P0 | Gallery clients only | same | 6 | Cipher review |

---

## G. Ship & quality

| # | Feature | Pri | Windows target | Phase | Notes |
|---|---------|-----|----------------|-------|-------|
| G1 | GitHub CI build/test | P0 | windows-ci on `src/` | 1 | No cancel-in-progress on short gates |
| G2 | SAST CodeQL csharp | P0 | codeql | 1 | |
| G3 | DAST posture N/A→REQUIRED | P0 | docs/security | 1 / 6 | Flip when network surface |
| G4 | UI/install smoke | P0 | smoke scripts | 1–9 | |
| G5 | Installer + self-sign alpha | P0 | Inno or MSIX (Phase 9 ADR) | 9 | Archive Inno lessons |
| G6 | Real Windows screenshots in docs | P0 | docs/install | 9–10 | |
| G7 | GPL LICENSE + NOTICE | P0 | keep forever | all | |
| G8 | macOS tree retained | P0 | repo root | all | Reference only |

---

## H. WRQ-WIN-001 archive reuse (knowledge only)

| Asset | Path | Reuse |
|-------|------|--------|
| WorkerW host | `archive/wrq-win-001/windows/.../Host/` | Reference for Phase 3 host; rewrite into `src/Waraq.Windows.Host` |
| Path gate | Phase 3 secure | Port tests into new solution |
| Inno + sign | `archive/wrq-win-001/installer/` | Phase 9 starting point |
| CI patterns | `.github/workflows/*` | Retargeted to archive; Phase 1 adds `src/` paths |
| WPF shell UX | archive MainWindow | **Do not** ship as product UI |

---

## Honest platform deltas (summary)

1. **Chrome location:** menu bar → system tray.  
2. **Wallpaper level:** desktopIconWindow−1 → WorkerW/Progman (fragile across Win11 builds).  
3. **Thermal:** IOKit → power/CPU heuristics.  
4. **Codecs:** MF matrix ≠ AVFoundation; document + optional LibVLC.  
5. **Signing:** notarized Developer ID → self-signed alpha + SmartScreen honesty.  
6. **Fonts:** SF Pro → Segoe UI Variable / system UI font (token map in DESIGN.md).

---

## Phase 0 exit checklist

- [x] Matrix covers shell, engines, displays, library, gallery, performance, ship  
- [x] Pri tags for roadmap  
- [x] Windows DESIGN.md tokens map  
- [x] ADR 0003 stack  
- [x] Phase 1 scaffold recipe (`docs/parity/phase1-scaffold.md`)  

**Next:** Nova score gate → Phase 1 scaffold RO (empty WinUI shell + CI).
