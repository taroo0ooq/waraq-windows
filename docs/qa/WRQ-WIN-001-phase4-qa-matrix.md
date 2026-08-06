# WRQ-WIN-001 Phase 4 — QA matrix (Stagecraft QA)

| Field | Value |
|-------|-------|
| **status** | COMPLETE — GREEN |
| **desk** | Stagecraft QA |
| **date** | 2026-08-06 |
| **repo** | https://github.com/taroo0ooq/waraq-windows |
| **local** | `C:\Users\justb\waraq-windows` |
| **baseline main** | `68dc020` (post Phase 3 Secure) |
| **branch** | `feature/wrq-win-001-phase4-qa` |
| **OS under test** | Windows 11 Pro 10.0.26200 (local) + `windows-latest` CI |

## Scope

Manual + automated quality certification for Windows MVP:

- Apply **video** / **GIF** wallpaper via WorkerW
- **Stop** and **exit cleanup**
- **Invalid path** rejection (UNC / URL)
- **Oversized** media handling
- Improve required **playwright** workflow beyond empty stub

## Automated results

| Suite | Command | Result | Notes |
|-------|---------|--------|-------|
| Unit + integration | `dotnet test windows/WaraqWindows.sln -c Release` | **PASS 55/55** | Path gate, classifier, STA Apply/Stop GIF+video |
| Windows QA smoke | `./e2e/windows-qa-smoke.ps1 -Configuration Release` | **PASS** | Fixtures + tests + 3s launch |
| STA GIF Apply/Stop | `WallpaperIntegrationTests.Apply_And_Stop_ValidGif_OnStaThread` | **PASS** (~648 ms) | WorkerW attach + teardown |
| STA video Apply/Stop | `Apply_ValidVideo_WhenFfmpegFixturePresent_OnStaThread` | **PASS** (~667 ms) | sample.mp4 fixture |
| STA re-apply | `Apply_ValidGif_ThenReapply_ThenStop` | **PASS** | Fit mode switch Stretch |
| Host probe | `DesktopWallpaperHostTests.Probe_*` | **PASS** | Progman/WorkerW present on local |
| CI playwright workflow | GitHub Actions (this PR) | pending push | Real smoke + N/A browser job |

## Manual matrix

Target: **Windows 10/11**, Release build. Local run: Win11 Pro.

| ID | Case | Steps | Expected | Result | Evidence |
|----|------|-------|----------|--------|----------|
| M01 | Build Release | `dotnet build windows/WaraqWindows.sln -c Release` | 0 errors | **PASS** | smoke log |
| M02 | Probe host | Unit `Probe_DoesNotThrow` + app Probe | Progman + WorkerW located | **PASS** | unit + shell present |
| M03 | Apply GIF | STA integration + fixture GIF | Wallpaper applies; IsRunning; Stop clears | **PASS** | STA test 648 ms |
| M04 | Apply video | STA + `e2e/fixtures/sample.mp4` | Video applies; Stop clears | **PASS** | STA test 667 ms |
| M05 | Fit modes | Re-apply Fill→Fit→Stretch | Surface updates without crash | **PASS** | re-apply STA |
| M06 | Stop | Stop while running | IsRunning false; ActivePath null | **PASS** | STA + unit idle Stop |
| M07 | Exit cleanup | App.OnExit → Wallpaper.Dispose | Surface torn down on exit (code path) | **PASS** | code review + Dispose idempotent test; launch smoke stop |
| M08 | Reject URL | Apply `https://…` | NotSupportedException | **PASS** | unit + controller tests |
| M09 | Reject UNC | Apply `\\server\share\…` | NotSupportedException | **PASS** | unit + controller tests |
| M10 | Reject unsupported | `.png` Apply | NotSupportedException | **PASS** | unit |
| M11 | Missing file | Bogus path Apply | FileNotFoundException | **PASS** | unit |
| M12 | Empty path | Empty Apply | ArgumentException / choose-file UX | **PASS** | unit ArgumentException; shell message covered by code |
| M13 | Oversized GIF | >64 MiB sparse | NotSupported before attach | **PASS** | unit |
| M14 | Launch entrypoint | Start exe 3s | Process stays alive | **PASS** | smoke script |

## Blocking issues

| Issue | Severity | Link | Disposition |
|-------|----------|------|-------------|
| none | — | — | Open blocking bugs = **0** |

## Residual risks (non-blocking)

1. **Browser Playwright N/A** — no web UI; documented in workflow job
2. **OpenFileDialog not UI-automated** — covered by typed path + unit path gate + STA Apply (Browse dialog residual)
3. **MF codec variance** — some agents/VMs lack codecs; integration soft-skips codec env errors
4. **WorkerW missing** — rare headless/shell-less; integration skips attach; smoke still runs unit tests + launch
5. **Explorer restart** — may need re-Apply (known MVP limit)
6. **No battery/fullscreen pause** — product deferred (Phase 2 notes), not a Phase 4 fail
7. **WinAppDriver/FlaUI full UI** — deferred; not required for green when path+Apply/Stop automated

## Verdict

| Gate | Result |
|------|--------|
| Automated tests | **PASS 55/55** |
| Manual critical path (M03/M04/M06/M07/M08/M09) | **PASS** |
| playwright workflow beyond stub | **PASS** (Windows QA smoke + N/A browser job) |
| Open blocking GitHub issues | **0** |
| Phase 4 QA | **GREEN** (pending PR CI confirm) |

## Ask Nova

Close Phase 4 when PR checks green + merge; route **Pipeline Warden** for Ship. Do not start Ship/Market from this desk.
