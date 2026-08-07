# WRQ-WIN-002 Phase 6 — Gallery + privacy

## Privacy contract (mac parity)

1. **No network** until the user presses **Search** (or **Import selected**, which downloads the chosen item).
2. **No telemetry / analytics / phone-home.**
3. **API keys** live only in `%AppData%\Waraq\gallery-keys.json` (local).
4. Keys are sent only as required by the chosen API host (Pixabay query param / Pexels Authorization header).
5. **Browse Web** opens `https://` links in the **default browser** via `ShellExecute`. No scrape, mirror, or proxy of MotionBGs / MoeWalls / MyLiveWallpapers / Wallsflow.
6. NASA requires **no** API key.

## Sources
| Source | Key? | Endpoint family |
|--------|------|-----------------|
| Pixabay | yes | `pixabay.com/api/videos` |
| Pexels | yes | `api.pexels.com/videos/search` |
| NASA | no | `images-api.nasa.gov/search` |

## Cache
24h disk cache under `%AppData%\Waraq\GalleryCache` (Pixabay terms).

## DAST note
Gallery introduces **optional user-initiated** HTTPS egress. Cipher Phase 6-Secure should re-evaluate DAST posture; this build does not claim DAST green for a permanent listener (still no inbound server).
