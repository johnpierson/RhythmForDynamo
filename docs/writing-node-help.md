# Writing node help

Every page under [Node help](nodes/README.md), and the whole of the
[node reference](node-reference.md), is generated. Editing one by hand is wasted work — the next
build overwrites it.

What you edit instead is the documentation comment above the node, in `src/`. That comment is the
single source: Dynamo reads it for the tooltip on a port, and the generator reads it for the page.
The two cannot disagree, which is the point.

## Improving a page

Find the node in [`src/RhythmRevit`](../src/RhythmRevit), [`src/RhythmCore`](../src/RhythmCore) or
[`src/RhythmUI`](../src/RhythmUI), and write the comment:

```csharp
/// <summary>
/// This node will convert the given elements to parts.
/// </summary>
/// <param name="element">The element to convert to parts.</param>
/// <returns name="Parts">The created parts from the given element.</returns>
/// <search>
/// Element.CreateParts, demolish
/// </search>
[NodeCategory("Actions")]
public static List<Element> CreateParts(Element element)
```

Each tag lands somewhere specific:

| Tag | Where it shows |
|---|---|
| `<summary>` | The paragraph under the signature, and the row in the node reference. |
| `<param>` | The line for that input. |
| `<returns name="…">` | The output's name and description. One tag per output on a `[MultiReturn]` node. |
| `<search>` | The search terms listed at the bottom of the page. |
| `[NodeCategory]` | Where the node sits in the library, quoted on the page. |

Then regenerate:

```powershell
./scripts/generate-docs.ps1
./scripts/pack-docs.ps1
```

The first writes `docs/nodes` and `docs/node-reference.md`; the second copies the help into
`deploy/Rhythm/doc`, which is the folder that ships. The build fails a pull request where those have
drifted from the source, so run both before pushing.

## Nothing is invented

A node with no `<summary>` gets a page that says *"This node has no description yet."* An input with
no `<param>` says *"Not documented yet."* on its own line.

That is deliberate. A page padded out with plausible text reads as finished and never gets written;
one that says it is empty is a job somebody can pick up. `docs/nodes/README.md` counts them on every
run — at the last count, 18 of 315 pages carry nothing but a signature, and it lists all 18 by name.

## Example graphs and screenshots

A page picks these up by file name, with no list to maintain. Drop them in `docs/nodes` beside the
generated pages:

- `<node>.dyn` — an example graph. Dynamo's help panel offers to insert it into the canvas, and this
  site offers it as a download.
- `<node>_img.png` — a screenshot, embedded at the bottom of the page. `.gif` works too, and several
  of the existing ones are animations.

`<node>` is the full name of the page, so a graph for `Rhythm.Revit.Elements.Elements.CreateParts`
is `Rhythm.Revit.Elements.Elements.CreateParts.dyn`. An asset whose name matches no node is reported
as an orphan on every run rather than silently shipped.

Save graphs from a current Dynamo. One of the existing examples predates Dynamo 2.0 and is still in
the old XML format, which no supported Dynamo can open; it is left out of the package and named in
`docs/nodes/README.md` until somebody re-saves it.

## How the generator works

[`tools/Rhythm.Docs`](../tools/Rhythm.Docs) reads the assemblies in `deploy/` — their metadata for
the signatures, the `.xml` files beside them for the prose — and writes one Markdown file per node,
named the way Dynamo's documentation browser looks it up.

It reads the metadata tables rather than loading the assemblies, so it needs no Revit and no Dynamo
installed. That is also how the Rhythm UI nodes are covered: they are `NodeModel` classes rather
than zero-touch methods, and are found by the `[NodeName]` attribute they carry.
