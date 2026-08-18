## In Depth

`Tools.ThreeDeeSpatialTags(runIt)`

Open the 3d Spatial Tags dialog, and report what it did.

This is the 3d Spatial Tags add-in as a single node: the same dialog, drawn the same way, doing the same work. Set runIt to true and it opens over Revit. Pick rooms or spaces, optionally a linked model to read them out of, the phase, the tag family type and a text height, then press Create / Update Tags. Close it and the node reports what the run did.

Re-running follows the model rather than piling up duplicates: a tag records the element it belongs to, and the link instance it was read through, in the family's SpatialElementId parameter. Rooms with a blank name or number are skipped, because there would be nothing to put in the tag, and so are unplaced ones, because there is nowhere to put it. Unbounded and redundant rooms still have a marker, so they are still tagged, and the dialog says how many. In a workshared model a tag owned by somebody else is reported, never overwritten. Tags whose room has been deleted are counted and left alone, because deleting elements out of somebody's model is not this node's business.

The dialog opens whenever this node evaluates with runIt true, so drive it from a Boolean toggle rather than from something that changes on every graph run.

The inputs are:

- `runIt` (_boolean_) — Set to true to open the dialog. False changes nothing and opens nothing.

The outputs are:

- `tags` — The tags the run created or updated.
- `created` — How many tags were placed.
- `updated` — How many existing tags were moved and rewritten.
- `skipped` — How many rooms or spaces could not be tagged.
- `orphanedTags` — How many tags in the model name a room that has been deleted.
- `message` — What the run did, in words.

Found in the library under **Actions**.

Search terms: `3d room tags`, `3d space tags`, `spatial tags`, `rhythm`.
