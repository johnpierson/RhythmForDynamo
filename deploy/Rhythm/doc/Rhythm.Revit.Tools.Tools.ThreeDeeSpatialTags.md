## In Depth

`Tools.ThreeDeeSpatialTags(runIt, target: "Rooms", phase: null, linkInstance: null, tagType: null, updateExisting: true, textHeightInches: 0)`

Place or update 3d tags on every room or space in the model, in one go.

This is the whole of the 3d Spatial Tags add-in as a single node. It reads the rooms or spaces out of this document or a link, works out which of them already have a tag, moves and rewrites those and creates the rest, and reports what it decided.

Re-running follows the model rather than piling up duplicates: a tag records the element it belongs to, and the link instance it was read through, in the family's SpatialElementId parameter. Rooms with a blank name or number are skipped, because there would be nothing to put in the tag, and so are unplaced ones, because there is nowhere to put it. Unbounded and redundant rooms still have a marker, so they are still tagged, and counted in the message so you know. In a workshared model a tag owned by somebody else is reported, never overwritten. Tags whose room has been deleted are counted and left alone, because deleting elements out of somebody's model is not this node's business.

The inputs are:

- `runIt` (_boolean_) — Set to true to write to the model. False reports nothing and changes nothing.
- `target` (_string_, defaults to `"Rooms"`) — What to tag: "Rooms" or "Spaces".
- `phase` (_Element_, defaults to `null`) — The phase to tag. Matched to the source document by name. Leave this empty to tag every phase.
- `linkInstance` (_Element_, defaults to `null`) — A Revit link to read the rooms or spaces out of. Leave this empty to use the current document. The tags are always placed in the current document, transformed into place.
- `tagType` (_FamilyType_, defaults to `null`) — The 3d tag family type to use. Leave this empty and the node uses a loaded 3dSpatialElementTag, loading the one from Rhythm's extra folder if the model has none. Whatever you give it has to carry Name, Number and SpatialElementId instance parameters.
- `updateExisting` (_boolean_, defaults to `true`) — Whether to move and rewrite the tags that are already placed. False places a fresh set on top of them every run.
- `textHeightInches` (_number_, defaults to `0`) — The text height to apply to the tag family type, in inches. Zero or less leaves the family's own height alone.

The outputs are:

- `tags` — The tags this run created or updated.
- `created` — How many tags were placed.
- `updated` — How many existing tags were moved and rewritten.
- `skipped` — How many rooms or spaces could not be tagged.
- `orphanedTags` — How many tags in the model name a room that has been deleted.
- `message` — What the run did, in words.

Found in the library under **Actions**.

Search terms: `3d room tags`, `3d space tags`, `spatial tags`, `rhythm`.
