# Waraq for Windows — `src/` (WRQ-WIN-002)

GPL-3.0 parity rebuild.

| Phase | Status |
|-------|--------|
| 1 Scaffold | Done — WinUI 3 modules |
| **2 Design shell** | **Tray + Settings NavigationView + stub panes** |

| Doc | Path |
|-----|------|
| ADR 0003 | [docs/adr/0003-wrq-win-002-stack.md](../docs/adr/0003-wrq-win-002-stack.md) |
| Design | [docs/design/windows/DESIGN.md](../docs/design/windows/DESIGN.md) |
| Tokens (XAML) | `Waraq.Windows.App/Themes/Waraq.xaml` |
| Windows UI shot | [docs/screenshots/windows/phase2-settings-shell.png](../docs/screenshots/windows/phase2-settings-shell.png) |

## Run

```powershell
cd src
dotnet build Waraq.Windows.App/Waraq.Windows.App.csproj -c Release -p:Platform=x64
dotnet run --project Waraq.Windows.App -c Release -p:Platform=x64 --no-build
```

- Settings window 720×560, left nav ~190dip, Advanced toggle (shows Diagnostics)
- Tray icon: Open settings / Pause stub / Quit
- Panes are **stubs** (not owner-accepted L&F final)

## Test

```powershell
cd src
dotnet test Waraq.Windows.Tests/Waraq.Windows.Tests.csproj -c Release
```
