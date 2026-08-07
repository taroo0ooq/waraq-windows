# WRQ-WIN-002 Phase 6-Secure — Gallery network surface (Cipher Shield)

| Field | Value |
|-------|--------|
| **status** | COMPLETE (pending PR CI + Nova score) |
| **desk** | Cipher Shield |
| **date** | 2026-08-07 |
| **baseline** | main `5bca563` (PR #18 Gallery) |
| **owner** | `go` / EXECUTING in #cipher-shield |
| **repo** | https://github.com/taroo0ooq/waraq-windows |

## Executive summary

Phase 6 adds **optional user-initiated HTTPS client egress** (Gallery Search/Import + Browse Web). **No inbound listener** → classic DAST remains **N/A**, with egress scope documented.

**Findings:** no Critical; Medium client-SSRF / unbounded download **mitigated** via `GalleryUrlPolicy`. CodeQL expected notes on unmanaged + new network code; **0 Critical/High** product blockers.

## Trust boundaries

| Boundary | Trust |
|----------|--------|
| User Search/Import click | Explicit consent to egress |
| Pixabay/Pexels/NASA API JSON | **Untrusted** (drive download URL) |
| Browse Web catalog URLs | Hardcoded https in app; open in browser only |
| ApiKeyStore file | Local secrets (plaintext JSON residual) |

## STRIDE (Gallery)

| Threat | Sev | Disposition |
|--------|-----|-------------|
| SSRF via malicious DownloadUrl in API JSON | Medium | **Mitigated**: HTTPS-only, block localhost/private/reserved IPs, streamed size cap 512 MiB |
| Unbounded download DoS | Medium | **Mitigated**: MaxDownloadBytes |
| Key theft from disk | Medium residual | Local AppData; Hidden attribute best-effort; no cloud sync by app |
| Key in Pixabay query string | Low residual | Vendor API design |
| Browse Web → open malicious scheme | Low | BrowseWeb allows http(s) only |
| Scrape third-party wallpaper sites | — | **Not implemented** (browser only) |
| Telemetry | — | **None found** |
| Inbound listener / auth bypass | — | **N/A** |

## Code map

| Component | Path | Network? |
|-----------|------|----------|
| Search orchestration | `Gallery/GalleryClients.cs` | Yes on Search only |
| Keys | `Gallery/ApiKeyStore.cs` | No |
| Cache | `Gallery/GalleryCache.cs` | No |
| Download policy | `Gallery/GalleryUrlPolicy.cs` | Yes on Import (validated) |
| UI | `Views/GalleryPaneView.xaml.cs` | Calls search/download |
| Browse Web | `BrowseWeb.Open` | Browser only |

## SAST

- CodeQL csharp on main + Secure PR (Actions URLs on handoff)
- Triage: no Critical/High requiring blocking issues

## DAST

- **Inbound: N/A** — updated `docs/security/DAST.md`
- Egress documented; not a ZAP target

## Hardening delivered

1. `GalleryUrlPolicy` — safe HTTPS download validation + size-capped stream  
2. Import path uses policy  
3. ApiKeyStore marks keys file Hidden (best-effort)  
4. Tests for reject http/localhost/private IP  
5. DAST + PRIVACY docs updated  

## Residual

- Plaintext API keys on disk (OS user profile trust)  
- DNS rebinding TOCTOU on SSRF check  
- Vendor CDN host diversity (policy is IP-class based, not full CDN allowlist)  
- Interactive live Gallery demo soft residual  

## Verdict

| Gate | Result |
|------|--------|
| Critical/High open | **None** |
| Blocking issues | **None** |
| DAST inbound | **N/A green** (documented egress) |
| Stage 6-Secure | **GREEN** after PR CI + Nova |

## Ask Nova

Score gate → merge Secure PR → route **Stagecraft 6-QA**. Cipher does not start QA.
