# Generated art assets

Last updated: 2026-08-25

Original, project-authored primitives (no third-party source, no licence
constraints). All are pure white so runtime code tints them via
`SpriteRenderer.color` / `Image.color`.

| File | Size | Content | Intended roles |
|---|---|---|---|
| `ui_white_square_4x4.png` | 4x4 | opaque white fill | health bar fill+background, production progress fill, panel backing |
| `ui_cell_frame_16x16.png` | 16x16 | 1px white border, transparent centre | hovered/target cell highlight, range indicator ring |

## Import contract

Sprite (2D and UI) / Single, Filter Mode Point, Compression None,
mipmaps off, center pivot.
Pixels Per Unit: 16 for `ui_cell_frame_16x16` (one board cell);
4 for `ui_white_square_4x4` (one world unit) - or scale in the prefab.
