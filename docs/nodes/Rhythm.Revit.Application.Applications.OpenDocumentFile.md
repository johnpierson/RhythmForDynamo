## In Depth

`Applications.OpenDocumentFile(filePath, audit: false, detachFromCentral: false, preserveWorksets: true, closeAllWorksets: false, suppressWarnings: false)`

This node will open the given file in the background.

The inputs are:

- `filePath` (_string_) — The file to obtain document from.
- `audit` (_boolean_, defaults to `false`) — Choose whether or not to audit the file upon opening. (Will run slower with this)
- `detachFromCentral` (_boolean_, defaults to `false`) — Choose whether or not to detach from central upon opening. Only for RVT files.
- `preserveWorksets` (_boolean_, defaults to `true`) — Choose whether or not to preserve worksets upon opening. Only for RVT files.
- `closeAllWorksets` (_boolean_, defaults to `false`) — Choose if you want to close all worksets upon opening. Defaulted to false.
- `suppressWarnings` (_boolean_, defaults to `false`) — Do you want to enable warning suppression? Caution, this is experimental and if something goes wrong you might have to restart Revit

Returns `document` (_Document_) — The document object. If the file path is blank this returns the current document.

Found in the library under **Create**.

Search terms: `Application.OpenDocumentFile`, `rhythm`.
