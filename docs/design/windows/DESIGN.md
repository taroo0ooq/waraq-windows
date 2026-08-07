# Waraq Windows — Design system (WRQ-WIN-002)

| Field | Value |
|-------|--------|
| **Status** | Phase 0 draft (locked for Phase 1–2 implementation) |
| **work_id** | WRQ-WIN-002 |
| **source of truth (mac)** | `docs/design/design-tokens.md`, `settings-shell.md`, pane specs, `docs/screenshots/*` |
| **UI toolkit** | **WinUI 3 + Windows App SDK** (see ADR 0003) |
| **Modes** | Light + dark required; dark-first QA |

This document maps macOS design tokens and Settings shell geometry to **WinUI 3 ResourceDictionary** outlines. Implementers must not invent chrome colors.

---

## 1) Principles (from mac global rules)

1. Dark and light both first-class; live switch without relaunch.  
2. System fonts only (no bundled display fonts).  
3. Semantic chrome colors; brand crimson only in content/icon.  
4. Materials for flyouts; Settings window standard chrome.  
5. Sentence case UI strings. No em dashes in UI copy.  
6. Basic vs Advanced global mode (Diagnostics only in Advanced).  
7. Screenshot-driven: every pane PR links a mac reference PNG.

---

## 2) Typography map

| mac token | Size / weight | Windows (WinUI) |
|-----------|---------------|-----------------|
| Pane title | 22 / 500 | `SubtitleTextBlockStyle` ~20–22sp, SemiBold |
| Modal title | 15 / 500 | `BodyStrongTextBlockStyle` |
| Body / list | 13 / 400 | `BodyTextBlockStyle` 13–14sp |
| Control label | 12 / 400 | Caption / Body 12sp |
| Section header | 11 / 500 UPPER | CaptionTextBlockStyle + CharacterSpacing 50; Secondary brush |
| Helper | 11 / 400 | Caption Secondary |
| Metadata | 10 / 400 | Caption |
| Pill | 9 / 500 UPPER | 10sp SemiBold |

**Font family:** `Segoe UI Variable` / theme default (`XamlAutoFontFamily`).  
**Do not** use weight Black/Bold for chrome; prefer Normal/SemiBold to mirror 400/500.

---

## 3) Color tokens → WinUI resources

### 3.1 Chrome (theme brushes)

| Token | mac | WinUI ThemeResource / custom |
|-------|-----|------------------------------|
| Window bg | windowBackgroundColor | `ApplicationPageBackgroundThemeBrush` / Mica backdrop optional |
| Primary text | labelColor | `TextFillColorPrimaryBrush` |
| Secondary text | secondaryLabelColor | `TextFillColorSecondaryBrush` |
| Tertiary | tertiaryLabelColor | `TextFillColorTertiaryBrush` |
| Disabled | quaternaryLabelColor | `TextFillColorDisabledBrush` |
| Hairline | separatorColor | `DividerStrokeColorDefaultBrush` |
| Selection | selected @ 0.18 | `SubtleFillColorSecondaryBrush` + accent tint 18% |
| Control bg | controlBackgroundColor | `ControlFillColorDefaultBrush` |
| Flyout material | regularMaterial | `AcrylicBrush` / `SystemControlTransientBackgroundBrush` |

### 3.2 Semantic states

| Token | Hex (guide) | WinUI |
|-------|-------------|-------|
| Live / OK | `#34c759` | `SystemFillColorSuccessBrush` / custom `WaraqBrush.Live` |
| Paused / Warn | `#ffb400` | `SystemFillColorCautionBrush` / `WaraqBrush.Paused` |
| Error | systemRed | `SystemFillColorCriticalBrush` |
| Accent / info | `#5ea7ff` | `AccentFillColorDefaultBrush` (or fixed blue if accent hijacked) |

### 3.3 Brand (content only)

| Name | Dark | Light | Use |
|------|------|-------|-----|
| Crimson | `#c83a4a` | `#a82838` | Icon, content moments only |
| Cream | `#f2ece4` | — | Icon crow on dark |
| Near black | `#0c0808` | — | Icon crow on light |

**Never** use brand crimson for Settings chrome, sidebar selection, or buttons (use system accent / semantic).

### 3.4 Suggested ResourceDictionary skeleton

