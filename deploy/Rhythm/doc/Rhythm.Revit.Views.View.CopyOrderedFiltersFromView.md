## In Depth

`View.CopyOrderedFiltersFromView(receivingView, sourceView)`

Revit 2021 - Copies view filters from the source view to the receiving view while preserving filter order. If the receiving view has a view template assigned, an exception will be thrown. If the source view has a view template assigned, the filters will be copied from that view template instead.

The inputs are:

- `receivingView` (_View_) — The target view to receive the filters.
- `sourceView` (_View_) — The source view to copy filters from.

Returns nothing.
