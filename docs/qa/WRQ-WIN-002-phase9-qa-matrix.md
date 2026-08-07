# WRQ-WIN-002 Phase 9-QA — Installer matrix (Stagecraft QA)

| Field | Value |
|-------|-------|
| **status** | COMPLETE — GREEN (desk) |
| **desk** | Stagecraft QA |
| **date** | 2026-08-08 |
| **main baseline** | `0a2b661` (PR #25 Installer) |
| **draft release** | `win-v0.9.0-phase9` |
| **release run** | https://github.com/taroo0ooq/waraq-windows/actions/runs/31226784189 |

## Explicit not claimed
- Market publish / “Latest” release
- EV / Store / trusted-root Authenticode
- Auto-updater

## Draft assets verified

| Asset | Result |
|-------|--------|
| `Waraq.Windows-Setup-win-x64-0.9.0-phase9.exe` | SHA256 **OK** |
| `Waraq.Windows-win-x64-0.9.0-phase9.zip` | SHA256 **OK** |
| `WaraqWindows-CodeSigning.cer` | SHA256 **OK** · thumbprint `2AD29EBDBBC8DA13660D3A1AAAACF29A0F967406` |
| `SHA256SUMS-installer.txt` / `SHA256SUMS-portable.txt` / `SIGNATURES.txt` | present |

## Matrix

| ID | Case | Result | Notes |
|----|------|--------|-------|
| IN01 | Download draft release assets | **PASS** | `gh release download win-v0.9.0-phase9` |
| IN02 | SHA256 installer + CER | **PASS** | matches published sums |
| IN03 | SHA256 portable zip | **PASS** | |
| IN04 | Authenticode present on Setup | **PASS** | Status `UnknownError` / untrusted root — **expected** self-sign |
| IN05 | Authenticode present on App.exe (portable) | **PASS** | same self-sign chain; DigiCert timestamp present |
| IN06 | CER subject matches signer | **PASS** | `CN=Waraq Windows OSS Self-Signed (WRQ-WIN-002)` |
| IN07 | Portable extract + launch ≥5s | **PASS** | `Waraq.Windows.App.exe` |
| IN08 | Silent Setup `/VERYSILENT` to QA dir | **PASS** | exit 0 → `%LOCALAPPDATA%\Programs\WaraqWindows-QA-Phase9\` |
| IN09 | Installed app launch ≥5s | **PASS** | |
| IN10 | Silent uninstall | **PASS** | unins exit 0; app path removed |
| IN11 | Unit regression on main | **PASS** | **129/129** |
| IN12 | Main Actions (post #25) | **PASS** | windows-ci/playwright/codeql/dast/build SUCCESS |
| IN13 | Install docs present | **PASS** | `docs/install/WINDOWS.md` Phase 9 |
| IN14 | SmartScreen / trust UX | **SOFT residual** | self-sign; NotTrusted until CER import; no EV |
| IN15 | Wallpaper Apply after install | **SOFT residual** | interactive; covered by prior Host QA matrices |
| IN16 | Market publish | **OUT OF SCOPE** | hold |

## Main CI (PR #25 merge / release)

| Workflow | Result | URL |
|----------|--------|-----|
| windows-release | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31226784189 |
| windows-ci | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31226782494 |
| playwright | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31226782480 |
| codeql | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31226782488 |
| dast | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31226782483 |
| macos build | SUCCESS | https://github.com/taroo0ooq/waraq-windows/actions/runs/31226782481 |

Open blocking issues: **0**

## Local evidence path
`e2e/out/phase9-qa/` (local only; not shipped)

## Residuals
- SmartScreen / untrusted self-sign root (documented product residual)
- Draft release not “Latest” until owner
- Interactive wallpaper Apply post-install soft
- Market hold

## Verdict
**Phase 9-QA GREEN** for installer integrity, signature presence, portable + Setup install/launch/uninstall smoke.  
Does **not** authorize Market or EV claims.
