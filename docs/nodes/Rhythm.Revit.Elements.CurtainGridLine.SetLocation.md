## In Depth

`CurtainGridLine.SetLocation(curtainGridLine, newLocation)`

This node will attempt to set the location of the given grid line to the given point. NOTE: This will "translate" the grid line parallel to where it is initially. We cannot move a U grid to a V grid and so forth.

The inputs are:

- `curtainGridLine` (_Element_) — The curtain grid line to try to set.
- `newLocation` (_Point_) — _Not documented yet._

Returns `curtainGridLine` (_object_) — The translated curtain grid line. (Returns null if failed)

Found in the library under **Actions**.
