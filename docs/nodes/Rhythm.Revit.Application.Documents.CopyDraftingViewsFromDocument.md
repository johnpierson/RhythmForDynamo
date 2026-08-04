## In Depth

`Documents.CopyDraftingViewsFromDocument(sourceDocument, draftingViews)`

This node will copy the given drafting views and their contents from the given document into the active document.

The inputs are:

- `sourceDocument` (_object_) — The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).
- `draftingViews` (_list of Element_) — The drafting views to copy.

Returns `newDraftingViews` (_list of Element_) — The copied drafting views with their elements.

Found in the library under **Actions**.

Search terms: `copy`.
