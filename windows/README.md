# Waraq for Windows (`windows/`)

GPL-3.0 Windows live-wallpaper app for [taroo0ooq/waraq-windows](https://github.com/taroo0ooq/waraq-windows).

**Stack:** C# / .NET 8 / WPF — [ADR 0001](../docs/adr/0001-windows-tech-stack-and-wallpaper-host.md).  
**Host:** WorkerW (Progman) behind desktop icons.  
**MVP engines:** local **video** (MediaElement / Media Foundation) and **GIF** (GifBitmapDecoder).

## Prerequisites

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

## Layout

```
windows/
  WaraqWindows.sln
  Waraq.Windows/
    Host/       # WorkerW attach + WallpaperController
    Engines/    # video + GIF views, media classifier
    Shell/      # settings UI
  Waraq.Windows.Tests/
```

Upstream macOS sources remain at the **repository root**. Do not delete them.

## License

GNU GPL v3 — see `../LICENSE` and `../NOTICE`.
