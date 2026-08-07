# ADR 0003 — WRQ-WIN-002 stack (WinUI 3 default)

| Field | Value |
|-------|--------|
| **Status** | **Accepted (Phase 0)** |
| **Date** | 2026-08-07 |
| **Work ID** | WRQ-WIN-002 |
| **Deciders** | Atlas Forge (Phase 0 RO); owner APPROVE scrap+parity restart |
| **Supersedes (product UX)** | ADR 0001 WPF MVP path as *shipped product* baseline |
| **Retains** | ADR 0001 WorkerW host research; ADR 0002 installer lessons (Phase 9) |

## Context

WRQ-WIN-001 delivered a functional WorkerW + video/GIF probe with a utilitarian WPF shell. Owner rejected that UX as the product direction: Windows Waraq must match **macOS v1.0.0 look & feel and features** (tray + multi-pane Settings, gallery, procedural, profiles, etc.).

Constraints:

- GPL-3.0; keep LICENSE/NOTICE; keep macOS tree at repo root.
- Archive MVP under `archive/wrq-win-001/` (Mode A).
- Agency gates: GitHub CI, SAST, DAST posture, smoke/QA each phase.
- Multi-monitor live wallpaper host remains required (WorkerW or documented fallback).

## Decision

### 1) UI framework: **WinUI 3 + Windows App SDK** (primary)

| Choice | Detail |
|--------|--------|
| Toolkit | WinUI 3 (Windows App SDK stable channel) |
| Runtime | .NET 8 (Windows TFMs, e.g. `net8.0-windows10.0.19041.0`) |
| Pattern | MVVM; thin code-behind; design tokens in ResourceDictionaries (`docs/design/windows/DESIGN.md`) |
| Shell | Tray icon + Settings window with left navigation mirroring `settings-shell.md` |
| Packaging (Phase 9) | Prefer MSIX unpackaged or packaged unpackaged-friendly; Inno sideload remains fallback from ADR 0002 |

**Why WinUI over continuing WPF as product UI**

- NavigationView / modern chrome closer to macOS System Settings–style sidebar than classic WPF.
- Mica/Acrylic and Fluent controls reduce “1990s utility” feel that tanked WRQ-WIN-001 L&F.
- Long-term Windows desktop direction from Microsoft.

**Fallback (only if Phase 0/1 spike fails hard)**

- **WPF + Wpf.Ui / ModernWpf** with *strict* DESIGN.md token compliance.
- Trigger: cannot build WinUI template on agent CI or owner machines within spike budget; document blockers in spike log.
- Fallback does **not** revive archive MainWindow layout as-is.

### 2) Process model: **Shell + Host modules** (same solution; optional separate host process later)

```
Tray/Settings (UI process) ──IPC/commands──> Wallpaper Host (WorkerW surfaces)
```

Phase 1–3 may start **in-process** host for speed, with interfaces that allow **out-of-process** host (Phase 3+) so a host crash does not kill tray.

### 3) Wallpaper host: **WorkerW (Progman)** primary

- Reuse algorithms and lessons from `archive/wrq-win-001/.../Host` and ADR 0001.
- Rewrite into `src/Waraq.Windows.Host` (clean namespace; tests ported).
- Fallback research spike if Win11 build breaks WorkerW (under-desktop diagnostic only).

### 4) Media stack

| Kind | Primary | Fallback |
|------|---------|----------|
| Video | Media Foundation / MediaPlayerElement or custom MF pipeline | LibVLC interop if codec gaps |
| GIF | WIC / frame decoder | Archive GifBitmapDecoder approach |
| Procedural | Win2D or SkiaSharp composition targets | — |
| Thumbnails | WIC + MF | — |

### 5) Data & privacy

- Library: `%AppData%\Waraq\`
- Settings: ApplicationData local settings / JSON as needed
- API keys: **DPAPI** or Windows Credential Locker (not plaintext)
- Zero telemetry; HttpClient only in Gallery module
- Local media path gate (no UNC/URL) until Gallery download path

### 6) Testing & CI

- xUnit for domain (profiles, path gate, governor pure logic)
- UI smoke scripts (not necessarily Playwright for WinUI desktop)
- CI: `windows-latest`, restore/build/test `src/`; CodeQL csharp; DAST N/A doc until Gallery; **no cancel-in-progress** on short required gates

## Options considered

| Option | Verdict |
|--------|---------|
| A. Stay on WPF product UI | Rejected for parity L&F; archive only |
| B. WinUI 3 + WASDK | **Accepted** |
| C. Electron/Tauri | Rejected (memory, host still native, GPL/deps) |
| D. Pure Rust GUI | Rejected for Settings velocity |
| E. Avalonia | Rejected; less Fluent native than WinUI for this product |

## Consequences

- Phase 1 creates new `src/` tree; does not delete archive.
- Design tokens implemented before pixel-pushing panes (Phase 2 owner visual gate).
- Installer ADR 0002 remains relevant at Phase 9; may add MSIX decision then.
- Engineers need Windows App SDK workload + VS 2022 or `winget` templates.

## Spike checklist (Phase 0 — not full implement)

| # | Check | Result (2026-08-07) |
|---|--------|---------------------|
| S1 | .NET 8 SDK present | **OK** — 8.0.423 on build agent host |
| S2 | `dotnet new` WinUI / WASDK template | **BLOCKED on this agent** without Windows App SDK workload / VS WinUI workload — Phase 1 must install `Microsoft.WindowsAppSDK` via project packages + document VS components |
| S3 | WorkerW reference code exists | **OK** — `archive/wrq-win-001/windows/` |
| S4 | Design specs + screenshots available | **OK** — `docs/design/*`, `docs/screenshots/*` |
| S5 | CI path retarget for archive done | **OK** — PR #7 / main |

**Phase 1 gate:** first PR must show `dotnet build` green for empty WinUI (or documented fallback activation) on `windows-latest`.

## References

- Master plan: Agency vault `00-Home/WRQ-WIN-002-Parity-Rebuild-Master-Plan.md`
- Parity matrix: `docs/parity/WRQ-WIN-002-matrix.md`
- Design: `docs/design/windows/DESIGN.md`
- Scaffold: `docs/parity/phase1-scaffold.md`
- ADR 0001 host: `docs/adr/0001-windows-tech-stack-and-wallpaper-host.md`
- ADR 0002 installer: `docs/adr/0002-windows-installer-and-signing.md`