```xml
<!-- Themes/Waraq.xaml (outline) -->
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

  <!-- Spacing -->
  <x:Double x:Key="WaraqSpace100">4</x:Double>
  <x:Double x:Key="WaraqSpace150">6</x:Double>
  <x:Double x:Key="WaraqSpace200">8</x:Double>
  <x:Double x:Key="WaraqSpace250">10</x:Double>
  <x:Double x:Key="WaraqSpace300">12</x:Double>
  <x:Double x:Key="WaraqSpace350">14</x:Double>
  <x:Double x:Key="WaraqSpace400">16</x:Double>
  <x:Double x:Key="WaraqSpace500">20</x:Double>
  <x:Double x:Key="WaraqSpace550">22</x:Double>
  <x:Double x:Key="WaraqSpace600">24</x:Double>
  <x:Double x:Key="WaraqSpace700">28</x:Double>
  <x:Double x:Key="WaraqSpace800">32</x:Double>

  <!-- Radii -->
  <CornerRadius x:Key="WaraqRadiusChip">4</CornerRadius>
  <CornerRadius x:Key="WaraqRadiusButton">6</CornerRadius>
  <CornerRadius x:Key="WaraqRadiusCard">8</CornerRadius>
  <CornerRadius x:Key="WaraqRadiusSheet">10</CornerRadius>

  <!-- Card chrome (map opacity to theme dictionaries Light/Dark) -->
  <!-- Light: primary 4% fill; Dark: white 4% fill -->
  <SolidColorBrush x:Key="WaraqCardBackgroundBrush" Color="{ThemeResource CardBackgroundFillColorDefault}" />
  <Thickness x:Key="WaraqCardBorderThickness">0.5</Thickness>
  <Thickness x:Key="WaraqCardRowPadding">14,11</Thickness>

  <!-- Sidebar -->
  <x:Double x:Key="WaraqSidebarWidth">190</x:Double>
  <x:Double x:Key="WaraqSidebarItemHeight">28</x:Double>
  <x:Double x:Key="WaraqSidebarIconSize">15</x:Double>

  <!-- Brand (content) -->
  <Color x:Key="WaraqBrandCrimsonDark">#FFC83A4A</Color>
  <Color x:Key="WaraqBrandCrimsonLight">#FFA82838</Color>
</ResourceDictionary>
```

Split **Light** / **Dark** ThemeDictionaries for card fill opacities matching mac 0.04 overlays.

---

## 4) Layout geometry (Settings shell)

| Spec | mac | Windows |
|------|-----|---------|
| Default window | 720 × 560 pt | 720 × 560 dip (min 640 × 480) |
| Sidebar width | 190 pt fixed | 190 dip `OpenPaneLength` |
| Content padding | 24 v / 28 h | same |
| Section header top | 22 pt | `WaraqSpace550` |
| Nav item height | 28 pt | 28 dip |
| Search field height | 26 pt | AutoSuggestBox ~32 min hit target OK if visual densified |

**Pattern:** `NavigationView` pane left + `Frame` content **or** custom two-column Grid matching screenshot density more tightly than stock NavView. Prefer **custom sidebar** if stock NavView chrome diverges from mac screenshots (Phase 2 decision with owner screenshots).

### 4.1 Sidebar items (Basic)

| Order | Label | mac SF | Windows icon |
|------:|-------|--------|--------------|
| 1 | General | gearshape | `Setting` / Segoe Fluent |
| 2 | Displays | display | `Desktop` / `TVMonitor` |
| 3 | Library | photo.on.rectangle | `Photo2` |
| 4 | Performance | speedometer | `SpeedHigh` |
| 5 | Wallpapers | square.stack.3d.up | `Picture` / stack |
| 6 | Gallery | (gallery) | `Globe` / `Search` |
| 7 | About | info.circle | `Info` |

**Advanced only:** Diagnostics (`Stethoscope` / medical) + ADV pill.

### 4.2 Card / row

- Background subtle fill; 0.5dip border; radius 8  
- Row padding 14×11  
- Label left, control right  
- Sub-setting indent 12 + 2dip accent bar system blue @ 40%

---

## 5) Controls map

| mac | Windows |
|-----|---------|
| Toggle switch | `ToggleSwitch` |
| Popup menu | `ComboBox` |
| Slider + readout | `Slider` + `TextBlock` |
| Default button | `Button` style outline |
| Primary button | Accent `Button` |
| Destructive | Critical brush style |
| Pill badge | `Border` + Caption |

---

## 6) Materials & motion

| Surface | Windows |
|---------|---------|
| Tray flyout | Acrylic / system popup |
| Settings window | Mica optional; solid fallback if Reduce Transparency |
| Wallpaper crossfade | 0–3s default 1.5s; skip if Reduce Motion |

No drop shadows on chrome cards (border only), matching mac tokens.

---

## 7) Iconography

- Prefer **Segoe Fluent Icons** / WinUI Symbol; maintain mapping table in code comments from SF Symbol list in `design-tokens.md`.  
- Tray: monochrome template-style `.ico` (white-on-transparent), 16–32px.  
- Do not ship Tabler font.

---

## 8) Accessibility

- Narrator names on all nav + toggles  
- Keyboard: Tab order, Esc closes flyouts, Ctrl+, opens Settings (register if possible)  
- Contrast AA both themes  
- Min hit target 28×28 dip  
- High contrast theme smoke in Phase 8  

---

## 9) Screenshot references (Phase 2 PR checklist)

| Pane | mac PNG |
|------|---------|
| General | `docs/screenshots/01-settings-general.png` |
| Displays | `docs/screenshots/02-displays.png` |
| Library | `docs/screenshots/03-library.png` |
| Gallery | `docs/screenshots/04-gallery.png` |
| Browse Web | `docs/screenshots/05-gallery-browse-web.png` |
| Performance | `docs/screenshots/06-performance.png` |
| About | `docs/screenshots/07-about.png` |

Every Phase 2 pane PR description must embed or link the matching PNG.

---

## 10) Out of scope for this doc

- Full XAML for each pane (Phase 2+)  
- Host wallpaper rendering (ADR 0003 + Phase 3)  
- Marketing visuals  

---

## Changelog

- 2026-08-07 — Phase 0 initial map (Atlas Forge, WRQ-WIN-002).
