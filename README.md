# CupkekGames Hex Grids

Deterministic hex grids in plain C#: the same inputs give the same tiles, paths and
ties every time, with no physics and no allocation in the queries. Built for
grid-based auto-battles, usable for any board.

## The parts

- **`HexCoord`**: a hex in axial coordinates (q, r); `S` is the third cube
  coordinate. `Distance` counts steps; `Neighbor` and `Direction` walk the six
  `HexDirection`s in a fixed order; `Round` finds the hex that holds a fractional
  position.
- **`HexLayout`**: where hexes sit in the world, on the XZ plane. `Orientation`
  (pointy or flat top), `Size` (centre to corner, the one world number a grid has)
  and `Origin`. `ToWorld`, `ToHex`, `Corner`, `Spacing`.
- **`HexShapes`**: `Range`, `Ring`, `Line` and `Cone`, written into your list in a
  fixed order.
- **`HexField`**: a rectangular board of columns by rows (offset rows), with blocked
  tiles. `Tiles` lists every tile row by row; `FromOffset` and `ToOffset` convert.
- **`HexOccupancy`**: one occupant per hex by integer id, with reservations: a mover
  reserves the next hex, then completes the move.
- **`HexPathfinder`**: A* over a field, to a hex or to the nearest hex a goal test
  accepts, with stable tie-breaks. Keep one per user; it reuses its buffers.
- **`HexFieldView`**: a field in a scene, centred on its transform, drawn as gizmos.
- **`HexTilePainter`**: paints any hexes of a field on the ground in play, each with its own fill and ring colour, in one mesh (`Paint`, `Erase`, `Clear`, then `Apply`; `Opacity` fades everything). Give it a transparent, vertex-coloured material.
- **`HexTileMesh`**: the mesh builder behind it: each hex as a flat fill and a ring inset from its edge, coloured by vertex colour.
- `HexFieldView` fails loudly on a rotated or scaled transform, since its layout follows the position only.

## Using it

```csharp
var field = new HexField(8, 4);
var layout = new HexLayout(HexOrientation.PointyTop, 1.1f, Vector3.zero);
var occupancy = new HexOccupancy();
occupancy.Place(1, HexField.FromOffset(6, 1));
occupancy.Place(2, HexField.FromOffset(1, 1));

// Unit 1 walks until unit 2 is within one step.
HexCoord target = occupancy.PositionOf(2);
var path = new List<HexCoord>();
var pathfinder = new HexPathfinder();
if (pathfinder.TryFindPath(field, occupancy.PositionOf(1),
        hex => HexCoord.Distance(hex, target) <= 1, target,
        hex => occupancy.IsFree(hex, 1), path) && path.Count > 0
    && occupancy.TryReserve(1, path[0]))
{
    // glide the model to layout.ToWorld(path[0]), then:
    occupancy.CompleteMove(1);
}
```

## Determinism

Coordinates and distances are integers. Every shape and every neighbour walk runs
in a fixed order, and the pathfinder breaks ties by estimate, then by the order it
found a hex. Same inputs, same results, on the same machine. Cross-platform
lockstep is not a goal: layout and cone angles use floating point.
