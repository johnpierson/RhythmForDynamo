using System.Text;

namespace Rhythm.Docs;

/// <summary>
/// Writes the one page that shows the whole library at once.
///
/// The per-node help in docs/nodes answers "what does this node do" once you already have the
/// node. This answers the question that comes first — "is there a node for this" — which 315
/// separate pages cannot, and which is the reason the site exists at all rather than the package
/// simply shipping its doc folder.
///
/// Generated from the same reading of the assemblies as the help pages, so the two cannot
/// disagree about what exists.
/// </summary>
internal static class NodeReference
{
    /// <summary>
    /// The top-level groups, in the order a reader would look for them rather than alphabetically:
    /// the Revit nodes are why almost everybody installs this.
    /// </summary>
    private static readonly (string Prefix, string Title, string Blurb)[] Sections =
    {
        ("Rhythm.Revit.", "Revit", "Nodes that read and change a Revit model."),
        ("RhythmUI.", "Revit UI", "Dropdowns and selection nodes, which put a Revit list on the node itself."),
        ("Rhythm.", "Core", "Geometry, text, numbers and helpers, with no reliance on Revit."),
    };

    internal static string Compose(
        IEnumerable<Node> nodes, XmlDocs docs, IReadOnlyDictionary<Node, string> fileNames)
    {
        List<Node> ordered = nodes
            .OrderBy(node => node.DeclaringType, StringComparer.Ordinal)
            .ThenBy(node => node.Member, StringComparer.Ordinal)
            .ToList();

        StringBuilder page = new();

        page.AppendLine("# Node reference");
        page.AppendLine();
        page.Append("Every node in the package — ").Append(ordered.Count)
            .AppendLine(" of them — with a line on what each one does.");
        page.AppendLine();
        page.AppendLine("Each name links to that node's own page, which is the same help Dynamo shows in");
        page.AppendLine("the panel beside the graph. Both are generated from the source, so what is written");
        page.AppendLine("here is what the node's own documentation comment says. Where a node has no");
        page.AppendLine("description the row says so rather than guessing.");
        page.AppendLine();

        // Assigned per section so a class that appears under one prefix is not repeated under a
        // shorter one. "Rhythm." matches everything, so it has to be tried last, which is the
        // order Sections is written in.
        Dictionary<string, List<Node>> byType = new(StringComparer.Ordinal);

        foreach (Node node in ordered)
        {
            if (!byType.TryGetValue(node.DeclaringType, out List<Node>? group))
            {
                byType[node.DeclaringType] = group = new List<Node>();
            }

            group.Add(node);
        }

        HashSet<string> placed = new(StringComparer.Ordinal);
        IReadOnlyDictionary<string, string> headings = Headings(byType.Keys);

        foreach ((string prefix, string title, string blurb) in Sections)
        {
            string[] types = byType.Keys
                .Where(type => type.StartsWith(prefix, StringComparison.Ordinal) && !placed.Contains(type))
                .OrderBy(type => type, StringComparer.Ordinal)
                .ToArray();

            if (types.Length == 0)
            {
                continue;
            }

            foreach (string type in types)
            {
                placed.Add(type);
            }

            page.AppendLine();
            page.Append("## ").AppendLine(title);
            page.AppendLine();
            page.AppendLine(blurb);

            foreach (string type in types)
            {
                page.AppendLine();
                page.Append("### ").AppendLine(headings[type]);
                page.AppendLine();

                if (docs.ForType(type) is { Length: > 0 } family && !IsBoilerplate(family))
                {
                    page.AppendLine(family);
                    page.AppendLine();
                }

                page.AppendLine("| Node | What it does |");
                page.AppendLine("|---|---|");

                foreach (Node node in byType[type])
                {
                    string file = fileNames[node];
                    string label = node.Kind == NodeKind.NodeModel
                        ? node.DisplayName ?? node.ShortName
                        : node.Member;

                    page.Append("| [`").Append(label).Append("`](nodes/").Append(Uri.EscapeDataString(file))
                        .Append(".md) | ").Append(Sentence(node, docs)).AppendLine(" |");
                }
            }
        }

        return page.ToString();
    }

    /// <summary>
    /// A heading per class: the class name alone where that is unambiguous, and enough of the
    /// namespace to tell it apart where it is not.
    ///
    /// Uniqueness is across the whole page rather than within a section, because the headings also
    /// become the anchors in the table of contents, and two rows reading "Helpers" leave a reader
    /// picking between them at random. There genuinely are two of those, two Elements and two
    /// System, in different corners of the library.
    /// </summary>
    private static IReadOnlyDictionary<string, string> Headings(IEnumerable<string> types)
    {
        string[] all = types.ToArray();

        Dictionary<string, string> headings = new(StringComparer.Ordinal);
        Dictionary<string, int> counts = new(StringComparer.OrdinalIgnoreCase);

        foreach (string type in all)
        {
            string shortName = Collapse(type);
            counts[shortName] = counts.GetValueOrDefault(shortName) + 1;
        }

        foreach (string type in all)
        {
            string shortName = Collapse(type);

            // Drop only the leading "Rhythm." the section heading already implies; everything
            // after it is what distinguishes one Helpers from the other.
            headings[type] = counts[shortName] == 1
                ? shortName
                : type.StartsWith("Rhythm.", StringComparison.Ordinal) ? type["Rhythm.".Length..] : type;
        }

        return headings;
    }

    /// <summary>
    /// Rhythm's wrapper classes are usually namespaced under a folder of the same name —
    /// <c>Elements.Elements</c>, <c>Views.View</c> — and saying it twice reads as a mistake.
    /// </summary>
    private static string Collapse(string type)
    {
        string[] parts = type.Split('.');
        return parts[^1];
    }

    /// <summary>
    /// One sentence, because a table row is not the place for four. The rest is on the node's own
    /// page, which the name links to.
    /// </summary>
    private static string Sentence(Node node, XmlDocs docs)
    {
        MemberDoc doc = docs.For(node);

        string summary = node.Kind == NodeKind.NodeModel && doc.Summary.Length == 0
            ? node.Description ?? string.Empty
            : doc.Summary;

        summary = summary.Trim();

        if (summary.Length == 0)
        {
            return "_Not documented yet._";
        }

        // Cut at the first full stop that ends a word rather than an abbreviation or a decimal.
        for (int i = 0; i < summary.Length - 1; i++)
        {
            if (summary[i] == '.' && char.IsWhiteSpace(summary[i + 1]) &&
                (i < 2 || summary[i - 2] != ' '))
            {
                summary = summary[..(i + 1)];
                break;
            }
        }

        // A newline or a pipe would end the table row early.
        return summary.Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal);
    }

    private static bool IsBoilerplate(string summary)
        => summary.StartsWith("Wrapper class for", StringComparison.OrdinalIgnoreCase) ||
           summary.StartsWith("Wrapper for", StringComparison.OrdinalIgnoreCase);
}
