## In Depth

`Elements.AreaLocation(element)`

*BETA* - This node will retrieve the closest area that an element resides in. This uses bounding boxes which encompass the whole geometry, so we take the closest one. This means that there is potential that we grab the wrong one..

The inputs are:

- `element` (_Element_) — The element to find the closest area location for.

Returns `area` (_Element_) — The closest area.

Found in the library under **Actions**.

Search terms: `Element.AreaLocation`.
