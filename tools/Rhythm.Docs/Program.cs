using System.Text;
using Rhythm.Docs;

// Generates Dynamo node help from the assemblies in deploy/ and writes it to docs/nodes.
//
//   dotnet run --project tools/Rhythm.Docs -- --assemblies deploy/2027 --out docs/nodes
//
// See docs/nodes/README.md, which this writes, for what ends up in the folder and why.

string assemblies = "deploy/2027";
string output = "docs/nodes";
string? reference = null;
bool listEmpty = false;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--assemblies" when i + 1 < args.Length:
            assemblies = args[++i];
            break;

        case "--out" when i + 1 < args.Length:
            output = args[++i];
            break;

        case "--reference" when i + 1 < args.Length:
            reference = args[++i];
            break;

        case "--list-undocumented":
            listEmpty = true;
            break;

        case "--help" or "-h":
            Console.WriteLine(
                "usage: rhythm-docs [--assemblies <folder>] [--out <folder>] [--reference <file>] " +
                "[--list-undocumented]");
            return 0;

        default:
            Console.Error.WriteLine($"Unknown argument: {args[i]}");
            return 2;
    }
}

string[] libraries = { "RhythmCore", "RhythmRevit", "RhythmUI" };

foreach (string library in libraries)
{
    string path = Path.Combine(assemblies, library + ".dll");

    if (!File.Exists(path))
    {
        Console.Error.WriteLine(
            $"{path} is not there. Node help is generated from the assemblies in deploy/, so point " +
            "--assemblies at a folder holding all three of RhythmCore.dll, RhythmRevit.dll and RhythmUI.dll.");
        return 1;
    }
}

XmlDocs docs = XmlDocs.Load(libraries.Select(library => Path.Combine(assemblies, library + ".xml")));

List<Node> nodes = libraries
    .SelectMany(library => NodeReader.Read(Path.Combine(assemblies, library + ".dll")))
    .ToList();

Directory.CreateDirectory(output);

Examples examples = Examples.In(output);
IReadOnlySet<string> overloaded = NodeDocs.Overloaded(nodes);

Coverage coverage = new();
HashSet<string> written = new(StringComparer.OrdinalIgnoreCase);
List<string> ambiguous = new();
Dictionary<Node, string> documented = new();

foreach (Node node in nodes.OrderBy(node => node.QualifiedName, StringComparer.Ordinal)
    .ThenBy(node => string.Join(",", node.Parameters.Select(p => p.Type.DocId())), StringComparer.Ordinal))
{
    string fileName = NodeDocs.FileNameOf(node, overloaded);

    // Overloads differing only in a parameter's type — Func<Element, bool> against
    // Predicate<Element> — produce the same port names, so Dynamo names them identically too and
    // shows one page for both. Writing the first and saying so is the honest outcome; there is no
    // second name for the second node to be filed under.
    if (!written.Add(fileName))
    {
        ambiguous.Add(fileName);
        continue;
    }

    Write(Path.Combine(output, fileName + ".md"), NodeDocs.Compose(node, docs, examples, fileName));

    coverage.Add(node, docs.For(node), fileName, examples);
    documented[node] = fileName;
}

// A node withdrawn from the library stops being generated, but its file would sit here forever,
// shipping help for something nobody can place. Writing files only ever adds, so this sweep is
// what makes regenerating the folder mean what it says. Scoped to Markdown, so the checked-in
// example graphs and screenshots survive it.
int removed = 0;

foreach (string stale in Directory.EnumerateFiles(output, "*.md")
    .Where(path => !written.Contains(Path.GetFileNameWithoutExtension(path)) &&
                   !Path.GetFileName(path).Equals("README.md", StringComparison.OrdinalIgnoreCase)))
{
    File.Delete(stale);
    removed++;
}

Write(Path.Combine(output, "README.md"), Readme(coverage, examples, written, ambiguous));

// The site's one page that shows the whole library. Written only when asked for, because the
// package's doc/ folder has no use for it — Dynamo's browser would file it as a node called
// "node-reference".
if (reference is not null)
{
    string? folder = Path.GetDirectoryName(Path.GetFullPath(reference));

    if (folder is not null)
    {
        Directory.CreateDirectory(folder);
    }

    Write(reference, NodeReference.Compose(documented.Keys, docs, documented));
    Console.WriteLine($"wrote the node reference to {reference}");
}

Console.WriteLine($"wrote {written.Count} node help files to {output}" +
    (removed > 0 ? $" ({removed} stale removed)" : string.Empty));
Console.WriteLine(coverage.Summary());

string[] orphans = examples.Orphans(written).ToArray();

if (orphans.Length > 0)
{
    Console.WriteLine();
    Console.WriteLine($"{orphans.Length} example asset(s) match no node and illustrate nothing:");

    foreach (string orphan in orphans)
    {
        Console.WriteLine("  " + orphan);
    }
}

