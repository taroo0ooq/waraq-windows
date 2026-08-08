# Waraq Windows — Feature Enhancement Pack (Agency Desks)

| Field | Value |
|-------|--------|
| **doc_id** | `WRQ-ENH-001` |
| **title** | Full feature enhancement backlog from macOS Waraq Settings (v1 UX) |
| **status** | READY FOR NOVA ROUTING |
| **created** | 2026-08-08 |
| **source** | Owner screenshots of macOS **Waraq Settings** (Displays, Library, Gallery Online Sources, Gallery Browse Web) + product README + `docs/parity/WRQ-WIN-002-matrix.md` |
| **product** | [taroo0ooq/waraq-windows](https://github.com/taroo0ooq/waraq-windows) (GPL-3.0 derivative of bahamut42/waraq) |
| **related** | `WRQ-WIN-001` (bootstrap) · `WRQ-WIN-002` (parity rebuild) · open HF1 defects if any |
| **orchestrator** | Gatekeeper Nova |
| **quality gates** | Standing policy: GitHub + CI/CD + SAST + DAST + Playwright green before stage advance; AGY-SCORE-001 scorecard on every handoff |
| **market** | HELD until Nova/owner open Market after install path + stability |

---

## 1. Purpose

This document is the **shared enhancement specification** for Agency Desks. It turns the macOS Waraq Settings experience (and full v1 feature set implied by those panes) into **routable work packages** with:

- Feature IDs and priorities (P0 / P1 / P2 / DELTA / OOS)
- Target desk ownership
- Acceptance criteria
- Evidence expected on handoff
- Explicit out-of-scope rules (license, privacy, platform honesty)

**Nova** should break this into Routing Orders (one desk at a time). Specialists must not self-start other desks.

---

## 2. Design principles (non-negotiable)

1. **GPL-3.0** — stays free/open; preserve `LICENSE` + `NOTICE`; no proprietary fork.
2. **Zero telemetry** — no analytics, no phone-home. Network only when user opts into Gallery search/download.
3. **Browse Web ≠ scraper** — open default browser only; user downloads MP4 and imports to Library.
4. **API keys local** — Pixabay/Pexels in user settings only; never leave device except as API query params to that host.
5. **Honest DELTAs** — Windows has no macOS menu bar / Gatekeeper model; tray + WinUI settings are the analogue. Document, don’t fake.
6. **Hard gates** — do not advance Secure/QA/Ship while CI/SAST/DAST/Playwright red or blocking issues open.
7. **Score loop** — every completion: §8 scorecard + inbox JSON → Nova score gate → dashboard → CONTINUE or REWORK.

---

## 3. Screenshot-derived surface map

| Pane (macOS) | User job | Enhancement clusters below |
|--------------|----------|----------------------------|
| **Displays** | Per-monitor LIVE/OFF, configure, hotplug policy | §5 C — Displays & profiles · §5 F — Configure sheet |
| **Library** | Browse/import local + built-in procedurals | §5 D — Library · §5 B — Engines |
| **Gallery → Online Sources** | API stock (Pixabay / Pexels / NASA) | §5 E — Gallery API |
| **Gallery → Browse Web** | Curated external sites → browser → drag into Library | §5 E — Browse Web |
| **Sidebar chrome** | General, Performance, Wallpapers, Diagnostics, About, Advanced, Search | §5 A — Shell · §5 G — Performance · §5 H — Diagnostics · §5 I — General/About/Onboarding |

---

## 4. Priority legend

| Code | Meaning |
|------|---------|
| **P0** | Required for “recognizable Waraq” / ship-quality core |
| **P1** | Full v1 parity after P0 |
| **P2** | Stretch / polish |
| **DELTA** | Platform difference — document behavior, do not pretend macOS identity |
| **OOS** | Out of scope (same as upstream mac) |

---

## 5. Feature catalog

### A. Shell & chrome

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| A1 | System tray icon + flyout (pause / open settings / quit) | P0 | Atlas Forge | Stagecraft QA | Tray visible; actions work; DELTA vs menu bar documented |
| A2 | Settings window: left nav + detail (dark theme) | P0 | Atlas Forge | Stagecraft QA | NavigationView (or equiv); panes switch without crash |
| A3 | Sidebar: General, Displays, Library, Gallery, Performance, Wallpapers, About | P0 | Atlas Forge | — | All entries present; stubs OK only if labeled and tracked |
| A4 | **Advanced** toggle (“Full depth controls”) | P1 | Atlas Forge | — | Hides/shows advanced-only controls + Diagnostics depth |
| A5 | Diagnostics pane (ADV) | P1 | Atlas Forge | Harbor Ops | Shows resource/governor state; no secrets leaked |
| A6 | Settings search / filter | P1 | Atlas Forge | Stagecraft QA | Finds panes/settings; keyboard usable |
| A7 | App icon + tray multi-size assets | P0 | Atlas Forge | Pipeline Warden | Packaged in installer/MSIX/portable |
| A8 | Onboarding wizard (first run + “Run setup again”) | P0 | Atlas Forge | Stagecraft QA | Displays pick, starter wallpaper, performance prefs |
| A9 | Design tokens (type, space, dark/light) | P0 | Atlas Forge | — | Shared ResourceDictionary / DESIGN.md applied |
| A10 | Reduce Motion / accessibility basics | P1 | Atlas Forge | Stagecraft QA | Honors OS reduce-motion where animations exist |

**Evidence on handoff:** screenshots per pane, PR link, Playwright smoke on shell navigation.

---

### B. Playback engines

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| B1 | Video wallpaper (MP4; document MOV/M4V support) | P0 | Atlas Forge | Cipher Shield, Stagecraft QA | Apply to desktop surface; loop; stop/teardown clean |
| B2 | GIF wallpaper | P0 | Atlas Forge | Stagecraft QA | Correct timing; no leak on stop |
| B3 | Static image engine | P1 | Atlas Forge | — | PNG/JPEG/WebP as wallpaper |
| B4–B9 | Procedural built-ins: Animated Gradient, Aurora, Matrix Rain, Synthwave Drive, Starfield, Neural Network | P0 | Atlas Forge | Stagecraft QA | All six render; “Zero MB” / lightweight; restore defaults |
| B10 | Fit modes: Fill / Fit / Stretch | P0 | Atlas Forge | Stagecraft QA | Per-display setting persists |
| B11 | Mute / volume / loop per display | P0 | Atlas Forge | Stagecraft QA | Audio respects mute; loop default on for walls |
| B12 | Wallpaper Engine **video subset** import only | P1 | Atlas Forge | Cipher Shield | No `.we` scene runtime (OOS) |
| B13 | Crossfade 0–3s on change | P1 | Atlas Forge | — | Optional; respects Reduce Motion |
| B14 | YouTube download | OOS | — | — | Do not implement |
| B15 | Wallpaper Engine scene runtime | OOS | — | — | Do not implement |

**Evidence:** sample media paths, short screen capture or Playwright where possible, unit tests for fit/loop helpers.

---

### C. Displays & profiles (from Displays screenshot)

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| C1 | List connected displays with name + resolution | P0 | Atlas Forge | Stagecraft QA | Matches OS topology after hotplug |
| C2 | MAIN / primary badge | P0 | Atlas Forge | — | Shows OS primary; optional Waraq Primary override (C7) |
| C3 | Per-display LIVE / OFF indicator + toggle | P0 | Atlas Forge | Stagecraft QA | OFF stops engine for that display only |
| C4 | **Configure** per display | P0 | Atlas Forge | — | Opens per-display wallpaper + fit + audio settings |
| C5 | Show Numbers (overlay 1…N on monitors) | P1 | Atlas Forge | Stagecraft QA | Topmost labels; dismiss clean |
| C6 | Profiles keyed by **stable hardware ID** | P0 | Atlas Forge | Cipher Shield | Replug known monitor restores last wallpaper/settings |
| C7 | Waraq Primary override | P1 | Atlas Forge | — | Independent of OS primary when set |
| C8 | When known display connects → **Restore profile** | P0 | Atlas Forge | Stagecraft QA | Default policy matches mac screenshot |
| C9 | When new display connects → **Ask me** (configurable) | P0 | Atlas Forge | Stagecraft QA | Prompt or policy; no silent wipe of other displays |
| C10 | Hotplug / WM_DISPLAYCHANGE handling | P0 | Atlas Forge | Stagecraft QA | No crash; engines rebind |
| C11 | Per-monitor DPI (PerMonitorV2) | P0 | Atlas Forge | Pipeline Warden | Manifest + correct layout on mixed DPI |

**Evidence:** multi-monitor test notes (or single-monitor + simulated topology unit tests), profile JSON sample, hotplug checklist.

---

### D. Library (from Library screenshot)

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| D1 | Grid of wallpapers with thumbnails | P0 | Atlas Forge | Stagecraft QA | Cards show type badge (VIDEO / BUILT-IN / GIF) |
| D2 | Total count + disk usage (e.g. “7 wallpapers · 479.6 MB”) | P0 | Atlas Forge | — | Accurate for user media; procedurals show Zero MB |
| D3 | Filters: All / Video / GIF / Built-in | P0 | Atlas Forge | Stagecraft QA | Filter correctness |
| D4 | Sort control | P1 | Atlas Forge | — | Name / date / size |
| D5 | **Import** files / folder / (optional GIF URL) | P0 | Atlas Forge | Cipher Shield | Drag-drop + file picker; copy into app data library |
| D6 | Large video card shows size (e.g. 479.6 MB) | P0 | Atlas Forge | — | Size visible before apply |
| D7 | Built-in labels: “Procedural · Zero MB” / lightweight | P0 | Atlas Forge | — | Cannot “delete” core built-ins without restore path |
| D8 | **Restore built-in wallpapers** | P0 | Atlas Forge | Stagecraft QA | Recreates missing built-ins |
| D9 | Context menu: set custom thumbnail / reset | P1 | Atlas Forge | — | Matches mac library UX intent |
| D10 | Apply wallpaper from Library to selected display(s) | P0 | Atlas Forge | Stagecraft QA | Integrates with C3/C4 |
| D11 | Delete / remove user imports only | P0 | Atlas Forge | — | Confirm dialog; does not delete OS files outside library unless user chose link mode (if ever offered: default = copy) |

**Evidence:** library folder layout doc, import test fixtures, Playwright on filter/import if UI automated.

---

### E. Gallery (from Gallery screenshots)

#### E1. Online Sources (Pixabay / Pexels / NASA)

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| E1 | Tabs: Online Sources \| Browse Web | P0 | Atlas Forge | — | Mutual exclusive modes |
| E2 | Provider chips: Pixabay, Pexels, NASA | P0 | Atlas Forge | Cipher Shield | NASA works without key if API allows |
| E3 | Empty state before key: connect copy + “Get API key” link | P0 | Atlas Forge | Beacon Growth (copy) | Opens provider docs in browser |
| E4 | Paste API key + **Save Key** | P0 | Atlas Forge | Cipher Shield | Stored locally (DPAPI/Credential Locker/User settings); never logged |
| E5 | Search + results grid | P0 | Atlas Forge | Stagecraft QA | Only after key (except NASA) |
| E6 | **Add to Library** download | P0 | Atlas Forge | Cipher Shield, Pipeline Warden | HTTPS download to library; progress; cancel |
| E7 | 24h search cache local | P1 | Atlas Forge | — | Cache eviction; no PII |
| E8 | Zero network if Gallery never used | P0 | Cipher Shield | Atlas Forge | Document + optional network test |
| E9 | No telemetry on search | P0 | Cipher Shield | — | Code review + SAST |

#### E2. Browse Web (MotionBGs, MoeWalls, MyLiveWallpapers, Wallsflow)

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| E10 | Curated site cards with tags + short blurb | P0 | Atlas Forge | Beacon Growth | Content from config JSON (editable) |
| E11 | **Open in Browser** only | P0 | Atlas Forge | Cipher Shield | `Process.Start` / shell open; no HTTP fetch of wallpapers through app |
| E12 | Instructional copy: download MP4 → drag to Library | P0 | Atlas Forge | Beacon Growth | Visible on Browse Web tab |
| E13 | Do not scrape/mirror/proxy third-party sites | P0 | Cipher Shield | — | Explicit test + code review sign-off |

**Evidence:** key storage design note, network egress list, Playwright “open browser” mock, Secure review of E8–E9/E13.

---

### F. Per-display Configure sheet (implied by Displays → Configure)

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| F1 | Pick wallpaper from Library | P0 | Atlas Forge | Stagecraft QA | Binds to that display profile only |
| F2 | Fit mode | P0 | Atlas Forge | — | Fill/Fit/Stretch |
| F3 | Mute / volume / loop | P0 | Atlas Forge | — | Persisted per display |
| F4 | Pause-on-battery override per display | P1 | Atlas Forge | — | Integrates with Performance governor |
| F5 | Preview before apply (optional) | P2 | Atlas Forge | — | Nice-to-have |

---

### G. Performance governor (Performance pane — in nav, not in screenshots)

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| G1 | Pause behind fullscreen apps / games | P0 | Atlas Forge | Stagecraft QA | Detect fullscreen; stop render; resume |
| G2 | Pause on battery (default on, configurable) | P0 | Atlas Forge | Stagecraft QA | AC/DC transitions |
| G3 | Pause on thermal pressure | P1 | Atlas Forge | — | Best-effort on Windows sensors |
| G4 | Hardware decode when available | P0 | Atlas Forge | — | MF/DXVA path documented |
| G5 | Render quality: auto/low/med/high | P1 | Atlas Forge | — | Affects procedural + video scale |
| G6 | Frame rate cap per display | P1 | Atlas Forge | — | e.g. 15/24/30/60 |
| G7 | Drop frames under load | P1 | Atlas Forge | — | Prefer jank over system freeze |
| G8 | Memory limit per wallpaper (default ~250 MB) | P1 | Atlas Forge | Cipher Shield | Enforce/reclaim; user-visible setting |
| G9 | Global pause from tray | P0 | Atlas Forge | Stagecraft QA | Overrides governor until cleared |

**Evidence:** manual test matrix (battery/fullscreen), perf notes, no freeze on Apply (regression vs known P0 defects).

---

### H. Diagnostics & Wallpapers panes

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| H1 | Diagnostics: CPU/GPU/RAM sample, engine state, display bind info | P1 | Atlas Forge | Harbor Ops | Read-only; copy support bundle redacts keys |
| H2 | Wallpapers pane (management / storage location reveal) | P1 | Atlas Forge | Harbor Ops | Open library folder in Explorer |
| H3 | Safe mode / disable all LIVE | P0 | Atlas Forge | Harbor Ops | One-click stop all displays |

---

### I. General & About

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| I1 | Launch at login | P0 | Atlas Forge | Pipeline Warden | Startup approved folder / task; user toggle |
| I2 | Language / theme (dark default) | P1 | Atlas Forge | — | At least dark stable |
| I3 | About: version, GPL notice, links to license + upstream | P0 | Atlas Forge | — | Required for GPL compliance UX |
| I4 | Check for updates (GitHub releases) | P1 | Pipeline Warden | Atlas Forge | Optional; user-initiated only (privacy) |
| I5 | Uninstall cleanliness | P0 | Pipeline Warden | Stagecraft QA | No broken Start-menu shortcuts (HF regression) |

---

### J. Installer, ship, security, QA (platform)

| ID | Feature | Pri | Primary desk | Supporting | Acceptance criteria (summary) |
|----|---------|-----|--------------|------------|-------------------------------|
| J1 | Installer (self-signed + prereq bundle or official runtime pull) | P0 | Pipeline Warden | Cipher Shield, Stagecraft QA | Document SmartScreen; .NET Desktop Runtime handled |
| J2 | Portable zip path remains | P1 | Pipeline Warden | — | Still shipable for power users |
| J3 | CI: build/test + CodeQL + DAST posture + Playwright | P0 | Pipeline Warden | Cipher, Stagecraft | All green on protected branch |
| J4 | Threat model: WorkerW/host, media paths, API keys | P0 | Cipher Shield | Atlas | Written review; no Crit/High open |
| J5 | Playwright: shell nav, import smoke, apply/stop smoke | P0 | Stagecraft QA | Atlas | CI-running suite; quarantine flake |
| J6 | Defect bar: Apply must not blank/freeze all screens | P0 | Atlas Forge | Stagecraft QA | Blocks ship if regressed |
| J7 | Start menu / shortcut must not uninstall or mis-target | P0 | Pipeline Warden | Stagecraft QA | Blocks ship if regressed |

---

### K. Explicit OOS / never

| ID | Item | Reason |
|----|------|--------|
| K1 | YouTube download / embed bypass | ToS + upstream OOS |
| K2 | Wallpaper Engine `.we` scene runtime | Proprietary format |
| K3 | Scraping MotionBGs/MoeWalls/etc. | Legal/takedown risk |
| K4 | Telemetry / analytics SDKs | Privacy contract |
| K5 | Closed-source relicensing | GPL-3.0 |
| K6 | iOS/Android/Linux targets in this pack | Windows focus |

---

## 6. Suggested delivery waves (for Nova sequencing)

> One desk at a time. Score ≥ 85 + hard gates green to advance. Market remains HELD until Wave D stability + installer sign-off unless owner Mode-A exception.

| Wave | Goal | Lead desk | Depends on |
|------|------|-----------|------------|
| **W0** | Stabilize P0 defects (Apply freeze, shortcut) | Atlas + Pipeline | Open HF approvals |
| **W1** | Shell + tray + Displays list/toggle + safe stop-all | Atlas Forge | W0 |
| **W2** | Engines video/GIF + fit/audio + Library grid/import/built-ins | Atlas Forge | W1 |
| **W3** | Profiles/hardware ID + hotplug policies + Configure sheet | Atlas Forge | W2 |
| **W4** | Performance governor (battery/fullscreen/pause) | Atlas Forge | W2 |
| **W5** | Gallery Online Sources + Browse Web (privacy-correct) | Atlas Forge → Cipher | W2 |
| **W6** | Diagnostics/About/General login item + polish search/advanced | Atlas Forge | W1 |
| **W7** | Secure full pass (keys, egress, media paths) | Cipher Shield | W5 |
| **W8** | Playwright expansion + cert | Stagecraft QA | W3–W6 |
| **W9** | Installer/signing/prereqs + release | Pipeline Warden | W7–W8 green |
| **W10** | Market kit (only when owner lifts HOLD) | Beacon Growth | W9 + owner APPROVE |

---

## 7. Desk responsibility matrix

| Desk | Owns | Does not own |
|------|------|----------------|
| **Gatekeeper Nova** | Sequencing, score gate, dashboard, CONTINUE/REWORK, Market hold | Implementing features, CI babysitting |
| **Atlas Forge** | UI, engines, library, gallery client, governor, profiles | Release signing, deep DAST program |
| **Cipher Shield** | SAST/DAST posture, key storage, egress audit, threat model | Feature scope expansion |
| **Stagecraft QA** | Playwright + exploratory cert of panes/flows | Shipping unsigned “trust us” |
| **Pipeline Warden** | CI gates, installer, artifacts, updates hook | Product UX design |
| **Beacon Growth** | Copy for empty states / Browse Web blurb / launch kit when unblocked | Live campaigns while HOLD |
| **Harbor Ops** | Support runbooks, safe-mode instructions | Code features |
| **Caliber Voss** | Process audit of desk loop adherence | Applying code |
| **Closer Quinn / Signal Scout** | N/A unless Nova routes | — |

---

## 8. Handoff requirements (every feature slice)

1. Handoff Packet sections 1–7 + **§8 Quality Scorecard** (AGY-SCORE-001).
2. Inbox JSON: `%LOCALAPPDATA%/hermes/scorecards/inbox/<work_id>_<desk>.json`.
3. Deliver to **desk channel and `#gatekeeper-nova`**.
4. Evidence table: PR, Actions URLs, screenshots matching mac panes where claimed, test commands.
5. Nova runs `agency_nova_score_gate.py` → live dashboard update → CONTINUE or REWORK.

---

## 9. Traceability to screenshots

| Screenshot | Primary feature IDs |
|------------|---------------------|
| Displays (LIVE/OFF, MAIN, Restore profile / Ask me) | C1–C11, F1–F4, A2–A3 |
| Library (7 items, filters, built-ins, import, restore) | D1–D11, B4–B9 |
| Gallery Pixabay API key empty state | E1–E9 |
| Gallery Browse Web cards | E10–E13 |

---

## 10. Open questions for owner (Nova may ask once)

1. Confirm **W0 hotfix** before new enhancement waves if Apply/shortcut P0s still open.
2. Gallery API keys: ship with **user-provided keys only** (recommended) vs any bundled demo key (not recommended).
3. Installer priority vs portable-only for next public tag.
4. When to lift **Market HOLD** (after W9 only vs earlier soft Mode C drafts).

---

## 11. Document control

| Version | Date | Author | Notes |
|---------|------|--------|-------|
| 1.0 | 2026-08-08 | Hermes / Agency intake | Initial pack from macOS Settings screenshots + parity matrix alignment |

**Paths**

- Agency vault (source of truth for desks):  
  `C:\Users\justb\Documents\Agency Desks Vault\01-Pipeline\WRQ-ENH-001-feature-enhancement-pack.md`
- Repo copy (engineering):  
  `C:\Users\justb\waraq-windows\docs\parity\WRQ-ENH-001-feature-enhancement-pack.md`

**Nova next (suggested)**  
Publish Approval Packet for **W0/W1** (or whole pack phased), then Routing Order to **Atlas Forge** or **Pipeline Warden** per open P0 defects — not both overlapping without RO.
