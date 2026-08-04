## In Depth

`Batch.UpgradeFamilies(directoryPath, suffix: "")`

This tool will batch upgrade all the Revit families in a directory, and delete the backup files that this run generates. Backup files that already existed are left alone.

The inputs are:

- `directoryPath` (_string_) — The directory to read for ALL families. Including subdirectories.
- `suffix` (_string_, defaults to `""`) — Optional suffix to save the files as. Useful for read-only files.

The outputs are:

- `Successfully Upgraded` — _Not documented yet._
- `Not So Successfully Upgraded` — _Not documented yet._

Search terms: `Application.OpenDocumentFile`, `rhythm`.
