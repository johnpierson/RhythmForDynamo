## In Depth

`GenerativeDesign.PackViewports(container, viewportRectangles, viewportIds, marginFromEdge: 0)`

Packs viewport rectangles within a container rectangle, ensuring no overlap with container edges

The inputs are:

- `container` (_Rectangle_) — The container rectangle to pack viewports within
- `viewportRectangles` (_list of Rectangle_) — List of viewport rectangles to pack
- `viewportIds` (_list of integer_) — List of viewport IDs corresponding to the rectangles
- `marginFromEdge` (_number_, defaults to `0`) — Minimum margin distance from container edges

The outputs are:

- `viewportsThatFit` — The viewports that fit in the titleblock container
- `proposedLocations` — The proposed locations
- `viewportRectangles` — The rectangles
