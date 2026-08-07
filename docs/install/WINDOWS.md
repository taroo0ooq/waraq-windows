# Install Waraq for Windows (WRQ-WIN-002)

| Field | Value |
|-------|--------|
| **Product** | Waraq for Windows (WinUI 3 parity) |
| **work_id** | WRQ-WIN-002 |
| **phase** | 9 — Installer + self-signed Authenticode |
| **License** | GNU GPL v3 (`LICENSE`, `NOTICE`) |
| **Platform** | Windows 10 / 11 (x64) |
| **App path** | `src/Waraq.Windows.App` |
| **ADR** | [0002 — installer & signing](../adr/0002-windows-installer-and-signing.md) |

## Recommended: Windows installer

1. Open **[Releases](https://github.com/taroo0ooq/waraq-windows/releases)** or Actions **`windows-release`** artifacts.
2. Download:
   - **`Waraq.Windows-Setup-win-x64-<version>.exe`** (primary)
   - `SHA256SUMS-installer.txt`
   - **`WaraqWindows-CodeSigning.cer`** (public cert; optional trust)
3. Verify checksum:

```powershell
Get-FileHash .\Waraq.Windows-Setup-win-x64-*.exe -Algorithm SHA256
```

4. Run Setup (per-user, **no admin** by default).  
   Default: `%LOCALAPPDATA%\Programs\WaraqWindows\`
5. Start Menu → **Waraq for Windows** (`Waraq.Windows.App.exe`).

### Prerequisites

| Component | Strategy |
|-----------|----------|
| **.NET 8** | **Bundled** — `dotnet publish --self-contained` |
| **Windows App SDK** | **Bundled** — `WindowsAppSDKSelfContained=true` |
| **Vendor downloads** | Not used on default path. Future FD bootstrap would use **only** official Microsoft / aka.ms URLs (no third-party mirrors). |

### Code signing (self-signed) & SmartScreen

- Authenticode uses a **project self-signed** cert (or repo secrets `CODE_SIGNING_PFX_BASE64` + `CODE_SIGNING_PFX_PASSWORD`).
- **Private keys never committed.**
- SmartScreen may still warn (no EV reputation).
- `Get-AuthenticodeSignature` may show `NotTrusted` until CER is imported — signature can still be present.

```powershell
Get-AuthenticodeSignature .\Waraq.Windows-Setup-win-x64-*.exe | Format-List *
# Optional:
Import-Certificate -FilePath .\WaraqWindows-CodeSigning.cer -CertStoreLocation Cert:\CurrentUser\TrustedPublisher
```

## Secondary: Portable zip

Extract `Waraq.Windows-win-x64-*.zip` and run **`Waraq.Windows.App.exe`**.

## First-run use

1. Complete onboarding if shown.
2. Import or browse local **video** / **GIF**, or use Gallery / procedural engines as available.
3. **Apply wallpaper** / Stop as needed.

Privacy: Gallery network only when you search/download; Browse Web opens the system browser only.

## Build from source

Prerequisites: Windows 10/11, [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), [Inno Setup 6](https://jrsoftware.org/isinfo.php), Windows SDK `signtool`.

```powershell
cd src
dotnet restore Waraq.Windows.sln
dotnet test Waraq.Windows.Tests/Waraq.Windows.Tests.csproj -c Release
dotnet build Waraq.Windows.App/Waraq.Windows.App.csproj -c Release -p:Platform=x64
```

### Local installer

```powershell
# from repo root
pwsh -File src/scripts/Build-Installer.ps1 -GenerateCiCert
# outputs: artifacts/installer/, artifacts/certs/
```

CI: **Actions → windows-release → Run workflow** or tag `win-v*`.

## CI quality gates

| Workflow | Role |
|----------|------|
| `windows-ci` | build + test `src/` |
| `codeql` | SAST csharp |
| `dast` | DAST gate / egress docs |
| `playwright` | Windows QA smoke |
| `build` | macOS upstream |
| `windows-release` | portable zip + **signed Setup** |

## Known limits

- Self-signed only (SmartScreen expected)
- No auto-updater / Store
- Codec / host edge cases documented in QA matrices

## Support / source

- https://github.com/taroo0ooq/waraq-windows  
- Upstream macOS: https://github.com/bahamut42/waraq  
- Archived MVP: `archive/wrq-win-001/`
