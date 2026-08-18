## In Depth

`Tools.ThreeDeeRoomTags(room, tagType, roomNameParameter: "Name", roomNumberParameter: "Number")`

Create 3d room tags given the input rooms!

Superseded on Revit 2025 and up by Tools.ThreeDeeSpatialTags, which does the whole job from one dialog: every room or space in a phase, from this document or a link, updated in place on a second run instead of piling up a fresh set of duplicates. This node places one tag per room and knows nothing about the tags already there.

Still here, and still working, for Revit 2020 to 2024, where that node does not exist because the tag family it places cannot be loaded, and for graphs already built on it.

The inputs are:

- `room` (_Room_) — The rooms to place 3d room tags on.
- `tagType` (_FamilyType_) — The 3d room tag to use. (There is a sample RFA in the extra folder for Rhythm)
- `roomNameParameter` (_string_, defaults to `"Name"`) — The name of your Name parameter, the sample has the parameter named as Room Name
- `roomNumberParameter` (_string_, defaults to `"Number"`) — The name of your Number parameter, the sample has the parameter named as Room Number

Returns `result` (_FamilyInstance_).
