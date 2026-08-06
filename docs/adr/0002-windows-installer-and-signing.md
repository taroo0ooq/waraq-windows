# ADR 0002 — Windows installer technology and code signing

| Field | Value |
|-------|--------|
| **Status** | Accepted |
| **Date** | 2026-08-06 |
| **work_id** | WRQ-WIN-001 Phase 6 |
| **Deciders** | Pipeline Warden (per Nova RO + owner) |

## Context

Phase 5 shipped a **portable self-contained zip** only. Owner required a real **Windows installer**, **self-signed Authenticode**, and **prerequisite handling** (bundle or official vendor download). Marketing remains held until installer path is ready.

## Decision

1. **Installer:** [Inno Setup 6](https://jrsoftware.org/isinfo.php)  
   - Script: `installer/waraq-windows.iss`  
   - Produces `Waraq.Windows-Setup-win-x64-<version>.exe`  
   - Per-user default dir (`%LOCALAPPDATA%\Programs\WaraqWindows`), Start Menu shortcut, optional desktop icon, uninstaller, LICENSE.

2. **App payload:** **Self-contained** `dotnet publish -r win-x64` (same as Phase 5).  
   - **Prereq strategy:** .NET 8 runtime is **bundled inside the app publish output** — no third-party mirror downloads.  
   - VC++ / OS components remain Windows-provided; no unofficial redistributables.

3. **Code signing:** **Self-signed** Authenticode (owner-directed; not EV/OV).  
   - CI generates an ephemeral project code-signing cert **or** uses optional GitHub secrets `CODE_SIGNING_PFX_BASE64` + `CODE_SIGNING_PFX_PASSWORD`.  
   - Signs **main EXE** + **Setup.exe** with `signtool` (SHA256).  
   - **Private key never committed.** Public `.cer` published as release/CI artifact for user trust import.  
   - Document SmartScreen expectations (self-signed ≠ Microsoft reputation).

## Alternatives considered

| Option | Why not now |
|--------|-------------|
| WiX MSI | Heavier authoring; Inno faster for portable app + Start Menu |
| Framework-dependent + aka.ms .NET bootstrap | Valid; deferred complexity — SC already satisfies “pack prereqs” |
| Purchased EV cert | Explicitly out of scope (owner: self-signed) |
| MSIX / Store | Out of scope |

## Consequences

- Users must optionally **trust the project CER** or click through SmartScreen.  
- Installer builds require Windows runner + Inno Setup in CI.  
- Portable zip remains secondary distribution path.

## Related

- `docs/install/WINDOWS.md`  
- `.github/workflows/windows-release.yml`  
- `windows/scripts/Build-Installer.ps1`
