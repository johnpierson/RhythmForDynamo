# Installing

## Through Dynamo's package manager

The way almost everybody should install it.

1. Open Dynamo in Revit.
2. **Packages → Search for a Package…**
3. Search for **Rhythm** and install the latest version.
4. Restart Dynamo.

The restart matters. Rhythm ships a small extension that works out which Revit you are running and
unpacks the matching node libraries the first time it loads; the nodes appear in the library once
that has happened.

## By hand

A Dynamo package is a folder, not an archive, so a manual install is a copy:

1. Download the package folder from the [releases page](https://github.com/johnpierson/RhythmForDynamo/releases).
2. Copy the `Rhythm` folder into your Dynamo packages directory, which is usually
   `%AppData%\Dynamo\Dynamo Revit\<version>\packages`.
3. Restart Dynamo.

The folder holds `pkg.json` beside `bin`, `extra` and `doc`. `doc` is the node help — the folder
Dynamo's documentation browser reads — and a package missing it shows *"no documentation available"*
on every node.

## Checking it worked

Place any Rhythm node, right-click it and choose **Help**. The panel that opens should describe the
node, name its inputs, and — for the nodes that have one — offer an example graph to insert.

If the panel says there is no documentation, the `doc` folder did not make it into the package
directory.

## Which Revit and Dynamo

Revit 2020 and up; Dynamo 2.0.x and up. One package covers all of them: the extension downloads the
libraries built against your version rather than shipping eight copies.

Revit 2027 is supported, with one difference — the legacy `*.customization.dll` icon assemblies are
no longer shipped for it, because .NET 10 disables the bitmap resource loading path they used. Nodes
show Dynamo's default icon there; everything else works the same.
