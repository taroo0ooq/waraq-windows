# Waraq for Windows — `src/` (WRQ-WIN-002)

GPL-3.0 parity rebuild. Phase 1: **empty WinUI 3 shell** + module layout.

| Doc | Path |
|-----|------|
| ADR 0003 | [docs/adr/0003-wrq-win-002-stack.md](../docs/adr/0003-wrq-win-002-stack.md) |
| Design | [docs/design/windows/DESIGN.md](../docs/design/windows/DESIGN.md) |
| Parity matrix | [docs/parity/WRQ-WIN-002-matrix.md](../docs/parity/WRQ-WIN-002-matrix.md) |
| Scaffold recipe | [docs/parity/phase1-scaffold.md](../docs/parity/phase1-scaffold.md) |

## Prerequisites

- Windows 10/11
- .NET 8 SDK
- Windows App SDK (restored via NuGet `Microsoft.WindowsAppSDK`)

## Build

```powershell
cd src
dotnet restore Waraq.Windows.sln
dotnet build Waraq.Windows.Tests/Waraq.Windows.Tests.csproj -c Release
dotnet build Waraq.Windows.App/Waraq.Windows.App.csproj -c Release -p:Platform=x64
```

## Test

```powershell
cd src
dotnet test Waraq.Windows.Tests/Waraq.Windows.Tests.csproj -c Release
```

## Run

```powershell
cd src
dotnet run --project Waraq.Windows.App -c Release -p:Platform=x64
```

Placeholder window lists planned Settings panes. **No** Settings L&F parity yet (Phase 2). **No** live wallpaper attach (Phase 3).

## Layout

```
src/
  Waraq.Windows.sln
  Waraq.Windows.App/       # WinUI 3 unpackaged head
  Waraq.Windows.Shell/     # ViewModels / nav catalog
  Waraq.Windows.Core/      # path gate, media classify, AppInfo
  Waraq.Windows.Host/      # WorkerW probe (compile-only attach)
  Waraq.Windows.Engines/   # contracts only
  Waraq.Windows.Tests/
```

Archive MVP: `archive/wrq-win-001/`. macOS sources remain at repo root.
