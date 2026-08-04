## In Depth

`PolyCurve.ByRandomPoints(numberOfPoints, minX: 0, maxX: 100, minY: 0, maxY: 100, minZ: 0, maxZ: 0, connectToStart: false, seed: -1)`

Creates a PolyCurve from a random distribution of points.

The inputs are:

- `numberOfPoints` (_integer_) — The number of random points to generate
- `minX` (_number_, defaults to `0`) — Minimum X coordinate (default: 0)
- `maxX` (_number_, defaults to `100`) — Maximum X coordinate (default: 100)
- `minY` (_number_, defaults to `0`) — Minimum Y coordinate (default: 0)
- `maxY` (_number_, defaults to `100`) — Maximum Y coordinate (default: 100)
- `minZ` (_number_, defaults to `0`) — Minimum Z coordinate (default: 0)
- `maxZ` (_number_, defaults to `0`) — Maximum Z coordinate (default: 0)
- `connectToStart` (_boolean_, defaults to `false`) — Whether to connect the last point back to the first point (default: false)
- `seed` (_integer_, defaults to `-1`) — Random seed for reproducible results (default: uses time-based seed)

Returns `result` (_PolyCurve_) — A PolyCurve created from randomly distributed points
