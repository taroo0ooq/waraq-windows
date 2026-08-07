# WRQ-WIN-002 Phase 3-Secure — Cipher Shield review

| Field | Value |
|-------|--------|
| **status** | COMPLETE (pending harden PR CI + Nova score) |
| **desk** | Cipher Shield |
| **date** | 2026-08-07 |
| **scope** | Host video/GIF on main `4c741e7` (PR #11) + path-gate harden |
| **repo** | https://github.com/taroo0ooq/waraq-windows |
| **local** | `C:\Users\justb\waraq-windows` |
| **owner_start** | EXECUTING in #cipher-shield `1535372634701832212` |

## Executive summary

WinUI 3 local live-wallpaper host (WorkerW + video/GIF Apply/Stop + tray Stop). **No network attack surface.** CodeQL open findings: **note** (unmanaged/PInvoke expected) and **warning** only in **generated** `obj/**` XAML code — **0 Critical/High**.

Phase 3-Secure delivers written threat model + DAST N/A reaffirm for WinUI + **defense-in-depth** on `LocalMediaPathGate` (normalize + re-gate + drive letter + size soft-caps).

## Trust model

| Actor | Trust |
|-------|--------|
| Interactive local user | Trusted to pick wallpaper media they can read |
| Network / remote share | **Untrusted** — must not become media source |
| Explorer Progman/WorkerW | OS shell peer (accepted product design) |
| Media Foundation / WIC | OS decoders — malicious file = OS surface |

## STRIDE (condensed)

| Category | Finding | Sev | Disposition |
|----------|---------|-----|-------------|
| Spoofing | No app authn | Info | N/A desktop single-user |
| Tampering | User-chosen local files | Info | Intentional |
| Info disclosure | Active path shown in UI status | Low | Local UI only |
| DoS | Huge GIF decode | Medium | **Mitigated** 64 MiB GIF / 4 GiB video caps |
| Network egress via media path | UNC/URL into MediaPlayer/BitmapImage | Medium | **Mitigated** path gate + post-normalize |
| EoP | WorkerW SetParent / tray P/Invoke | Accepted | user32/shell32/comctl32 only; no admin required |
| Secrets / telemetry | Hardcoded secrets / phone-home | — | **None** in `src/**` |

## P/Invoke surface

### Host (`DesktopWallpaperHost` / `NativeMethods`)
`user32.dll` only: FindWindow/Ex, EnumWindows, SendMessageTimeout (0x052C), SetParent, Get/SetWindowLong, SetWindowPos, ShowWindow, GetSystemMetrics, IsWindow, GetClassName.

### Tray (`TrayIconService`)
`shell32` Shell_NotifyIcon · `user32` menu/cursor/subclass · `comctl32` SetWindowSubclass — tray UX only; **Stop wallpaper** calls `App.Wallpaper.Stop()` (local teardown).

### Bootstrap
`Microsoft.ui.xaml.dll` XamlCheckProcessRequirements (WinUI pack).

No process create, no token APIs, no kernel32 dangerous imports.

## SAST (CodeQL)

- Workflow green on main merge #11: https://github.com/taroo0ooq/waraq-windows/actions/runs/31210385579
- Open alerts: notes (unmanaged) + warnings in **generated** obj XAML — not product source Critical/High
- Triage: **no blocking security issues** opened

## DAST

- **Status: N/A** — `docs/security/DAST.md` updated for WRQ-WIN-002 WinUI Host
- Main dast run: https://github.com/taroo0ooq/waraq-windows/actions/runs/31210385603

## Hardening (this phase)

| Change | Path |
|--------|------|
| Normalize + re-gate + drive letter + size caps | `src/Waraq.Windows.Core/LocalMediaPathGate.cs` |
| ApplyAsync uses NormalizeExistingLocalFile | `HostRuntime/WallpaperController.cs` |
| Extra gate tests | `Waraq.Windows.Tests` |
| DAST reaffirm | `docs/security/DAST.md` |
| This review | `docs/security/WRQ-WIN-002-phase3-secure-review.md` |

## Residual (non-blocking)

1. OS decoder bugs on crafted media  
2. WorkerW shell variance across Explorer builds  
3. Interactive desktop Apply live proof (product residual from Atlas)  
4. CodeQL notes on intentional P/Invoke  
5. Future Gallery → flip DAST  

## Verdict

| Gate | Result |
|------|--------|
| Critical/High | **None** |
| Blocking issues | **None** |
| DAST | **N/A green** |
| SAST | green (notes/generated warnings only) |
| Stage 3-Secure | **GREEN** after harden PR CI + merge (Nova) |

## Ask Nova

Score gate → merge Secure PR when CLEAN → route **Stagecraft QA**. Do not start QA/Market from Cipher.
