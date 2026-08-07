# WRQ-WIN-002 Phase 7-QA — Governor matrix (Stagecraft QA)

| Field | Value |
|-------|-------|
| **status** | COMPLETE — GREEN |
| **desk** | Stagecraft QA |
| **date** | 2026-08-08 |
| **main baseline** | `5a55c6c` (PR #21 Governor) |
| **QA branch** | `feature/wrq-win-002-phase7-qa` |

## Main Actions (PR #21 merge)

| Workflow | Result | URL |
|----------|--------|-----|
| windows-ci | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31223974092 |
| playwright | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31223973840 |
| dast | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31223974075 |
| macos build | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31223973799 |
| codeql | watch PR/main | 31223973803 |

Open blocking issues: **0**

## Local

| Suite | Result |
|-------|--------|
| `dotnet test` | **PASS 120/120** |
| e2e smoke | **PASS** |

## Matrix

| ID | Case | Result |
|----|------|--------|
| GV01 | User pause wins over battery/fullscreen/memory | **PASS** |
| GV02 | Battery threshold pause | **PASS** |
| GV03 | Battery above threshold plays | **PASS** |
| GV04 | AC power does not battery-pause | **PASS** |
| GV05 | Fullscreen pause | **PASS** |
| GV06 | High memory pause | **PASS** |
| GV07 | Governor disabled plays | **PASS** |
| GV08 | Clear conditions play | **PASS** |
| GV09 | Fullscreen priority before battery | **PASS** |
| GV10 | Settings round-trip | **PASS** |
| GV11 | Onboarding 5 steps + complete/reset | **PASS** |
| GV12 | Diagnostics probes (battery/WS/fullscreen API) | **PASS** |
| GV13 | Host/Procedural/Gallery no-regression | **PASS** |
| GV14 | Interactive desktop demo | **SOFT residual** |

## Residuals
- Interactive owner demo soft · no thermal claim · Phase 2 L&F parallel

## Verdict
**Phase 7-QA GREEN**
