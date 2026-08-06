> **Windows port workspace (taroo0ooq/waraq-windows)**  
> This repository is a **GPL-3.0 derivative** of [bahamut42/waraq](https://github.com/bahamut42/waraq) (macOS live wallpapers).  
> Goal: develop a **Windows** version. Upstream macOS Swift/AppKit tree is kept at repo root for provenance and reference.  
> License: **GNU GPL v3** — stays free/open; see `LICENSE` and `NOTICE`.  
> Status: **Phase 2 MVP** — video + GIF live wallpaper via WorkerW under `windows/` ([ADR 0001](docs/adr/0001-windows-tech-stack-and-wallpaper-host.md)). Gallery/multi-profile polish not shipped.

---

<p align="center">
  <img src="docs/hero.svg" alt="Waraq - Live wallpapers for macOS" width="100%">
</p>

<p align="center">
  <a href="https://github.com/bahamut42/waraq/blob/main/LICENSE">
    <img src="https://img.shields.io/badge/License-GPLv3-blue.svg" alt="License: GPL v3">
  </a>
  <a href="https://github.com/bahamut42/waraq/releases">
    <img src="https://img.shields.io/github/v/release/bahamut42/waraq?label=latest&color=green" alt="Latest release">
  </a>
  <img src="https://img.shields.io/badge/macOS-14%2B-lightgrey.svg" alt="macOS 14+">
  <img src="https://img.shields.io/badge/Swift-5.10%2B-orange.svg" alt="Swift 5.10+">
  <img src="https://komarev.com/ghpvc/?username=bahamut42-waraq&label=Repo%20views&color=c83a4a&style=flat" alt="Repo views">
</p>

Waraq started as a proof of concept I built for myself. I wanted live wallpapers on my Mac - the kind Wallpaper Engine ships on Windows - and the existing Mac options were either paid, abandoned, or built on content from sites with sketchy licensing. So I spent some free time building a clean version. If you find it useful, great. If you want to pick it up and extend it, please do - fork it, ship your own version, contribute back. The only rule is the one GPL v3 already enforces: it stays free, nobody sells it, nobody closes the source. That's the entire condition.

<p align="center">
  <img src="docs/hero-desktop.gif" alt="Waraq running an animated Aurora wallpaper on the desktop" width="100%">
</p>

## What Waraq does

- Plays videos, GIFs, and procedural animations as your desktop wallpaper
- Runs independently on every connected display, with its own settings
- Remembers your config per monitor using hardware ID, so plugging a different monitor in doesn't blow away your setup
- Has a built-in Gallery with thousands of free wallpapers from Pixabay, Pexels, and NASA
- Lists external sites (motionbgs, moewalls, mylivewallpapers, wallsflow) where you can grab anime and gaming wallpapers manually
- Pauses when on battery, pauses behind fullscreen apps, pauses on thermal pressure
- Imports video files you already own, including the video subset of Wallpaper Engine downloads

## What Waraq doesn't do

- Doesn't host, mirror, or proxy any content from external sites. The Browse Web tab is just curated bookmarks - you download from those sites directly under their personal-use licenses, then drag the MP4 into Waraq.
- Doesn't support Wallpaper Engine's `.we` scene files. Only the video subset works. Scenes are a proprietary editor format with their own runtime.
- Doesn't download from YouTube. We tried. YouTube actively blocks embedding for most videos, and downloading violates their ToS.
- **Upstream macOS app** doesn't run on Windows, Linux, iPhone, or iPad (macOS 14+ only). This fork adds a separate Windows scaffold under `windows/` (see below) — not feature-complete yet.
- Doesn't cost money. Now or ever. See the license section.

## Privacy

Zero telemetry. No analytics. No phone-home. No tracking. No "anonymous usage data."

The only outbound network activity Waraq ever does is:

- API calls to **Pixabay**, **Pexels**, or **NASA** - only when **you** type a search and click Search in the Gallery tab
- HTTP downloads of the specific wallpaper file you choose to add to your library

That's it. Nothing else. Ever.

If you never use the Gallery, Waraq makes **zero outbound connections**. Open the app, set a wallpaper from your own files, close Settings - no network traffic at all.

The Browse Web tab opens links in your **default browser**. Waraq doesn't fetch anything from those sites. The browser does its thing, you download manually, drag the file in.

API keys for Pixabay and Pexels are yours, stored locally in your macOS UserDefaults, never sent anywhere except as query parameters to the relevant API host. NASA needs no key.

If you want zero network involvement: don't put API keys in. Use Waraq purely as a playback engine for files you bring yourself. Everything works the same.

**Documentation site analytics.** The standalone install guide at [bahamut42.github.io/waraq/install/](https://bahamut42.github.io/waraq/install/) uses [GoatCounter](https://www.goatcounter.com) (privacy-friendly, no cookies, no personal data, respects Do Not Track) for aggregate visit counts. See the [wiki Privacy page](https://github.com/bahamut42/waraq/wiki/Privacy) for details. This does not affect the Waraq app itself, which remains zero-telemetry.

## Install

<p align="center">
  <a href="https://bahamut42.github.io/waraq/install/">
    <img src="docs/install-guide-button.svg" alt="Visual Install Guide - step-by-step walkthrough with screenshots" width="700">
  </a>
</p>

Or follow the quick instructions below. Two ways to install Waraq on your Mac — you do **not** need Xcode or to build anything, just download and run.

### Option 1: DMG (drag and drop)

1. Download `Waraq-1.0.0.dmg` from the [latest Release](https://github.com/bahamut42/waraq/releases)
2. Double-click the downloaded file
3. Drag the Waraq icon onto the Applications folder
4. Eject the disk image
5. Open Applications and launch Waraq

### Option 2: PKG (guided installer)

1. Download `Waraq-1.0.0.pkg` from the [latest Release](https://github.com/bahamut42/waraq/releases)
2. Double-click to launch the installer
3. Follow the wizard (Continue → agree to GPL v3 → Install)
4. Waraq is installed to Applications

Both methods install to `/Applications/Waraq.app` — pick whichever you prefer. Both downloads are code-signed and notarized, so they launch with no Gatekeeper warnings. The first launch shows a quick setup wizard (displays, a starter wallpaper, performance preferences).

Found a bug? [Open an issue.](https://github.com/bahamut42/waraq/issues)

## Documentation

For in-depth documentation (per-feature guides, multi-monitor setup, performance tuning, troubleshooting, privacy details, contributing, and the project roadmap), see the [Waraq Wiki](https://github.com/bahamut42/waraq/wiki).

## Quick tour

### Menu bar

After install, Waraq lives in your menu bar in the top-right corner of your screen. Look for the small wallpaper-roll icon. Click it to open Settings.

### Displays

<p align="center">
  <img src="docs/screenshots/02-displays.png" alt="Waraq Settings - Displays pane" width="80%">
</p>

Every connected display gets its own row. Toggle each on or off. Configure to pick a wallpaper for each, set fit mode, mute, volume, loop.

Profiles save by hardware ID. Plug a different monitor in, Waraq detects it's new. Plug a familiar monitor back in, Waraq restores its last config automatically.

Right-click a display row for the context menu - including "Set as Waraq Primary" if you want to override macOS's notion of which display is main.

### Library

<p align="center">
  <img src="docs/screenshots/03-library.png" alt="Waraq Settings - Library pane" width="80%">
</p>

Every wallpaper you've imported. Drag MP4 or GIF files onto the Library tab from anywhere - Finder, your browser, wherever. Or use the Import menu for files / folders / GIF URLs.

Right-click a wallpaper for context menu actions: set custom thumbnail, reset to auto-generated, etc.

### Gallery

<p align="center">
  <img src="docs/screenshots/04-gallery.png" alt="Waraq Settings - Gallery pane" width="80%">
</p>

Three built-in sources, all free:

- **Pixabay** - thousands of stock videos, requires a free API key from [pixabay.com/api/docs/](https://pixabay.com/api/docs/)
- **Pexels** - high-quality curated stock, requires a free API key from [pexels.com/api/](https://www.pexels.com/api/)
- **NASA** - space and Earth observation videos, no key required, public domain

Search, pick something, click Add to Library. It downloads and shows up in Library, ready to assign to any display.

Searches are cached locally for 24 hours.

### Browse Web

<p align="center">
  <img src="docs/screenshots/05-gallery-browse-web.png" alt="Waraq Settings - Gallery Browse Web tab" width="80%">
</p>

For anime, gaming, and themed wallpapers, the Browse Web section lists curated external sites:

- [MotionBGs](https://motionbgs.com) - anime, gaming, cyberpunk 4K live wallpapers
- [MoeWalls](https://moewalls.com) - one of the largest anime live wallpaper libraries
- [MyLiveWallpapers](https://mylivewallpapers.com) - diverse community collection
- [Wallsflow](https://wallsflow.com) - anime, gaming, abstract

Click "Open in Browser" on any card to open that site in your default browser. Find a wallpaper you like, download the MP4 file from their site, drag it onto Waraq's Library tab.

Waraq does not scrape, mirror, or redistribute any content from these sites. They handle their own personal-use licensing - you're downloading directly from them, not through Waraq. This pattern keeps Waraq legally clean and free of takedown risk while still giving you access to the content.

## Built-in wallpapers

Six procedural animations ship with Waraq, generated at runtime via SwiftUI with zero file footprint:

- Animated Gradient
- Aurora Borealis
- Matrix Rain
- Synthwave Drive
- Starfield
- Neural Network

Use them as default options, fallbacks for empty libraries, or just because they look good.

## Performance

<p align="center">
  <img src="docs/screenshots/06-performance.png" alt="Waraq Settings - Performance pane" width="80%">
</p>

Animated wallpapers cost CPU and GPU. Waraq's defaults minimize this:

- **Pauses behind fullscreen apps** - if you're in a fullscreen app or game, the wallpaper isn't visible anyway, so it stops rendering
- **Pauses on battery** by default - configurable per display
- **Pauses on thermal pressure** when your Mac is running hot
- **Hardware decoding** when available
- **Configurable render quality** - automatic, low, medium, high
- **Configurable frame rate cap** per display
- **Drops frames under heavy load** rather than slowing the system
- **Memory limit per wallpaper** - default 250 MB, adjustable

If Waraq is using more resources than you're comfortable with, you can dial all of this down in the Performance pane.

## Requirements

- macOS 14 Sonoma or later
- Apple Silicon or Intel
- Roughly 80 MB disk for the app itself
- More disk for whatever wallpapers you import (your call)

## Windows port (this repository)

Active Windows work lives under **`windows/`**. Architecture decision: **C# / .NET 8 + WPF**, wallpaper host strategy **WorkerW (Progman)**. Full write-up: [docs/adr/0001-windows-tech-stack-and-wallpaper-host.md](docs/adr/0001-windows-tech-stack-and-wallpaper-host.md).

| Item | Path |
|------|------|
| Solution | `windows/WaraqWindows.sln` |
| App | `windows/Waraq.Windows/` |
| Tests | `windows/Waraq.Windows.Tests/` |
| Windows README | [windows/README.md](windows/README.md) |

### Requirements (Windows developers)

- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build, test, run (Windows)

```powershell
git clone https://github.com/taroo0ooq/waraq-windows.git
cd waraq-windows/windows
dotnet restore WaraqWindows.sln
dotnet build WaraqWindows.sln -c Release
dotnet test WaraqWindows.sln -c Release
dotnet run --project Waraq.Windows -c Release
```

Phase 2 MVP: pick a local **video** or **GIF**, **Apply wallpaper** (WorkerW host across the virtual desktop), **Stop** to tear down. App exit also stops wallpaper.

Deferred: battery/fullscreen pause, per-display profiles, gallery APIs, procedural pack.

## Building macOS upstream sources (reference only)

The original macOS app sources remain at the repo root. You do **not** need them to work on the Windows port.

Requires macOS, Xcode 16+, and [XcodeGen](https://github.com/yonaskolb/XcodeGen) (`brew install xcodegen`).

```bash
git clone https://github.com/taroo0ooq/waraq-windows.git
cd waraq-windows
xcodegen generate
open Waraq.xcodeproj
```

```bash
xcodebuild \
  -project Waraq.xcodeproj \
  -scheme Waraq \
  -destination 'platform=macOS' \
  -configuration Debug \
  CODE_SIGNING_ALLOWED=NO \
  build
```

`Waraq.xcodeproj` is gitignored — generated from `project.yml`. Deeper macOS notes: [`CLAUDE.md`](CLAUDE.md).

## License

Waraq is licensed under the **GNU General Public License v3.0**. See [LICENSE](LICENSE) for the full text.

What this means in practice:

- Free to use forever
- You can study, modify, and redistribute the code
- Any redistribution must also be GPL v3 (no closed-source forks)
- Nobody can practically sell Waraq because anyone who receives a copy gets the right to give it away free under the same license

This is intentional. Waraq stays free.

Read the full license at [gnu.org/licenses/gpl-3.0.html](https://www.gnu.org/licenses/gpl-3.0.html).

Copyright (C) 2026 Omar A. Othman.

## Roadmap

Planned after v1:

- Configurable library location (point Waraq at an external drive instead of `~/Library/Application Support/`)
- More built-in procedural wallpapers
- More Gallery sources, when good free APIs exist
- Community-submitted wallpaper section, if there's demand
- Polish on the import flows

No timelines. It ships when it ships.

## Contributing

PRs welcome. For non-trivial changes, open an issue first to discuss.

By contributing, you agree your contribution is also GPL v3.

Code style is enforced by SwiftLint and SwiftFormat. Tests live in `Tests/`. Don't break existing tests, and add new ones for new functionality where it makes sense.

## Credits

Built by Omar A. Othman ([@bahamut42](https://github.com/bahamut42)).

Coded with significant assistance from Anthropic's Claude.

Stock wallpaper sources integrated in the Gallery feature:

- [Pixabay](https://pixabay.com)
- [Pexels](https://www.pexels.com)
- [NASA Image and Video Library](https://images.nasa.gov)

Browse Web tab lists (Waraq is not affiliated with these, doesn't redistribute their content, just links to them):

- [MotionBGs](https://motionbgs.com)
- [MoeWalls](https://moewalls.com)
- [MyLiveWallpapers](https://mylivewallpapers.com)
- [Wallsflow](https://wallsflow.com)

## Support

If Waraq is useful to you and you want to throw something the developer's way: [paypal.me/OOthman666](https://paypal.me/OOthman666).

Not required. Ever.
