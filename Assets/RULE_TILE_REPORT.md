# Platform Rule Tiles

The terrain sheet contains two matching 47-tile blob families:

- `Platform_Border_Final.asset` uses sprites `Terrain (32x32)_0` through `_46`.
- `Platform_Inner_Pink_Final.asset` uses sprites `_47` through `_93`.

Both assets are already included at the end of `New Tile Palette`.

## How to paint

For the complete red-wall room shown in the reference, paint a filled shape with
`Platform_Border_Final` on one Tilemap. It is a self-contained composite Rule Tile:

1. Boundary cells use red brick with cream trim facing inward.
2. Fully surrounded cells automatically use the pink center brick.
3. Only boundary rules use Grid colliders; the pink center uses None.

Use `Platform_Inner_Pink_Final` separately when a shape should contain only the pink
wood-brick family without the red outer wall.

Recommended component settings:

| Tilemap | Rule Tile | Sorting Order | Collider |
| --- | --- | ---: | --- |
| Pink-only background | `Platform_Inner_Pink_Final` | 0 | None |
| Composite platform/room | `Platform_Border_Final` | 1 | Boundary only |

If a `TilemapCollider2D` is added to the composite Platform Tilemap, it follows the
red boundary while leaving the pink interior without per-tile colliders.

## Rule design

This is the standard 47-tile blob topology. Cardinal neighbors determine exposed
edges; diagonal neighbors select convex and concave corners. The red artwork is
inward-facing, so each red rule uses the sprite associated with the 180-degree
opposite pink topology. For example, an exposed top edge uses red sprite `_24`, not
the superficially paired `_1`. The four compact outer corners are the artwork-specific
exceptions: `_4`, `_5`, `_15`, and `_16`. The thick alternatives extend cream strips
to the outer edge and must not be used for a normal convex room corner. Rules use
fixed output with no runtime sprite rotation because the pixel-art lighting and brick
patterns are directional.

- Pink/background: 47 visible rules, including the fully surrounded center.
- Red/composite: 46 red boundary rules; the fully surrounded default is the pink
  center sprite with no collider.
- Source texture: 32 pixels per unit, point filtering, no mipmaps.

## Validation

Run `Tools > Platform Rule Tiles > Validate` in Unity. The validator checks:

- both assets load and contain the expected 46/47 rules;
- every rule has a valid sprite from the correct half of the sheet;
- every one of the 256 possible 8-neighbor layouts resolves;
- every red result uses the correct 180-degree opposite pink topology;
- straight edges and four rectangle corners have red outside and cream inside;
- the composite fully surrounded cell uses the pink center sprite;
- all collider and no-rotation requirements are preserved.

Batch mode entry point:

```text
ProjectWoods.Editor.PlatformRuleTileValidator.ValidateBatchMode
```
