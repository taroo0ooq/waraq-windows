# Waraq for Windows — `src/` (WRQ-WIN-002)

| Phase | Status |
|-------|--------|
| 1 Scaffold | Done |
| 2 Design shell | Done |
| **3 Host** | **WorkerW + video/GIF Apply/Stop** |

## Run
```powershell
cd src
dotnet build Waraq.Windows.App/Waraq.Windows.App.csproj -c Release -p:Platform=x64
dotnet run --project Waraq.Windows.App -c Release -p:Platform=x64 --no-build
```
Wallpapers/Library pane → **Apply wallpaper** / **Stop**. Tray: Stop wallpaper, Quit.

## Test
```powershell
cd src
dotnet test Waraq.Windows.Tests/Waraq.Windows.Tests.csproj -c Release
```

## Docs
- Phase 3 notes: [docs/parity/phase3-host-notes.md](../docs/parity/phase3-host-notes.md)
- Design: [docs/design/windows/DESIGN.md](../docs/design/windows/DESIGN.md)
