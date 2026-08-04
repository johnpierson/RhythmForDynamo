## In Depth

`Group.ByElementsAndOrigin(elements, name: Rhythm.Utilities.MiscUtils.GetNull(), origin: Rhythm.Utilities.MiscUtils.GetNull())`

This node is a pretty neat group creator, that allows for you to pick an origin at creation time.

The inputs are:

- `elements` (_list of Element_) — The elements to group
- `name` (_string_, defaults to `Rhythm.Utilities.MiscUtils.GetNull()`) — Optional Name
- `origin` (_Point_, defaults to `Rhythm.Utilities.MiscUtils.GetNull()`) — Optional origin. (Note: This node will fix whatever Z Value you input to match the group's Z value)

Returns `newGroup` (_Element_) — The new group
