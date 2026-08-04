## In Depth

`Selection.IntersectingGridsByModelCurve(modelCurve)`

This node will select grids along a model curve element ordered based on the start of the model curve. This works in the active view. So whatever plan representation your grids have, that is what is used.

The inputs are:

- `modelCurve` (_ModelCurve_) — Revit model curve to select grids along.

Returns `orderedGrids` (_list of Grid_) — The intersecting grids ordered from beginning to end of the line.
