## In Depth

`CirclePacking.ByBoundary(boundary, count: 10, minRadius: 1, maxRadius: 5, iterations: 100, seed: -1)`

Packs circles within a boundary polygon using a particle-spring simulation system. Circles repel each other to prevent overlapping while remaining constrained within the boundary.

The inputs are:

- `boundary` (_Polygon_) — The boundary polygon to pack circles within
- `count` (_integer_, defaults to `10`) — Number of circles to pack (default: 10)
- `minRadius` (_number_, defaults to `1`) — Minimum circle radius (default: 1.0)
- `maxRadius` (_number_, defaults to `5`) — Maximum circle radius (default: 5.0)
- `iterations` (_integer_, defaults to `100`) — Number of simulation iterations — more iterations produce better packing (default: 100)
- `seed` (_integer_, defaults to `-1`) — Random seed for reproducible results, use -1 for a random seed (default: -1)

The outputs are:

- `circles` — The packed circles
- `centers` — The center points of the packed circles
- `radii` — The radii of the packed circles

Search terms: `circle packing`, `particle spring`, `generative design`, `circles`, `packing`.
