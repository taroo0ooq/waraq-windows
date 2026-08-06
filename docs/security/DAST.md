# DAST posture — Waraq Windows

| Field | Value |
|-------|--------|
| **Status** | **N/A** (Not applicable) |
| **work_id** | WRQ-WIN-001 |
| **phase** | 1b (CI/CD baseline) |
| **last_reviewed** | 2026-08-06 |
| **owner_desk** | Pipeline Warden (gate) → Cipher Shield (deep scans when surface exists) |

## Why N/A

The Windows product surface in Phase 1a/1b is a **local WPF desktop application** (`windows/Waraq.Windows`):

- No HTTP listener
- No embedded web server
- No authenticated network API owned by this app binary
- Gallery network clients (upstream macOS) are not wired into the Windows scaffold yet

Dynamic Application Security Testing (DAST) requires a running network-reachable target (URL). None exists for the Windows scaffold today.

## Gate policy

| Condition | Required outcome |
|-----------|------------------|
| No network attack surface | Workflow `.github/workflows/dast.yml` **green** with this document declaring **Status: N/A** |
| Network surface introduced (local API, updater endpoint, web UI, etc.) | Update this doc to **Status: REQUIRED**; replace stub job with real DAST (e.g. ZAP baseline against documented URL); Cipher Shield owns deep findings |

## When to flip from N/A

Flip **Status** away from N/A when any of the following land:

1. App hosts or proxies HTTP(S) locally or remotely
2. Installer/update channel serves content that must be probed
3. Web-based settings/gallery UI is shipped beside WPF
4. Nova routes a Secure phase that names a concrete DAST target URL

## Related workflows

- SAST: `.github/workflows/codeql.yml` (CodeQL csharp)
- Build/test: `.github/workflows/windows-ci.yml`
- Playwright stub: `.github/workflows/playwright.yml`
- macOS upstream CI (unchanged): `.github/workflows/build.yml`
