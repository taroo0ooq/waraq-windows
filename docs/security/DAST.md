# DAST posture — Waraq Windows

| Field | Value |
|-------|--------|
| **Status** | **N/A** (Not applicable — no inbound network listener) |
| **work_id** | WRQ-WIN-002 |
| **phase** | **6-Secure** (Cipher Shield) — Gallery optional egress |
| **last_reviewed** | 2026-08-07 |
| **reviewer_desk** | Cipher Shield |
| **main_sha_baseline** | `5bca563` (PR #18 Gallery) + Phase 6-Secure harden PR |

## Inbound DAST (ZAP / dynamic scan of app URL)

**Still N/A.** The WinUI app does **not**:

- Host an HTTP listener
- Embed a web server
- Expose an authenticated local API

There is **no durable URL target** for traditional DAST. Workflow `.github/workflows/dast.yml` remains a documentation gate requiring this file to declare **Status: N/A** (or Not applicable).

## Outbound / egress surface (Phase 6 Gallery)

Gallery introduces **optional user-initiated HTTPS client egress only**:

| Trigger | Destination family | Notes |
|---------|-------------------|--------|
| Search | Pixabay / Pexels / NASA Images API | Only after explicit Search; keys local |
| Import selected | HTTPS media URLs from result JSON | Validated by `GalleryUrlPolicy` (https, no private hosts, 512 MiB cap) |
| Browse Web / Open page | Default browser (`ShellExecute`) | No scrape/proxy of third-party wallpaper sites |

**Not DAST-in-scope as an app server**, but **in Secure scope** as client SSRF/privacy review (see `docs/security/WRQ-WIN-002-phase6-secure-review.md` and `PRIVACY_GALLERY.md`).

## When inbound Status flips from N/A

1. App hosts or proxies HTTP(S) locally or remotely  
2. Installer/update channel must be probed as a service  
3. Embedded web UI becomes a network target  
4. Nova names a concrete DAST target URL  

Outbound Gallery alone does **not** flip inbound DAST to REQUIRED.

## Gate policy

| Condition | Outcome |
|-----------|---------|
| No inbound listener + this doc Status N/A | `dast.yml` **green** |
| Inbound surface appears | Status → **REQUIRED**; real DAST job; Cipher owns findings |

## Related

- SAST: `.github/workflows/codeql.yml`  
- Build/test: `.github/workflows/windows-ci.yml`  
- Playwright: `.github/workflows/playwright.yml`  
- Privacy: `docs/security/PRIVACY_GALLERY.md`  
- Phase 6-Secure review: `docs/security/WRQ-WIN-002-phase6-secure-review.md`  
