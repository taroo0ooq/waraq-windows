# WRQ-WIN-002 Phase 3 — Host notes

## Behavior
- **WorkerW (Progman)** attach via `DesktopWallpaperHost.AttachHwnd`
- One surface spans **virtual desktop** (all monitors MVP)
- **Video**: `MediaPlayer` + `MediaPlayerElement`, loop, muted
- **GIF**: `BitmapImage` on `Image`
- **Apply/Stop**: Wallpapers + Library panes; tray **Stop wallpaper** / Quit stops host
- **Path gate**: `LocalMediaPathGate` rejects UNC/URL/relative

## Codecs
Depends on system Media Foundation. Prefer MP4 (H.264/AAC). MOV may need codecs.

## Limitations
- No per-monitor independent wallpapers yet
- No pause-on-battery/fullscreen (later)
- Explorer restart may require re-Apply
- WinUI surface under WorkerW can be brittle across Win11 builds

## Run
```powershell
cd src
dotnet run --project Waraq.Windows.App -c Release -p:Platform=x64
```
Open **Wallpapers** → Apply wallpaper → pick local `.mp4`/`.gif` → Stop.
