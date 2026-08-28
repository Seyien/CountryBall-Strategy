# Generated art assets

Last updated: 2026-08-28

Original, project-authored primitives (no third-party source, no licence
constraints). All are pure white so runtime code tints them via
`SpriteRenderer.color` / `Image.color`.

| File | Size | Content | Intended roles |
|---|---|---|---|
| `ui_white_square_4x4.png` | 4x4 | opaque white fill | health bar fill+background, production progress fill, panel backing |
| `ui_cell_frame_16x16.png` | 16x16 | 1px white border, transparent centre | hovered/target cell highlight, range indicator ring |
| `terrain_water_16x16.png` | 16x16 | white fill + 24 slightly darker ripple pixels | layer-0 backdrop bands (open sea, shoal, beach), tinted per band |

## Import contract

Sprite (2D and UI) / Single, Filter Mode Point, Compression None,
mipmaps off, center pivot.
Pixels Per Unit: 16 for `ui_cell_frame_16x16` and `terrain_water_16x16`
(one board cell); 4 for `ui_white_square_4x4` (one world unit) - or scale in
the prefab.

`terrain_water_16x16` additionally needs Mesh Type = Full Rect, because it is
drawn with `SpriteRenderer.drawMode = Tiled`, which silently refuses to tile a
Tight mesh. `SceneSetupTool.ConfigureSpriteImports` enforces all of the above.

## Why `terrain_water_16x16` exists

Every terrain tile the project owns was measured pixel by pixel:
`dirt_fill_plain_tile_0025` and `grass_plain_tile_0000` are 256/256 pixels of a
single colour, and the scatter/tufts/flowers variants carry only 19-62 detail pixels out of 256.
A wide layer laid out of those tiles is therefore indistinguishable on screen
from a flat camera background colour. Tinting cannot rescue them either: the
dirt tile's blue channel peaks at 108/255, so multiplying it can never produce a
sea blue.

This tile is white on purpose - `SpriteRenderer.color` multiplies, so a white
base can become any colour, while the 24 darker pixels survive the tint and keep
the texture. One file therefore serves three differently coloured bands.
