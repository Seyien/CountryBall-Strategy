# Derived art assets

Last updated: 2026-08-25

Everything under `Assets/Art/Derived/` is a recolor of a CC0 source that
lives under `Assets/Art/ThirdParty/`. Sources are never edited; each
derivative is a new file whose name carries the source tile id.

## Licence

Source pack: Kenney "Tiny Battle" 1.0 — CC0 1.0 Universal
(verified in `Assets/Art/THIRD_PARTY_ASSETS.md`, archive SHA-256 on record).
CC0 places the work in the public domain; derivatives, including commercial
and portfolio use, are permitted without attribution.

## Method

Palette swap only (PNG PLTE entries rewritten, pixel index data untouched).
The friendly->enemy color mapping is the pack's own team scheme, measured
from its unit sprites:

| Roll  | Friendly (blue) | Enemy (red)   |
|-------|-----------------|---------------|
| dark  | 0,119,197       | 170,44,35     |
| mid   | 0,154,220       | 232,69,55     |
| light | 0,198,244       | 255,117,113   |

Outline `63,38,49`, white highlights, and the alpha channel are unchanged.
Each output was re-read and verified: same 16x16 size, same PNG color type 3
(indexed) at bit depth 4, alpha identical pixel-for-pixel to the source.

## File ledger

| Derived file | Source | Changed pixels |
|---|---|---|
| `Kenney/TinyBattle/Buildings/enemy_command_depot_from_tile_0045.png` | `ThirdParty/.../friendly_command_depot_tile_0045.png` | 124 of 256 |
| `Kenney/TinyBattle/Buildings/enemy_industrial_pump_from_tile_0048.png` | `ThirdParty/.../friendly_industrial_pump_tile_0048.png` | 118 of 256 |

## Import contract

Same as the ThirdParty sprites: Sprite (2D and UI) / Single,
Pixels Per Unit 16, Filter Mode Point, Compression None, mipmaps off,
center pivot. Unity generates the `.meta`; set these in the importer
after first import.
