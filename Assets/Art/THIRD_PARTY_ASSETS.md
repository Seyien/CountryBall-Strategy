# Third-party art asset manifest

Last verified: 2026-08-16  
Scope: the 17 PNG files under `Assets/Art/ThirdParty/Kenney/` only.

## Licence and source record

### Kenney Tiny Battle 1.0

- Product page: <https://kenney.nl/assets/tiny-battle>
- Archive read: <https://kenney.nl/media/pages/assets/tiny-battle/c1c25ac1f3-1691487575/kenney_tiny-battle.zip>
- Archive SHA-256: `7751EC7D9A07E57BAA9FA1174D6F78FCD779A050377227AFEE77993C73CB5F9E`
- Exact licence: **CC0 1.0 Universal**
- Full CC0 legal text read: <https://creativecommons.org/publicdomain/zero/1.0/legalcode>
- In-archive text read: `License.txt` says, “You can use this content for personal, educational, and commercial purposes.” It also says crediting `Kenney` or `www.kenney.nl` is not required.

### Kenney Tiny Dungeon 1.0

- Product page: <https://kenney.nl/assets/tiny-dungeon>
- Archive read: <https://opengameart.org/sites/default/files/kenney_tinydungeon.zip>
- Archive SHA-256: `C109438AB06F65FD80F9B2686A4CF9C7C11DC64444B47333EC71D602F8BB5FC7`
- Exact licence: **CC0 1.0 Universal**
- Full CC0 legal text read: <https://creativecommons.org/publicdomain/zero/1.0/legalcode>
- In-archive text read: `License.txt` says the content is free for personal, educational, and commercial projects. It says crediting `Kenney` or `www.kenney.nl` is not mandatory.

### Kenney Tiny Town 1.1

- Product page: <https://kenney.nl/assets/tiny-town>
- Archive read: <https://kenney.nl/media/pages/assets/tiny-town/a415fbeb49-1735736916/kenney_tiny-town.zip>
- Archive SHA-256: `9768692DCCFF1D706408A5AEDD6CA4F6CD1409506CBC84CB2F862919764BE977`
- Exact licence: **CC0 1.0 Universal**
- Full CC0 legal text read: <https://creativecommons.org/publicdomain/zero/1.0/legalcode>
- In-archive text read: `License.txt` permits personal, educational, and commercial projects and says crediting `Kenney` or `www.kenney.nl` is not mandatory.
- Selection note: Tiny Town grass IDs `0/1/2` were deliberately not duplicated because their SHA-256 values are identical to Tiny Battle's corresponding grass files. This first ground import uses five dirt-fill sprites only.

### Rights verdict

Commercial and portfolio use are allowed. Raw files may remain in a public GitHub repository under CC0's waiver/public-licence fallback. Attribution is not legally required. No AI-training-specific restriction or resale prohibition appears in the pack declarations or CC0 text. CC0 does not waive trademark, patent, privacy/publicity, third-party, endorsement, or warranty limitations.

Required attribution string: **none**.

Voluntary README attribution string used for provenance:

> Art assets: “Tiny Battle”, “Tiny Dungeon”, and “Tiny Town” by Kenney — https://kenney.nl — released under CC0 1.0 Universal: https://creativecommons.org/publicdomain/zero/1.0/. Attribution is not required; this credit is included for provenance. Gameplay code, systems design, UI behavior/layout, level composition, and Unity integration are original project work.

## Import contract

All 17 files are separate, grid-aligned `16x16` RGBA PNGs imported as `Sprite (2D and UI)` / `Single` with `Pixels Per Unit = 16`, `Filter Mode = Point (no filter)`, mipmaps disabled, compression `None/Uncompressed`, alpha transparency enabled, center pivot, and clamp wrapping. The Tiny Town metadata uses the same Unity 2021.3.45f2 serializer schema already proved for the existing project art.

No image was resized, redrawn, recolored, or palette-adjusted. File SHA-256 values below therefore bind both the extracted source PNG and imported PNG.

## Exact file ledger

