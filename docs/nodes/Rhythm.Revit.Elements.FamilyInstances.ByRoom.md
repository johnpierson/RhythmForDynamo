## In Depth

`FamilyInstances.ByRoom(familyTemplatePath, room, materialName, category, subcategory: "")`

This node will create and place a generic model family instance at all the room locations given the room element. This will close all family documents so keep that in mind!

The inputs are:

- `familyTemplatePath` (_string_) — The family template to use.
- `room` (_Room_) — The room to convert to generic model.
- `materialName` (_string_) — The material to assign to the solid. *Note - The material has to exist in the family template to work. If it does not exist nothing will be assigned.
- `category` (_Category_) — The category to assign to the family. *Note - this needs to be a category that works for families. (Doors, Generic Models, etc.)
- `subcategory` (_string_, defaults to `""`) — The subcategory to assign to the solid. *Note - this needs to exist in the family template, if it does not, nothing will be changed in this regard.

Returns `familyInstance` (_Dictionary of string and object_) — The family instances that were placed.

Found in the library under **Create**.

Search terms: `space`, `rhythm`, `element.space`.
