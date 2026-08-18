## In Depth

`Tools.ThreeDeeSpaceTags(space, tagType, spaceNameParameter: "Name", spaceNumberParameter: "Number")`

Create 3d space tags given the input spaces!

Superseded on Revit 2025 and up by Tools.ThreeDeeSpatialTags, which tags spaces as readily as rooms, from one dialog: every space in a phase, from this document or a link, updated in place on a second run instead of piling up a fresh set of duplicates. This node places one tag per space and knows nothing about the tags already there.

Still here, and still working, for Revit 2021 to 2024, where that node does not exist because the tag family it places cannot be loaded, and for graphs already built on it.

The inputs are:

- `space` (_Space_) — The spaces to place 3d space tags on.
- `tagType` (_FamilyType_) — The 3d space tag to use. (There is a sample RFA in the extra folder for Rhythm)
- `spaceNameParameter` (_string_, defaults to `"Name"`) — The name of your Name parameter, the sample has the parameter named as Space Name
- `spaceNumberParameter` (_string_, defaults to `"Number"`) — The name of your Number parameter, the sample has the parameter named as Space Number

Returns `result` (_FamilyInstance_).
