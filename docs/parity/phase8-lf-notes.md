# WRQ-WIN-002 Phase 8 — L&F / a11y / DPI

## Owner L&F accept #2 (visual gate)
Desk does **not** self-pass visual L&F. Owner reviews this pack after Stagecraft 8-QA.

### Screenshot pack
| File | Notes |
|------|--------|
| `docs/screenshots/windows/phase8/phase8-desktop-settings.png` | Live Settings shell on desktop (Phase 8 build) |
| `docs/screenshots/windows/phase8/01-general.png` | Cover / General focus |
| `docs/screenshots/windows/phase2-settings-shell.png` | Earlier shell baseline |

Mac references: `docs/screenshots/*` + `docs/design/settings-*.md`.

## L&F polish delivered
- Expanded `Themes/Waraq.xaml` (spacing, cards, helper/title styles, button radii)
- Mica backdrop on Settings window (when available)
- Footer status bar + **DPI badge** (live scale)
- General pane card chrome using design tokens
- Dark/light follow system theme (Fluent)

## Accessibility
- `AutomationProperties.Name` / HelpText on NavigationView items
- Landmarks: navigation + main content
- Advanced toggle named for Narrator
- Access keys from pane title first letter
- Pane titles use heading levels in styles
- Status live region (polite)

## High-DPI
- `app.manifest`: **PerMonitorV2** + `true/pm` fallback
- Runtime badge via `DpiProbe.GetDpiForWindow`
- WinUI layout uses effective pixels (dip)

## Honest Windows Fluent deltas
| mac | Windows |
|-----|---------|
| Vibrancy / materials | Mica / Acrylic (system) — not pixel-identical |
| SF Pro | Segoe UI Variable |
| Crimson brand chrome | Brand reserved for content/icon only |
| IOKit thermal | Not claimed (Phase 7) |

## Not claimed
- Owner L&F accept (human gate)
- Pixel-perfect mac parity
