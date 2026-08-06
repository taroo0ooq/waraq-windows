# Install Waraq for Windows

| Field | Value |
|-------|--------|
| **Product** | Waraq for Windows |
| **work_id** | WRQ-WIN-001 |
| **phase** | 5 — Ship |
| **License** | GNU GPL v3 (`LICENSE`, `NOTICE`) |
| **Platform** | Windows 10 / 11 (x64) |

## Download (recommended)

1. Open **[Releases](https://github.com/taroo0ooq/waraq-windows/releases)** on this repository.
2. Prefer a **Windows** release / draft tagged `win-v*` (prerelease is expected while alpha).
3. Download:
   - `Waraq.Windows-win-x64-<version>.zip`
   - `SHA256SUMS.txt` (optional but recommended)
4. Verify checksum (PowerShell):

```powershell
Get-FileHash .\Waraq.Windows-win-x64-*.zip -Algorithm SHA256
# Compare to the line in SHA256SUMS.txt
```

5. Extract the zip to a folder you control (example: `%LOCALAPPDATA%\Programs\WaraqWindows\`).
6. Run **`Waraq.Windows.exe`**.

### First-run notes

- The build is **self-contained** — you do **not** need to install the .NET 8 runtime separately for the published zip.
- The binary is **not Authenticode-signed**. Windows SmartScreen / Defender may warn on first launch. That is expected for unsigned open-source builds. Prefer building from source if your policy forbids unsigned binaries.
- No installer/MSI yet — portable zip only (Phase 5).

### Use

1. **Browse…** and pick a local **video** (`.mp4`, `.webm`, …) or **GIF**.
2. Choose fit mode (Fill / Stretch / Fit).
3. Click **Apply wallpaper**.
4. **Stop** removes the live surface (also on exit).

Privacy: the Windows MVP plays **local files you choose**. Do not expect macOS Gallery parity yet.

## Build from source (developers)

Prerequisites: Windows 10/11, [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
git clone https://github.com/taroo0ooq/waraq-windows.git
cd waraq-windows\windows
dotnet restore WaraqWindows.sln
dotnet test WaraqWindows.sln -c Release
dotnet run --project Waraq.Windows -c Release
```

### Local package (matches CI)

```powershell
# from repo root
pwsh -File windows/scripts/package-release.ps1
# output under artifacts/
```

Or trigger GitHub Actions:

- **Actions → windows-release → Run workflow**
- Or push a tag: `git tag win-v0.2.0-alpha && git push origin win-v0.2.0-alpha`

## CI quality gates (still required)

Ship does **not** disable standing gates:

| Workflow | Role |
|----------|------|
| `windows-ci` | build + unit/integration tests |
| `codeql` | SAST (csharp) |
| `dast` | DAST gate (N/A until network surface — see `docs/security/DAST.md`) |
| `playwright` | Windows QA smoke + browser N/A job |
| `build` | upstream macOS CI |
| `windows-release` | package + optional draft release |

## Known MVP limits

- Not code-signed / no auto-updater
- Battery / fullscreen pause deferred
- Explorer restarts may need re-Apply
- Codec support depends on Media Foundation
- Gallery / multi-profile polish not shipped

## Support / source

- Source: https://github.com/taroo0ooq/waraq-windows  
- Upstream macOS project: https://github.com/bahamut42/waraq  
- Corresponding source for binaries is this repo under GPL-3.0.