| Pack / source ID | Intended role | Imported file | SHA-256 | Palette change |
|---|---|---|---|---|
| Tiny Battle `tile_0131.png` | Friendly support transport | `Assets/Art/ThirdParty/Kenney/TinyBattle/Units/Friendly/friendly_support_transport_tile_0131.png` | `EA618DE3EE2FC363AC3A4DB20A9230E5CA7962D3AC01B448539901C46CF54216` | None |
| Tiny Battle `tile_0136.png` | Friendly air scout | `Assets/Art/ThirdParty/Kenney/TinyBattle/Units/Friendly/friendly_air_scout_tile_0136.png` | `2E9FC011DC273ADBE87D25161E9549B7D914E5D1DC2E3BEDB3288275DD0148E2` | None |
| Tiny Battle `tile_0142.png` | Friendly vanguard infantry | `Assets/Art/ThirdParty/Kenney/TinyBattle/Units/Friendly/friendly_vanguard_infantry_tile_0142.png` | `32F933BEC7C2D0E08FE6718C9BFBF0E2B303403E1870688A45CD1AB11C052748` | None |
| Tiny Battle `tile_0155.png` | Enemy air raider | `Assets/Art/ThirdParty/Kenney/TinyBattle/Units/Enemy/enemy_air_raider_tile_0155.png` | `BF6B32E1FB6EE04E0361FC2B8022CB678EB5CE1BBEA452D3CEE4590CA8A44E89` | None |
| Tiny Battle `tile_0158.png` | Enemy heavy vehicle | `Assets/Art/ThirdParty/Kenney/TinyBattle/Units/Enemy/enemy_heavy_vehicle_tile_0158.png` | `718AB1324B8A90EABA1EF46213273F955AB1FB51A1A80C9EE3F2CE4163B9212E` | None |
| Tiny Battle `tile_0161.png` | Enemy ranged infantry | `Assets/Art/ThirdParty/Kenney/TinyBattle/Units/Enemy/enemy_ranged_infantry_tile_0161.png` | `65A25C501E4351FA060025A8D90F95F0B678B7BEC234FEEB33618442421E6140` | None |
| Tiny Battle `tile_0061.png` | Unit selection bracket overlay | `Assets/Art/ThirdParty/Kenney/TinyBattle/UI/selection_unit_bracket_tile_0061.png` | `D5911A981FA5E494B87C9FB40FCDF1367523DB5C82E6AADA447B6E84133C9240` | None |
| Tiny Battle `tile_0045.png` | Friendly unit-producer command depot | `Assets/Art/ThirdParty/Kenney/TinyBattle/Buildings/friendly_command_depot_tile_0045.png` | `1A2A71F4A4CF3948FB077A06635B76900BF3224297FDF26D81326E3BF77D729E` | None |
| Tiny Battle `tile_0048.png` | Friendly resource-producer industrial pump | `Assets/Art/ThirdParty/Kenney/TinyBattle/Buildings/friendly_industrial_pump_tile_0048.png` | `6645AFF06B981E2DB5F4E760CBD16256A6FBE053AD233E614D510FC6322634AB` | None |
| Tiny Dungeon `tile_0104.png` | Vanguard sword / attack cue | `Assets/Art/ThirdParty/Kenney/TinyDungeon/Equipment/vanguard_sword_tile_0104.png` | `3B38808F0C20A49A9923475022041C46E8A04B47D8621E5186FE153B4BBAED48` | None |
| Tiny Dungeon `tile_0117.png` | Engineer hammer / attack cue | `Assets/Art/ThirdParty/Kenney/TinyDungeon/Equipment/engineer_hammer_tile_0117.png` | `4F75A60E03D46523AFBA3C52F73C1FCBE06A7566A87EEA9FB9B82320718182BF` | None |
| Tiny Dungeon `tile_0130.png` | Support staff / attack cue | `Assets/Art/ThirdParty/Kenney/TinyDungeon/Equipment/support_staff_tile_0130.png` | `A61E8A3FE7E1E6F192D6B6452852274A02768C20A45EF6AFA48EC80908A39185` | None |
| Tiny Town `tile_0025.png` | Plain dirt ground fill | `Assets/Art/ThirdParty/Kenney/TinyTown/Terrain/Dirt/dirt_fill_plain_tile_0025.png` | `63519065228F5F8AF6200D979FA54CD7F00650034CEEA10A1609203A34B3FF9B` | None |
| Tiny Town `tile_0039.png` | Dirt ground scatter A | `Assets/Art/ThirdParty/Kenney/TinyTown/Terrain/Dirt/dirt_fill_scatter_a_tile_0039.png` | `FD9127CE420B7E05BA2BB361A82C9301B95D2C55D3EAB899198819E9BA78214A` | None |
| Tiny Town `tile_0040.png` | Dirt ground scatter B | `Assets/Art/ThirdParty/Kenney/TinyTown/Terrain/Dirt/dirt_fill_scatter_b_tile_0040.png` | `1AB17E4B24BE7EAEB6D8830C0E763B58A66FE2B8F70C15242FBDE5138007D334` | None |
| Tiny Town `tile_0041.png` | Dirt ground scatter C | `Assets/Art/ThirdParty/Kenney/TinyTown/Terrain/Dirt/dirt_fill_scatter_c_tile_0041.png` | `3DB01FF694F3D9F1E492D45913C3F85FF5F704AC262A1404D368C9B272A3F42D` | None |
| Tiny Town `tile_0042.png` | Dirt ground scatter D | `Assets/Art/ThirdParty/Kenney/TinyTown/Terrain/Dirt/dirt_fill_scatter_d_tile_0042.png` | `322A22E1567B667F5F5423767412014AC490968E0F6F6FD22F1721234784F9AA` | None |

