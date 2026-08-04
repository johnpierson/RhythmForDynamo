## In Depth

`CirclePacking.ByCentersAndRadii(centers, radii, iterations: 100)`

Packs circles from given starting positions using a particle-spring simulation system. Circles repel each other to eliminate overlaps while maintaining their original radii.

The inputs are:

- `centers` (_list of Point_) — Initial center points of the circles
- `radii` (_list of number_) — Radii of the circles. If fewer radii than centers are provided, the last radius is reused.
- `iterations` (_integer_, defaults to `100`) — Number of simulation iterations — more iterations produce better separation (default: 100)

The outputs are:

- `circles` — The packed circles
- `centers` — The center points of the packed circles
- `radii` — The radii of the packed circles

Search terms: `circle packing`, `particle spring`, `generative design`, `circles`, `packing`.
