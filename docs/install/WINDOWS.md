# Install Waraq for Windows

| Field | Value |
|-------|--------|
| **Product** | Waraq for Windows |
| **work_id** | WRQ-WIN-001 |
| **phase** | 6 — Installer + self-signed Authenticode |
| **License** | GNU GPL v3 (`LICENSE`, `NOTICE`) |
| **Platform** | Windows 10 / 11 (x64) |
| **ADR** | [0002 — installer & signing](../adr/0002-windows-installer-and-signing.md) |

## Recommended: Windows installer

1. Open **[Releases](https://github.com/taroo0ooq/waraq-windows/releases)** (or the `windows-release` Actions run artifacts).
2. Download:
   - **`Waraq.Windows-Setup-win-x64-<version>.exe`** (primary)
   - `SHA256SUMS-installer.txt` (if present)
   - **`WaraqWindows-CodeSigning.cer`** (public cert for optional trust)
3. Verify checksum (PowerShell):

```powershell
Get-FileHash .\Waraq.Windows-Setup-win-x64-*.exe -Algorithm SHA256
```

4. Run the Setup EXE (per-user install, **no admin required** by default).  
   Default location: `%LOCALAPPDATA%\Programs\WaraqWindows\`
5. Launch **Waraq for Windows** from the Start Menu.

### Prerequisites (what the installer does)

| Component | Strategy |
|-----------|----------|
| **.NET 8 runtime** | **Bundled** — installer deploys a **self-contained** app publish (runtime inside the app folder). You do **not** need a separate .NET Desktop Runtime install for the default payload. |
| **Vendor downloads** | **Not used** for the default path (no third-party mirrors). If a future framework-dependent build is offered, only **official Microsoft** aka.ms / dotnet.microsoft.com URLs would be used (see ADR 0002). |

### Code signing (self-signed) & SmartScreen

Builds are **Authenticode-signed with a project self-signed certificate** (owner requirement for Phase 6). This is **not** an EV/OV certificate from a commercial CA.

**What to expect**

- Windows **SmartScreen** / Defender may still show “Windows protected your PC” or unknown publisher warnings.
- `Get-AuthenticodeSignature` may show `Status = NotTrusted` / `UnknownError` until the public CER is trusted — the signature can still be present and valid cryptographically.

**Optional: trust the project certificate (advanced)**

Only import a `.cer` you obtained from **this repo’s official GitHub Release or CI artifacts**.

```powershell
# Inspect first
Get-AuthenticodeSignature .\Waraq.Windows-Setup-win-x64-*.exe | Format-List *

# Optional — Trusted Publishers (still may not silence SmartScreen reputation)
Import-Certificate -FilePath .\WaraqWindows-CodeSigning.cer -CertStoreLocation Cert:\CurrentUser\TrustedPublisher
```

Do **not** import certificates from untrusted sources. Enterprise admins may prefer software restriction policies instead of trusting a self-signed root.

**CI secrets (maintainers)**

Optional repo secrets for a stable cert across runs:

- `CODE_SIGNING_PFX_BASE64` — base64-encoded PFX  
- `CODE_SIGNING_PFX_PASSWORD` — PFX password  

If unset, CI generates an **ephemeral** self-signed cert per release job and uploads the public `.cer` as an artifact. **Private keys are never committed.**

## Secondary: Portable zip

1. Download `Waraq.Windows-win-x64-<version>.zip` + checksum file.
2. Extract to a folder you control.
3. Run `Waraq.Windows.exe`.

Portable builds from the same release pipeline may also be signed when produced alongside the installer.

## First-run use

1. **Browse…** — local **video** (`.mp4`, `.webm`, …) or **GIF**.
2. Fit mode (Fill / Stretch / Fit).
3. **Apply wallpaper**.
4. **Stop** removes the live surface (also on exit).

Privacy: Windows MVP plays **local files you choose**. Gallery parity with macOS is not claimed.

## Build from source (developers)

Prerequisites: Windows 10/11, [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), [Inno Setup 6](https://jrsoftware.org/isinfo.php) (for installer), Windows SDK `signtool` (for signing).

```powershell
git clone https://github.com/taroo0ooq/waraq-windows.git
cd waraq-windows\windows
dotnet restore WaraqWindows.sln
dotnet test WaraqWindows.sln -c Release
dotnet run --project Waraq.Windows -c Release
```

### Local package / installer

```powershell
# portable zip only
pwsh -File windows/scripts/package-release.ps1

# installer + self-signed (ephemeral cert)
pwsh -File windows/scripts/Build-Installer.ps1 -GenerateCiCert

# installer with your PFX
pwsh -File windows/scripts/Build-Installer.ps1 -PfxPath C:\secure\waraq.pfx -PfxPassword '***'
```

Outputs under `artifacts/installer/` and `artifacts/certs/`.

Or **Actions → windows-release → Run workflow** / tag `win-v*`.

## CI quality gates

| Workflow | Role |
|----------|------|
| `windows-ci` | build + unit/integration tests |
| `codeql` | SAST (csharp) |
| `dast` | DAST gate (N/A until network surface — `docs/security/DAST.md`) |
| `playwright` | Windows QA smoke + browser N/A job |
| `build` | upstream macOS CI |
| `windows-release` | portable zip + **signed installer** + optional draft release |

## Known MVP limits

- Self-signed only (no EV reputation / no Microsoft Store)
- No auto-updater
- Battery / fullscreen pause deferred
- Explorer restarts may need re-Apply
- Codec support depends on Media Foundation
- Gallery / multi-profile polish not shipped

## Support / source

- Source: https://github.com/taroo0ooq/waraq-windows  
- Upstream macOS: https://github.com/bahamut42/waraq  
- Corresponding source for binaries: this repo (GPL-3.0)