### Measured note — the selection bracket is not a hollow frame

`tile_0061.png` was measured pixel by pixel before import. Its 256 pixels are
`160` opaque `RGB(63,38,49)` outline, `20` opaque `RGB(255,255,255)` bracket
marks, and `76` fully transparent pixels arranged as a plus-shaped cross. The
white marks form four L-shaped corner brackets, but they sit on an **opaque dark
backing plate** — the tile is not a hollow frame with a transparent centre.

Consequence for rendering, recorded here so it is not rediscovered: drawing this
sprite *above* a unit would cover 62.5% of the unit with opaque dark pixels. It
is therefore authored to render *below* the unit, as a selection pad. This is the
same ordering used by conventional RTS selection markers. Render order is
authored on the prefab's `SpriteRenderer`, not in C#, so the decision is one
Inspector value to reverse.

A survey of all 198 Tiny Battle tiles, plus the Tiny Town, Tiny Farm, Tiny Ski,
Tiny Dungeon, Micro Roguelike, and Pixel Shmup staging sets, found no
transparent-centre bracket in any licence-verified pack that is also palette
coherent with Tiny Battle. `tile_0061` is the only four-corner selection bracket
in the pack family the project already uses.

## Coherence evidence

Against the selected Tiny Battle subset, the three equipment sprites share 5 of 8 unique RGB values and 330 of 348 non-transparent pixels exactly (`94.828%`). Against the full Tiny Battle palette they share 7 of 8 unique RGB values and 344 of 348 non-transparent pixels exactly (`98.851%`). Both groups use the same dominant outline `RGB(63,38,49)`. The residual 18 subset-mismatched pixels are small highlight/accent colors, so recoloring would reduce rather than improve material readability.

The S04 diagnostic contact sheet, per-color measurements, nearest-color mapping, import receipt, and Unity serializer logs are retained under `parallel_sessions/S04_ASSET_SOURCING_RESEARCH/staging/COHERENCE/`.

The five imported Tiny Town dirt fills are part of the S05-measured 14-tile dirt family, which reached `99.247%` exact pixel coverage against the full Tiny Battle palette. The selected files remain unmodified. S05 evidence is retained under `parallel_sessions/S05_TERRAIN_VFX_RESEARCH/staging/TERRAIN/`.
