# Waraq for Windows (`windows/`)

GPL-3.0 Windows live-wallpaper app for [taroo0ooq/waraq-windows](https://github.com/taroo0ooq/waraq-windows).

**Stack:** C# / .NET 8 / WPF — [ADR 0001](../docs/adr/0001-windows-tech-stack-and-wallpaper-host.md).  
**Host:** WorkerW (Progman) behind desktop icons.  
**MVP engines:** local **video** (MediaElement / Media Foundation) and **GIF** (GifBitmapDecoder).

## Install (end users)

See **[docs/install/WINDOWS.md](../docs/install/WINDOWS.md)**.

1. Download the latest `Waraq.Windows-win-x64-*.zip` from [Releases](https://github.com/taroo0ooq/waraq-windows/releases) (or Actions `windows-release` artifacts).
2. Verify `SHA256SUMS.txt` (recommended).
3. Extract and run `Waraq.Windows.exe` (self-contained; **not** code-signed — SmartScreen may warn).

### Package locally / CI

```powershell
# local (repo root)
pwsh -File windows/scripts/package-release.ps1

# CI: Actions → windows-release → Run workflow
# or: git tag win-v0.2.0-alpha && git push origin win-v0.2.0-alpha
```

Workflow: `.github/workflows/windows-release.yml`

## Prerequisites (from source)

- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build

```powershell
cd windows
dotnet restore WaraqWindows.sln
dotnet build WaraqWindows.sln -c Release
```

## Test

```powershell
cd windows
dotnet test WaraqWindows.sln -c Release
```

## Run

```powershell
cd windows
dotnet run --project Waraq.Windows -c Release
```

1. **Browse…** — pick a local `.mp4` / `.webm` / `.gif` (etc.)
2. Choose **Fit** (Fill / Stretch / Fit)
3. **Apply wallpaper** — attaches a surface under WorkerW across the virtual desktop
4. **Stop** — tears down the surface (also runs on app exit)

**Probe host** only locates Progman/WorkerW without applying media.

### MVP limits / follow-ups

- Battery pause and fullscreen-app pause are **not** implemented yet (documented deferred).
- One surface spans the full virtual desktop (not polished per-monitor profiles).
- Codec support depends on system Media Foundation / installed codecs.
- Explorer restarts may require re-Apply.
- Portable zip only — no MSI/auto-updater/signing in Phase 5.

## Layout

```
windows/
  WaraqWindows.sln
  Waraq.Windows/
    Host/       # WorkerW attach + WallpaperController
    Engines/    # video + GIF views, media classifier
    Shell/      # settings UI
  Waraq.Windows.Tests/
  scripts/package-release.ps1
```

Upstream macOS sources remain at the **repository root**. Do not delete them.

## CI

| Workflow | Purpose |
|----------|---------|
| `windows-ci` | restore / build / test |
| `codeql` | SAST csharp |
| `dast` | DAST N/A gate until network surface |
| `playwright` | Windows QA smoke |
| `windows-release` | self-contained zip + optional draft release |
| `build` | macOS upstream CI |

## License

GNU GPL v3 — see `../LICENSE` and `../NOTICE`.
