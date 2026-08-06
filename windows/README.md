# Waraq for Windows (`windows/`)

GPL-3.0 Windows live-wallpaper scaffold for [taroo0ooq/waraq-windows](https://github.com/taroo0ooq/waraq-windows).

**Stack:** C# / .NET 8 / WPF — see [ADR 0001](../docs/adr/0001-windows-tech-stack-and-wallpaper-host.md).  
**Host strategy:** WorkerW (Progman) — probe only in Phase 1a; attach + playback in Phase 2.

## Prerequisites

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (`dotnet --list-sdks` should list `8.x`)

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

## Run (settings shell)

```powershell
cd windows
dotnet run --project Waraq.Windows -c Release
```

The window is a **scaffold shell**. Use **Probe desktop host** to locate Progman/WorkerW without parenting a wallpaper HWND yet.

## Layout

```
windows/
  WaraqWindows.sln
  Waraq.Windows/           # app
    Host/                  # WorkerW discovery (Phase 2: attach surfaces)
    Shell/                 # settings UI
  Waraq.Windows.Tests/     # xUnit
```

Upstream macOS sources remain at the **repository root** (`App/`, `Core/`, `Engines/`, …). Do not delete them.

## CI (Phase 1b)

Additive GitHub Actions (macOS `build.yml` unchanged):

| Workflow | Path | Purpose |
|----------|------|---------|
| `windows-ci` | `.github/workflows/windows-ci.yml` | `windows-latest` restore / build / test `WaraqWindows.sln` |
| `codeql` | `.github/workflows/codeql.yml` | SAST — CodeQL csharp |
| `dast` | `.github/workflows/dast.yml` | DAST gate — **N/A** until network surface ([docs/security/DAST.md](../docs/security/DAST.md)) |
| `playwright` | `.github/workflows/playwright.yml` | Playwright stub until `e2e` specs exist |

## License

GNU GPL v3 — see `../LICENSE` and `../NOTICE`.
