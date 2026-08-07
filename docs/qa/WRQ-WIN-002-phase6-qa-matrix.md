# WRQ-WIN-002 Phase 6-QA — Gallery privacy matrix (Stagecraft QA)

| Field | Value |
|-------|-------|
| **status** | COMPLETE — GREEN |
| **desk** | Stagecraft QA |
| **date** | 2026-08-07 |
| **main baseline** | `c567811` (PR #19 Secure) |
| **QA branch** | `feature/wrq-win-002-phase6-qa` |

## Main Actions (PR #19 merge)

| Workflow | Result | URL |
|----------|--------|-----|
| windows-ci | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31221431684 |
| playwright | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31221431924 |
| dast | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31221431896 |
| macos build | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31221431682 |
| codeql | pending/watch PR QA | main run 31221431895 |

Open blocking issues: **0**

## Local

| Suite | Result |
|-------|--------|
| `dotnet test` | **PASS 106/106** |
| e2e smoke | **PASS** |

## Matrix

| ID | Case | Result |
|----|------|--------|
| G01 | ApiKey store round-trip + blank clear | **PASS** |
| G02 | NASA no key; Pixabay/Pexels require key | **PASS** |
| G03 | BrowseWeb rejects file/ftp/invalid | **PASS** |
| G04 | ExternalBrowse 4 sites HTTPS-only | **PASS** |
| G05 | Search cache second call no network | **PASS** |
| G06 | Missing Pixabay key no network | **PASS** |
| G07 | URL policy rejects http/file/ftp/private IP/localhost/userinfo/empty | **PASS** |
| G08 | URL policy allows public HTTPS | **PASS** |
| G09 | Max download 512 MiB constant | **PASS** |
| G10 | Host probe no-regression | **PASS** |
| G11 | Procedural catalog still 6 | **PASS** |
| G12 | Library GIF import no-regression | **PASS** |
| G13 | Interactive Gallery search/import UI | **SOFT residual** |

## Residuals
- Interactive Gallery UX soft
- DNS rebinding TOCTOU (Cipher residual)
- Phase 2 L&F parallel

## Verdict
**Phase 6-QA GREEN**
