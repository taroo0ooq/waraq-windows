# WRQ-WIN-002 — Phase 1 scaffold recipe

| Field | Value |
|-------|--------|
| **work_id** | WRQ-WIN-002 |
| **for phase** | 1 — Solution scaffold + CI (empty shell) |
| **depends on** | Phase 0 docs (matrix, DESIGN.md, ADR 0003) |
| **desk** | Atlas Forge → Pipeline Warden for CI polish if needed |

This is a **recipe**, not implemented code. Phase 1 RO executes these steps.

---

## 1) Goals

1. Empty **WinUI 3** app runs (`dotnet build` / F5).  
2. Solution layout matches module boundaries (Shell / Host / Core / Engines).  
3. CI builds `src/` on `windows-latest`; CodeQL; DAST N/A; smoke stub.  
4. No claim of Settings parity yet (Phase 2).  
5. `archive/wrq-win-001/` untouched except read-only reference.

---

## 2) Proposed repo layout

```
src/
  Waraq.Windows.sln
  Directory.Build.props          # net8 windows TFM, nullable, version
  Waraq.Windows.App/             # WinUI 3 head (tray + settings host window)
    App.xaml
    MainWindow.xaml              # temporary; becomes SettingsShell in Phase 2
    Assets/
    Package.appxmanifest         # if packaged; or unpackaged profile
  Waraq.Windows.Shell/           # ViewModels, nav, design resource dicts
    Themes/Waraq.xaml            # from DESIGN.md
  Waraq.Windows.Core/            # profiles, library models, path gate, governor pure
  Waraq.Windows.Host/            # WorkerW native interop (port from archive)
  Waraq.Windows.Engines/         # interfaces only in Phase 1; impl Phase 3–5
  Waraq.Windows.Tests/           # xUnit

docs/
  parity/WRQ-WIN-002-matrix.md   # Phase 0
  design/windows/DESIGN.md       # Phase 0
  adr/0003-wrq-win-002-stack.md  # Phase 0
  parity/phase1-scaffold.md      # this file

archive/wrq-win-001/             # frozen MVP reference
App/ Core/ Engines/ …            # macOS upstream tree (reference)
```

**Do not** put new product code under `archive/`.

---

## 3) Project references

```
App → Shell, Core, Host, Engines
Shell → Core
Host → Core
Engines → Core
Tests → Core, Host (logic), Engines
```

Host must not reference Shell (keeps future OOP host clean).

---

## 4) Bootstrap commands (Phase 1 executor)

### 4.1 Prerequisites

- .NET 8 SDK  
- Visual Studio 2022 workload **Windows application development** **or**  
  Windows App SDK + WinUI project templates via `dotnet new install`  
- Windows 10 1903+ / Windows 11  

### 4.2 Create solution (illustrative)

```powershell
cd src
dotnet new sln -n Waraq.Windows
# Prefer VS "Blank App, Packaged with Windows Application Packaging Project (WinUI 3)"
# or unpackaged WinUI template when available to the toolchain.
# Then:
dotnet new classlib -n Waraq.Windows.Core -f net8.0-windows
dotnet new classlib -n Waraq.Windows.Host -f net8.0-windows
dotnet new classlib -n Waraq.Windows.Engines -f net8.0-windows
dotnet new classlib -n Waraq.Windows.Shell -f net8.0-windows
dotnet new xunit -n Waraq.Windows.Tests -f net8.0-windows
# Add WinUI app project via VS or template; wire ProjectReferences
dotnet sln add **/*.csproj
```

Exact template short-name depends on installed workload — record the working command in Phase 1 handoff.

### 4.3 Minimal App behavior (Phase 1)

- Single window: placeholder “Waraq for Windows — Phase 1 scaffold”  
- Optional NotifyIcon stub (can be Phase 2 if tray APIs slow)  
- `AppInfo` version `0.1.0-phase1`  
- No WorkerW attach required for Phase 1 green (Host project compiles with types only)

### 4.4 Port early (optional but recommended)

- `MediaPathClassifier` + path gate tests from archive → Core  
- `DesktopWallpaperHost` types compile under Host (attach behind feature flag off)

---

## 5) CI workflow recipe

Add/adjust `.github/workflows/windows-ci.yml`:

```yaml
# paths
- "src/**"
- ".github/workflows/windows-ci.yml"

# concurrency
cancel-in-progress: false   # NEVER true on short required gates

# job
runs-on: windows-latest
steps:
  - uses: actions/checkout@v4
  - uses: actions/setup-dotnet@v4
    with: { dotnet-version: "8.0.x" }
  # Install Windows App SDK build tools if required by template
  - run: dotnet restore src/Waraq.Windows.sln
  - run: dotnet build src/Waraq.Windows.sln -c Release
  - run: dotnet test src/Waraq.Windows.sln -c Release --no-build
```

- **CodeQL:** paths include `src/**`  
- **DAST:** keep N/A doc gate; no cancel concurrency  
- **Archive CI:** may remain pointed at `archive/wrq-win-001/**` or retire after Phase 1 green  

---

## 6) Directory.Build.props sketch

```xml
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <Company>Waraq Windows contributors</Company>
    <Authors>Waraq Windows contributors</Authors>
    <Copyright>Copyright (C) Waraq authors and Waraq Windows contributors</Copyright>
    <Version>0.1.0-phase1</Version>
  </PropertyGroup>
</Project>
```

GPL header comment on new `.cs` files (copy style from archive).

---

## 7) README updates (Phase 1)

- Banner: WRQ-WIN-002 parity rebuild; MVP archived  
- Build section points to `src/`  
- Link matrix + DESIGN + ADR 0003  

---

## 8) Phase 1 success criteria

| Criterion | Done when |
|-----------|-----------|
| Solution builds on windows-latest | Actions URL green |
| At least one unit test project runs | `dotnet test` green |
| WinUI app launches locally | Documented `dotnet run` or VS F5 |
| DESIGN resource dict stub present | `Themes/Waraq.xaml` compiles |
| No product claim of Settings parity | README honest |
| LICENSE/NOTICE intact | git diff clean on those |
| Scored handoff to Nova | §8 + inbox JSON |

---

## 9) Explicit non-goals (Phase 1)

- Full NavigationView Settings panes  
- WorkerW live attach  
- Gallery network  
- Installer  
- Marketing  

---

## 10) Handoff chain after Phase 1

Atlas Phase 1 green → Nova → Pipeline (CI harden if needed) → Atlas Phase 2 Design shell (owner visual accept).
