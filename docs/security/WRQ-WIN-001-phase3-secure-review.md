# WRQ-WIN-001 Phase 3 — Secure review (Cipher Shield)

| Field | Value |
|-------|--------|
| **status** | COMPLETE (pending PR CI green + Nova gate) |
| **desk** | Cipher Shield |
| **date** | 2026-08-06 |
| **scope** | Windows MVP on `main` post Phase 2 (`3d46d98`) + hardening PR |
| **repo** | https://github.com/taroo0ooq/waraq-windows |
| **local** | `C:\Users\justb\waraq-windows` |

## Executive summary

Local WPF live-wallpaper MVP (WorkerW + video/GIF). **No network attack surface.** CodeQL open alerts are **note**-severity only (unmanaged P/Invoke expected; broad catch on teardown). **No Critical/High** findings requiring blocking issues.

Phase 3 delivered **defense-in-depth** on media path loading: reject UNC/URL/device paths, require local drive files, soft size caps, file URI construction from validated paths.

## Trust model

| Actor | Trust |
|-------|--------|
| Interactive local user | Fully trusted to choose wallpaper media they can read |
| Network / remote share | **Untrusted** — must not become media source via path string |
| Explorer / Progman / WorkerW | OS shell peer; required for host strategy (accepted design risk) |
| Media Foundation / WIC codecs | OS components; malicious file → decoder bugs are OS surface |

## STRIDE (condensed)

| Category | Finding | Severity | Disposition |
|----------|---------|----------|-------------|
| Spoofing | No authn identity; single-user desktop app | Info | N/A |
| Tampering | Local file content controlled by user | Info | User intent |
| Repudiation | No audit log of applied paths | Low | Accept for MVP; consider later diagnostics |
| Info disclosure | Probe shows Progman/WorkerW HWND hex | Low | Local UI only; accept |
| DoS | Huge GIF decoded fully into RAM | Medium | **Mitigated**: 64 MiB GIF / 4 GiB video soft caps |
| DoS | WorkerW attach failure / Explorer quirks | Low | UX failure, not security boundary |
| EoP | P/Invoke SetParent onto desktop WorkerW | Info/Accepted | Required for product; runs as user, no admin required |
| Network egress | UNC or `http(s)` path into MediaElement | Medium | **Mitigated**: `LocalMediaPath` rejects UNC/URL |
| Secrets / telemetry | Hardcoded secrets / phone-home | — | **None found** in `windows/**` |
| Supply chain | App package refs | Info | App has **zero** NuGet deps; tests only |

## P/Invoke surface

All in `windows/Waraq.Windows/Host/NativeMethods.cs` → `user32.dll` only:

- `FindWindow` / `FindWindowEx` / `EnumWindows` / `GetClassName`
- `SendMessageTimeout` (WM `0x052C` spawn WorkerW)
- `SetParent` / `GetWindowLong` / `SetWindowLong` / `SetWindowPos` / `ShowWindow`
- `GetSystemMetrics` / `IsWindow`

No process create, no token APIs, no `kernel32` dangerous imports, no injection APIs.

**Accepted risk:** Desktop shell manipulation is inherent to live wallpapers. Documented in ADR 0001.

**Note:** `GetWindowLong`/`SetWindowLong` (32-bit) on 64-bit — correctness/quality; not rated High for this HWND style flags use.

## SAST (CodeQL)

- Workflow: `.github/workflows/codeql.yml` — csharp, `security-and-quality`, manual build
- Open alerts reviewed: severity **note** only (`cs/unmanaged-code`, `cs/call-to-unmanaged-code`, `cs/catch-of-all-exceptions`, one test `cs/path-combine`)
- **No error/warning security alerts** at Critical/High

## DAST

- **Status: N/A** — reaffirmed Phase 3 (`docs/security/DAST.md`)
- Workflow `.github/workflows/dast.yml` remains documentation gate

## Secrets / deps

- No API keys, tokens, or credentials in Windows sources
- `AppInfo` public GitHub URLs only
- App csproj: framework references only (WPF)

## Hardening delivered (this phase)

| Change | Path |
|--------|------|
| Local path allowlist + UNC/URL reject | `windows/Waraq.Windows/Engines/LocalMediaPath.cs` |
| Apply() uses guard + size limits | `Host/WallpaperController.cs` |
| Engines use `LocalMediaPath.ToFileUri` | `Engines/WallpaperViews.cs` |
| Regression tests | `Waraq.Windows.Tests` |
| DAST reaffirm | `docs/security/DAST.md` |
| This review | `docs/security/WRQ-WIN-001-phase3-secure-review.md` |

## Residual risks (non-blocking)

1. **Decoder bugs** in MF/WIC on malicious media — OS surface; keep local-only input
2. **No battery/fullscreen pause** — availability/perf (product), not AppSec Critical
3. **CodeQL notes** for broad catch / unmanaged — noise relative to intentional design
4. **Future gallery** will introduce network → flip DAST + threat model
5. **Installer/signing** not in scope (Ship later)

## Verdict

| Gate | Result |
|------|--------|
| High/Critical open | **None** |
| Blocking GitHub issues | **None** required |
| DAST | **N/A green** (documented) |
| SAST | **Notes only**; gates present |
| Stage 3 Secure | **GREEN** after hardening PR CI green + merge (Nova) |

## Ask Nova

Close Phase 3 when PR checks green + merge; route Stagecraft QA (Playwright) next. Do not start Market/Ship from this desk.
