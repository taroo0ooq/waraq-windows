# DAST posture — Waraq Windows

| Field | Value |
|-------|--------|
| **Status** | **N/A** (Not applicable) |
| **work_id** | WRQ-WIN-002 |
| **phase** | 3-Secure (Cipher Shield) — Host video/GIF on WinUI |
| **last_reviewed** | 2026-08-07 |
| **reviewer_desk** | Cipher Shield |
| **owner_desk** | Pipeline Warden (gate) → Cipher Shield (deep scans when surface exists) |
| **main_sha_reviewed** | `4c741e7` (+ Secure harden PR if open) |

## Why N/A

The Windows product surface through Phase 3 Host is a **local WinUI 3 desktop application** (`src/Waraq.Windows.App`):

- No HTTP listener
- No embedded web server
- No authenticated network API owned by this app binary
- No `HttpClient` / socket client in the `src/**` Windows tree
- Gallery network clients (upstream macOS) are **not** wired into the Windows binary
- Media load is restricted to **local drive files** (`LocalMediaPathGate` — UNC/URL/device rejected; post-normalize re-check + size soft-caps)

Dynamic Application Security Testing (DAST) requires a running network-reachable target (URL). None exists for the Windows app today.

## Gate policy

| Condition | Required outcome |
|-----------|------------------|
| No network attack surface | Workflow `.github/workflows/dast.yml` **green** with this document declaring **Status: N/A** |
| Network surface introduced (local API, updater endpoint, web UI, gallery client, etc.) | Update this doc to **Status: REQUIRED**; replace stub job with real DAST (e.g. ZAP baseline against documented URL); Cipher Shield owns deep findings |

## When to flip from N/A

1. App hosts or proxies HTTP(S) locally or remotely
2. Installer/update channel serves content that must be probed
3. Web-based settings/gallery UI is shipped beside WinUI
4. Outbound gallery/API clients are enabled in the Windows binary
5. Nova routes a Secure phase that names a concrete DAST target URL

## Related workflows

- SAST: `.github/workflows/codeql.yml` (CodeQL csharp)
- Build/test: `.github/workflows/windows-ci.yml`
- Playwright / Windows QA smoke: `.github/workflows/playwright.yml`
- macOS upstream CI (unchanged): `.github/workflows/build.yml`
- DAST gate workflow: `.github/workflows/dast.yml`

## Evidence

- Secure review: `docs/security/WRQ-WIN-002-phase3-secure-review.md`
- Prior (superseded product line): `docs/security/WRQ-WIN-001-phase3-secure-review.md`
