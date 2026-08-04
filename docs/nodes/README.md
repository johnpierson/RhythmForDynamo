# Node help

Generated. Do not edit the `.md` files here by hand — the next run overwrites them.

```
dotnet run --project tools/Rhythm.Docs -- --assemblies deploy/2027 --out docs/nodes
```

One Markdown file per node, named the way Dynamo's documentation browser looks
it up. `scripts/pack-docs.ps1` copies the folder to `deploy/Rhythm/doc`, which is
where the browser reads it from inside an installed package.

Everything on a page comes from the shipped assemblies: signatures from their
metadata, prose from the XML documentation the compiler wrote beside them. To
improve a page, write the `<summary>`, `<param>` and `<returns>` comments above
the node in `src/` and regenerate.

## Example graphs and screenshots

Checked in beside the pages and picked up by file name — no list to maintain:

- `<node>.dyn` — an example graph. Dynamo's help panel offers to insert it.
- `<node>_img.png` — a screenshot, embedded at the bottom of the page.

There are 8 graphs and 45 screenshots.

## Coverage

Counted on the last run. A page with no summary and no described input says so
on its face rather than being padded out, so this number is the honest one.

| | Pages | Share |
|---|---:|---:|
| Total | 315 | 100% |
| With a summary | 297 | 94% |
| Taking at least one input | 297 | 94% |
| — every input described | 230 | 77% |
| — no input described | 62 | 21% |
| With the output described | 192 | 61% |
| With an example image | 45 | 14% |
| With an example graph | 8 | 3% |
| **Empty — signature only** | 18 | 6% |

The two input rows are shares of the nodes that take inputs, not of every node.

By assembly:

| Assembly | Pages | With a summary | Empty |
|---|---:|---:|---:|
| RhythmCore | 46 | 44 (96%) | 2 (4%) |
| RhythmRevit | 236 | 221 (94%) | 15 (6%) |
| RhythmUI | 33 | 32 (97%) | 1 (3%) |

## Graphs in the old format

Saved before Dynamo 2.0 replaced the XML graph format with JSON, so no supported
Dynamo can open them. Their pages offer no example graph until someone opens each
one in a recent Dynamo and saves it again.

- `Rhythm.Revit.Elements.ElevationMarker.CreateElevationByMarkerIndex.dyn`

## Overloads sharing a page

Dynamo names a node by its ports, so two overloads differing only in a
parameter's type are indistinguishable to it and to the browser. One page
serves both.

- `RhythmUI.Utilities.SelFilter.GetElementFilter(filterMethod)`
