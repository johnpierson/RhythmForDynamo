# Rhythm

<img src="assets/rhythm-logo.png" alt="Rhythm for Dynamo" class="rh-logo">

A collection of custom nodes for [Dynamo](https://dynamobim.org/), aimed at keeping a Revit project
in good rhythm. Written and maintained by [John Pierson](https://designtechunraveled.com/), free and
open source under the BSD 3-Clause licence.

315 nodes across three libraries, and this site documents every one of them.

|  |  |
|---|---|
| **[Installing](installing.md)** | Through Dynamo's package manager, or by hand. |
| **[Node reference](node-reference.md)** | Every node, grouped by what it works on, one line each. |
| **[Node help](nodes/README.md)** | A page per node — the same help Dynamo shows in the panel beside your graph. |
| **[Writing node help](writing-node-help.md)** | How these pages are produced, and how to improve one. |

## What is in the package

| Library | What it holds |
|---|---|
| **Rhythm Core** | Geometry, text, numbers, and general helpers. No reliance on Revit. |
| **Rhythm Revit** | The Revit nodes: elements, views, sheets, worksharing, documents. |
| **Rhythm UI** | Dropdowns and selection nodes, which put a list from the model on the node itself. |

Rhythm also ships a view extension that annotates its own nodes and manages run mode around the
background-document nodes, and a set of [Python equivalents](../RhythmPython) for many of the
popular nodes.

## Help inside Dynamo

Every node's page is packaged with Rhythm and shown by Dynamo's own documentation browser. Right-click
a node and choose **Help**, and the panel beside the graph fills with the same text this site
publishes — including, for the nodes that have one, an example graph you can insert straight into the
canvas.

That help is generated from the source, so it cannot describe a port the node does not have.

## Supported versions

Revit 2020 and up, Dynamo 2.0.x and up. Rhythm is deployed from GitHub and downloads the right node
libraries for your Revit version when it first runs.

For Revit 2027 the package no longer ships the legacy `*.customization.dll` icon assemblies, because
.NET 10 disables the bitmap resource path they relied on. Everything else is unchanged.

## Disclaimer

Not affiliated with Autodesk. Provided in a personal capacity by the author, as is, without warranty
of any kind. See the [licence](../LICENSE).
