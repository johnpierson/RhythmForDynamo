## In Depth

`Applications.UpgradeFile(filePath, audit: false, detachFromCentral: false, preserveWorksets: true, closeAllWorksets: false, unloadAllLinks: false)`

This will try to open a file in the current version with various options.

The inputs are:

- `filePath` (_string_) — The file to obtain document from.
- `audit` (_boolean_, defaults to `false`) — Choose whether or not to audit the file upon opening. (Will run slower with this)
- `detachFromCentral` (_boolean_, defaults to `false`) — Choose whether or not to detach from central upon opening. Only for RVT files.
- `preserveWorksets` (_boolean_, defaults to `true`) — Choose whether or not to preserve worksets upon opening. Only for RVT files.
- `closeAllWorksets` (_boolean_, defaults to `false`) — Choose if you want to close all worksets upon opening. Defaulted to false.
- `unloadAllLinks` (_boolean_, defaults to `false`) — Choose if you want unload all links?

Returns `result` (_string_) — Did it work?
