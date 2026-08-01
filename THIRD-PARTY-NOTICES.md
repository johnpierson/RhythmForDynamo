# Third-Party Notices

Rhythm for Dynamo is distributed under the BSD 3-Clause License (see [LICENSE](LICENSE)).
That license covers Rhythm's own code. It does **not** cover the third-party code
listed below, which is incorporated into this repository and, in most cases,
compiled into the shipped assemblies. Each item remains under its own license and
its own copyright holder.

This file is informational and does not modify any of the licenses referenced.

Last reviewed: 2026-08-01

---

## Contents

1. [Nuclex Framework — rectangle packing](#1-nuclex-framework--rectangle-packing) — **IBM Common Public License v1.0**
2. [ShortestWalk](#2-shortestwalk) — MIT
3. [Revit MEP API sample forms](#3-revit-mep-api-sample-forms) — Autodesk sample terms
4. [Point-in-polygon test](#4-point-in-polygon-test) — permissive, notice must be preserved
5. [Gaussian-Random](#5-gaussian-random) — MIT
6. [polylabel-csharp](#6-polylabel-csharp) — **no license stated upstream**
7. [SpringNodes](#7-springnodes) — MIT
8. [Community-contributed snippets](#8-community-contributed-snippets)
9. [LunchboxML](#9-lunchboxml-not-shipped) — LGPL, **not shipped**
10. [Redistributed binaries](#10-redistributed-binaries) — MIT
11. [Build-time dependencies](#11-build-time-dependencies-not-redistributed)

---

## 1. Nuclex Framework — rectangle packing

**Copyright (C) 2002-2011 Nuclex Development Labs**
**License: IBM Common Public License v1.0 (CPL-1.0)**

> ⚠️ **This is the most significant item in this file.** The CPL is a weak-copyleft
> license, not a permissive one. These files are compiled into the shipped
> `RhythmCore.dll` and `RhythmRevit.dll`. They are **not** BSD 3-Clause and are not
> covered by Rhythm's LICENSE file. Obligations under CPL-1.0 attach to these files
> and to modifications of them.

Files (six; the set is duplicated across both projects):

- `src/RhythmCore/GenerativeDesign/CygonRectanglePacker.cs`
- `src/RhythmCore/GenerativeDesign/RectanglePacker.cs`
- `src/RhythmCore/GenerativeDesign/OutOfSpaceException.cs`
- `src/RhythmRevit/Revit/Views/CygonRectanglePacker.cs`
- `src/RhythmRevit/Revit/Views/RectanglePacker.cs`
- `src/RhythmRevit/Revit/Views/OutOfSpaceException.cs`

Each file carries its original `#region CPL License` header, which reads in part:

```
Nuclex Framework
Copyright (C) 2002-2011 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.
```

The code has been adapted for Dynamo geometry types (the original XNA
`Microsoft.Xna.Framework` dependency is commented out), but the namespace
`Nuclex.Game.Packing` and the license headers are intact.

Full license text: <https://opensource.org/license/cpl1-0-txt>

---

## 2. ShortestWalk

**Copyright (c) 2011 McNeel Europe. All Rights Reserved.**
**License: MIT** (see [MIT License text](#mit-license-text) below)

A port of the RhinoCommon/Grasshopper *ShortestWalk* component, originally written
by Giulio Piacentino of Robert McNeel & Associates, adapted here to Dynamo's
`Autodesk.DesignScript.Geometry` types. Reached Rhythm by way of Proving Ground's
open-sourced LunchBox package.

Files (nine, approximately 1,030 lines):

- `src/RhythmRevit/ShortestWalk/Geometry/AStar.cs`
- `src/RhythmRevit/ShortestWalk/Geometry/CurvesTopology.cs`
- `src/RhythmRevit/ShortestWalk/Geometry/Dijkstra.cs`
- `src/RhythmRevit/ShortestWalk/Geometry/EdgeAddress.cs`
- `src/RhythmRevit/ShortestWalk/Geometry/ListByPattern.cs`
- `src/RhythmRevit/ShortestWalk/Geometry/NodeAddress.cs`
- `src/RhythmRevit/ShortestWalk/Geometry/PathMethod.cs`
- `src/RhythmRevit/ShortestWalk/Geometry/SearchMode.cs`
- `src/RhythmRevit/Utilities/ShortestWalkUtils.cs` (Dynamo adapter over the above)

Exposed to users through `Rhythm.Geometry.Geometry.LunchboxShortestWalk`.

> **Note:** none of these files currently carries a copyright header, even though the
> upstream MIT terms require that "the above copyright notice and this permission
> notice shall be included in all copies or substantial portions of the Software."
> The notice below satisfies that requirement at the distribution level; adding
> per-file headers would be an improvement.

Upstream: <https://github.com/shortestwalk-demo/ShortestWalk>

---

## 3. Revit MEP API sample forms

**Copyright (C) 2007-2010 by Jeremy Tammik, Autodesk, Inc.**
**License: Autodesk sample-code terms (permissive, notice must be preserved)**

Files:

- `src/RhythmRevit/Forms/DefaultProgressForm.cs`
- `src/RhythmRevit/Forms/FamilyUpgradeForm.cs`

Both retain their original `#region Header` block, which grants permission to
"use, copy, modify, and distribute this software for any purpose and without fee
[...] provided that the above copyright notice appears in all copies", and includes
Autodesk's warranty disclaimer and U.S. Government restricted-rights notice.

---

## 4. Point-in-polygon test

**Copyright (C) 2009 by Jeremy Tammik. All rights reserved.**
**License: permissive — "This code may be freely used. Please preserve this comment."**

File: `src/RhythmRevit/Utilities/PointInPoly.cs` (method `PolygonContains`)

Per its header, written by Jeremy Tammik (Autodesk, 2009-09-23), based on his own
1996 C++ code, which in turn derived from C code in the article *"An Incremental
Angle Point in Polygon Test"* by Kevin Weiler, Autodesk, in **Graphics Gems IV**
(Academic Press, 1994).

---

## 5. Gaussian-Random

**Copyright (c) Marco Fazio Random**
**License: MIT** (see [MIT License text](#mit-license-text) below)

File: `src/RhythmCore/GenerativeDesign/GenerativeDesign.cs` — the
`RandomDistribution` class (Marsaglia polar-method Gaussian sampling).

Attribution is recorded inline on the class. Upstream:
<https://github.com/MarcoFazioRandom/Gaussian-Random>

---

## 6. polylabel-csharp

**Author: eqmiller**
**License: ⚠️ not stated upstream**

Files:

- `src/RhythmCore/Polylabel/Cell.cs`
- `src/RhythmCore/Polylabel/Polylabel.cs`

A C# port of Mapbox's [polylabel](https://github.com/mapbox/polylabel) (pole-of-
inaccessibility) algorithm; the upstream JavaScript/C++ original is ISC-licensed.
Used by `Rhythm.Geometry.Polygon`.

> ⚠️ **Flagged:** the upstream repository <https://github.com/eqmiller/polylabel-csharp>
> publishes **no LICENSE file and no license statement**. Absent an express grant,
> the default position is that no redistribution rights were given. Options worth
> considering: ask the author to add a license, re-derive the ~220 lines directly
> from Mapbox's ISC-licensed original, or replace the implementation.

---

## 7. SpringNodes

**Copyright (c) Dimitar Venkov**
**License: MIT** (see [MIT License text](#mit-license-text) below)

`Rhythm.Revit.Elements.FamilyInstance.ByGeometry` in
`src/RhythmRevit/Revit/Elements/FamilyInstance.cs` is a C# reimplementation of the
approach in SpringNodes' `FamilyInstance.ByGeometry.py`. Attribution is recorded
inline. Upstream: <https://github.com/dimven/SpringNodes>

---

## 8. Community-contributed snippets

Code adapted from publicly posted community contributions. Attribution is recorded
inline in each file; no formal license was attached at the source.

- **`src/RhythmRevit/Utilities/Converters.cs`** — Dynamo/Revit type converters
  contributed by **@erfajo** on the Dynamo forum.
  <https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033>
- **`src/RhythmRevit/Revit/Elements/FilledRegion.cs`** — multi-loop filled-region
  creation, based on a Dynamo forum post.
  <https://forum.dynamobim.com/t/filled-region-with-hole-in-the-middle-like-a-donut/22838/3>

---

## 9. LunchboxML (not shipped)

**License: GNU Lesser General Public License (LGPL)**

File: `src/RhythmCore/ML/ML.cs`

> **Not shipped.** This file is **100% commented out** — every line, including the
> `using` directives — and contributes no code to any built assembly. It is retained
> only as a reference to a Gaussian Mixture classification node made possible by
> LunchboxML (<https://bitbucket.org/archinate/lunchboxml/src/master/>), which is
> LGPL-licensed.
>
> Listed here for completeness. If this node is ever revived, the LGPL implications
> must be addressed first, since LGPL is incompatible with shipping the code as
> BSD 3-Clause.

---

## 10. Redistributed binaries

These third-party assemblies are shipped alongside Rhythm in the `deploy/` folder.
Both are MIT-licensed (see [MIT License text](#mit-license-text) below).

| Component | Version | Copyright | Project |
| --- | --- | --- | --- |
| Humanizer | 2.14.1 | Copyright (c) .NET Foundation and Contributors | <https://github.com/Humanizr/Humanizer> |
| Markov | 2.0.0 | Copyright (c) 2018 John Gietzen and Contributors | <https://github.com/otac0n/markov> |

---

## 11. Build-time dependencies (not redistributed)

Referenced at build time and **not** redistributed by this repository. Each remains
under its own license, obtained from its own publisher:

Dynamo (`DynamoVisualProgramming.*`), Revit API
(`Nice3point.Revit.Api.*`, `Revit_All_Main_Versions_API_x64`), `Newtonsoft.Json`,
`Prism`, `CommonServiceLocator`, `OptimizedPriorityQueue`, `NUnit`, and the
`System.*` / `Microsoft.CSharp` packages.

---

## MIT License Text

The following is the standard MIT License text referenced by items 2, 5, 7 and 10
above. Substitute the copyright line given for each component.

```
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## Reporting

If you are a copyright holder listed here and something is attributed
incorrectly — or if you spot third-party code in this repository that is missing
from this file — please open an issue at
<https://github.com/johnpierson/RhythmForDynamo/issues>.
