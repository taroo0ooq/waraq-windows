# WRQ-WIN-002 Phase 4 — Library notes

## Store layout (`%AppData%\Waraq`)

| Path | Purpose |
|------|---------|
| `library.json` | Catalog of imported wallpapers |
| `profiles.json` | Per-display wallpaper selection |
| `Wallpapers/` | Copied media files |
| `Thumbnails/` | JPEG thumbs (GIF/image stills; video placeholder) |

## Profiles
Keys come from `EnumDisplayDevices` **DeviceID** (PNP), not volatile GDI indices alone (`DisplayEnumerator`).

## UI
Library pane: Import · Apply selected · Remove · Reload. Apply updates profiles for all active displays.

## Out of scope (later)
Gallery network, procedural pack, auto-restore on launch (can wire Phase 7), drag-drop polish.