if (examples.LegacyGraphs.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine($"{examples.LegacyGraphs.Count} example graph(s) are in the pre-2.0 XML format and " +
        "no supported Dynamo can open them. Their pages offer no graph until they are re-saved:");

    foreach (string graph in examples.LegacyGraphs)
    {
        Console.WriteLine("  " + graph);
    }
}

if (listEmpty)
{
    Console.WriteLine();
    Console.WriteLine("Nodes whose page carries nothing but a signature:");

    foreach (Node node in coverage.Empty)
    {
        Console.WriteLine("  " + node.QualifiedName);
    }
}

if (ambiguous.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("Overloads Dynamo cannot tell apart, sharing one page:");

    foreach (string name in Distinct(ambiguous))
    {
        Console.WriteLine("  " + name);
    }
}

return 0;

static IEnumerable<string> Distinct(IEnumerable<string> names)
    => names.Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal);

/// <summary>
/// Line endings and byte-order mark pinned rather than left to the platform.
///
/// The build fails a pull request whose generated help differs from what is checked in, so a file
/// that comes out with CRLF on one machine and LF on another fails that check for no reason at
/// all. .gitattributes keeps the working copy on LF to match; see the entry for docs/nodes.
/// </summary>
static void Write(string path, string text)
    => File.WriteAllText(
        path,
        text.Replace("\r\n", "\n", StringComparison.Ordinal),
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

static string Readme(Coverage coverage, Examples examples, IReadOnlySet<string> written, IReadOnlyList<string> ambiguous)
{
    StringBuilder text = new();

    text.AppendLine("# Node help");
    text.AppendLine();
    text.AppendLine("Generated. Do not edit the `.md` files here by hand — the next run overwrites them.");
    text.AppendLine();
    text.AppendLine("```powershell");
    text.AppendLine("./scripts/generate-docs.ps1");
    text.AppendLine("./scripts/pack-docs.ps1");
    text.AppendLine("```");
    text.AppendLine();
    text.AppendLine("One Markdown file per node, named the way Dynamo's documentation browser looks");
    text.AppendLine("it up. The second script copies the folder to `deploy/Rhythm/doc`, which is where");
    text.AppendLine("the browser reads it from inside an installed package.");
    text.AppendLine();
    text.AppendLine("Everything on a page comes from the shipped assemblies: signatures from their");
    text.AppendLine("metadata, prose from the XML documentation the compiler wrote beside them. To");
    text.AppendLine("improve a page, write the `<summary>`, `<param>` and `<returns>` comments above");
    text.AppendLine("the node in `src/` and regenerate.");
    text.AppendLine();
    text.AppendLine("## Example graphs and screenshots");
    text.AppendLine();
    text.AppendLine("Checked in beside the pages and picked up by file name — no list to maintain:");
    text.AppendLine();
    text.AppendLine("- `<node>.dyn` — an example graph. Dynamo's help panel offers to insert it.");
    text.AppendLine("- `<node>_img.png` — a screenshot, embedded at the bottom of the page.");
    text.AppendLine();
    text.AppendLine($"There are {examples.GraphCount} graphs and {examples.ImageCount} screenshots.");
    text.AppendLine();
    text.AppendLine("## Coverage");
    text.AppendLine();
    text.AppendLine("Counted on the last run. A page with no summary and no described input says so");
    text.AppendLine("on its face rather than being padded out, so this number is the honest one.");
    text.AppendLine();
    text.Append(coverage.Report());

    string[] orphans = examples.Orphans(written).ToArray();

    if (orphans.Length > 0)
    {
        text.AppendLine();
        text.AppendLine("## Orphaned assets");
        text.AppendLine();
        text.AppendLine("These match no node — usually one that was renamed or withdrawn. They ship in");
        text.AppendLine("every package illustrating nothing.");
        text.AppendLine();

        foreach (string orphan in orphans)
        {
            text.AppendLine("- `" + orphan + "`");
        }
    }

    if (examples.LegacyGraphs.Count > 0)
    {
        text.AppendLine();
        text.AppendLine("## Graphs in the old format");
        text.AppendLine();
        text.AppendLine("Saved before Dynamo 2.0 replaced the XML graph format with JSON, so no supported");
        text.AppendLine("Dynamo can open them. Their pages offer no example graph until someone opens each");
        text.AppendLine("one in a recent Dynamo and saves it again.");
        text.AppendLine();

        foreach (string graph in examples.LegacyGraphs)
        {
            text.AppendLine("- `" + graph + "`");
        }
    }

    if (ambiguous.Count > 0)
    {
        text.AppendLine();
        text.AppendLine("## Overloads sharing a page");
        text.AppendLine();
        text.AppendLine("Dynamo names a node by its ports, so two overloads differing only in a");
        text.AppendLine("parameter's type are indistinguishable to it and to the browser. One page");
        text.AppendLine("serves both.");
        text.AppendLine();

        foreach (string name in Distinct(ambiguous))
        {
            text.AppendLine("- `" + name + "`");
        }
    }

    return text.ToString();
}
