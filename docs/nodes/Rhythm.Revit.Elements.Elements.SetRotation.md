## In Depth

`Elements.SetRotation(element, angle, vector: Rhythm.Utilities.MiscUtils.GetNull())`

Rotate an element in Revit given the angle and an optional rotation vector.

The inputs are:

- `element` (_Element_) — The element to rotate
- `angle` (_number_) — How much to rotate?
- `vector` (_Vector_, defaults to `Rhythm.Utilities.MiscUtils.GetNull()`) — The vector to rotate about.

Returns `Element` (_Element_) — The rotated element
