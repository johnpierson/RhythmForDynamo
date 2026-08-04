## In Depth

`Documents.CopyElementsFromLinkedDocument(sourceDocument, sourceInstance, elements)`

This node will copy the given elements from the given linked document into the active document.

The inputs are:

- `sourceDocument` (_object_) — The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).
- `sourceInstance` (_Element_) — The instance of the link to copy from.
- `elements` (_list of Element_) — The elements to copy.

Returns `newElements` (_list of Element_) — The copied elements.

Found in the library under **Actions**.

Search terms: `copy`.
