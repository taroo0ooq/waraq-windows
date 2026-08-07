# WRQ-WIN-002 Phase 5 — Procedural engines

## Mac parity set (6)

| Id | Name | mac source |
|----|------|------------|
| `aurora` | Aurora | AuroraView.swift |
| `matrix-rain` | Matrix Rain | MatrixRainView.swift |
| `synthwave` | Synthwave | SynthwaveView.swift |
| `starfield` | Starfield | StarfieldView.swift |
| `neural-network` | Neural Network | NeuralNetworkView.swift |
| `animated-gradient` | Animated Gradient | gradient family / factory |

## Runtime
- CPU BGRA frames → `WriteableBitmap` on WorkerW surface (~1/3 desktop res, ~30fps)
- Apply from Library pane **Procedural** list
- `WallpaperController.ApplyProcedural(id)`

## Limitations
- Not GPU/Win2D; quality is MVP parity visual language, not pixel-perfect mac shaders
- Heavy full-desktop CPU cost avoided via downscale + stretch
