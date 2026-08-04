## In Depth

`Windows.Direction(window)`

This will get the window's facing direction based on the FacingOrientation property. Windows in Revit are typically FamilyInstance elements with a FacingOrientation that indicates the direction they face.

The inputs are:

- `window` (_Element_) — The window to calculate facing from.

The outputs are:

- `facingDirection` — The estimated facing direction.
- `facingVector` — The facing vector.

Found in the library under **Query**.
