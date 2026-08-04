## In Depth

`Documents.SaveAs(document, filePath, previewViewId: -1)`

This node will save the Revit document to another path.

The inputs are:

- `document` (_object_) — A valid Revit Document.
- `filePath` (_string_) — The file path to save the document.
- `previewViewId` (_integer_, defaults to `-1`) — Optional - If you want to specify the preview view for the thumbnail.

Returns `result` (_string_) — A string message whether the save as was successful or a failure.

Found in the library under **Action**.
