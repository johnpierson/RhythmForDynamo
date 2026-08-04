## In Depth

`Selection.FromLink(refreshSelection, singleSelection)`

Select stuff from a link. Useful for Dynamo player.

The inputs are:

- `refreshSelection` (_boolean_) — Reset the selection and reselect new things
- `singleSelection` (_boolean_) — Enable single selection. False for multiple selection.

The outputs are:

- `selectedElements` — The selected elements.
- `transform` — If the link was moved this transform is needed to relocate the stuff.
