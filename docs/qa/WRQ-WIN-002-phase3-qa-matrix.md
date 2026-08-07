# WRQ-WIN-002 Phase 3-QA — matrix (Stagecraft QA)

| Field | Value |
|-------|-------|
| **status** | COMPLETE — GREEN |
| **desk** | Stagecraft QA |
| **date** | 2026-08-07 |
| **repo** | https://github.com/taroo0ooq/waraq-windows |
| **main baseline** | `cdd2f8a` (PR #12 Secure) |
| **QA branch** | `feature/wrq-win-002-phase3-qa` |
| **OS** | Windows 11 Pro local + windows-latest CI |

## Scope

Host video/GIF Apply/Stop quality certification after Phase 3-Secure.

## Main Actions (post-merge PR #12 → cdd2f8a)

| Workflow | Result | URL |
|----------|--------|-----|
| windows-ci | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31213586827 |
| playwright | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31213586826 |
| codeql | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31213587067 |
| dast | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31213586757 |
| macos build | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31213586828 |

Open blocking issues: **0**

## Automated / local (2026-08-07)

| Suite | Result | Notes |
|-------|--------|-------|
| `dotnet test` Release | **PASS 42/42** | Expanded path gate + host + classifier |
| `./e2e/windows-qa-smoke.ps1` | **PASS** | App launch 4s OK |
| Host probe | **PASS** | Message non-empty |
| Virtual screen multi-monitor | **PASS** | w/h positive; MVP one surface |

## Manual matrix

| ID | Case | Expected | Result | Evidence |
|----|------|----------|--------|----------|
| M01 | Build Release tests + App x64 | 0 errors | **PASS** | smoke |
| M02 | Path reject URL | InvalidOperation | **PASS** | unit |
| M03 | Path reject UNC | InvalidOperation | **PASS** | unit |
| M04 | Path reject missing | FileNotFound | **PASS** | unit |
| M05 | Path accept temp local | full path | **PASS** | unit |
| M06 | Oversized GIF/video caps | reject | **PASS** | unit |
| M07 | Classifier video/GIF/image | correct kinds | **PASS** | unit |
| M08 | Phase3 playable only V/G | image false | **PASS** | unit |
| M09 | Host Probe | message non-empty | **PASS** | unit |
| M10 | App launch 4s | process stays alive | **PASS** | smoke |
| M11 | Tray Stop wallpaper | CMD_PAUSE → Stop | **PASS** (code review) | TrayIconService.cs |
| M12 | Exit cleanup | ShutdownWallpaper | **PASS** (code review) | App.xaml.cs |
| M13 | Interactive Apply visual | wallpaper behind icons | **SOFT residual** | RO-allowed |
| M14 | Multi-monitor | virtual desktop surface | **PASS** | design + test |

## Residuals (non-blocking)

1. Owner interactive Apply visual proof (RO soft residual)
2. Phase 2 L&F owner accept (parallel)
3. Browser Playwright N/A
4. FlaUI tray click deferred
5. MF codec variance

## Verdict

| Gate | Result |
|------|--------|
| Automated path/host/smoke | **PASS 42/42 + smoke** |
| Main Actions green cdd2f8a | **PASS** |
| Blocking issues | **0** |
| Phase 3-QA | **GREEN** |

## Ask Nova

Score gate CONTINUE → close Phase 3-QA → route Phase 4+ per master plan.
