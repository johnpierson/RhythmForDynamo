using System.Globalization;
using System.Text;

namespace Rhythm.Docs;

/// <summary>
/// Writes one Dynamo node help file per Rhythm node.
///
/// Dynamo's documentation browser looks in a package's <c>doc/</c> folder for a Markdown file named
/// after the node — <c>Rhythm.Revit.Elements.Elements.CreateParts.md</c> — and renders it in the
/// panel beside the graph. The layout followed here is Dynamo's own, taken from the fallback docs
/// that ship with Dynamo Core: an <c>## In Depth</c> heading, prose that names the node, a bullet
/// per input, and an <c>## Example File</c> section carrying an image and an example graph.
///
/// Everything is read from the shipped assemblies: the signatures from the metadata tables, the
/// prose from the XML documentation the compiler emitted beside them. Nothing is written by hand,
/// so a node that gains a port gains a documented port. Help that disagrees with the node it
/// documents is worse than no help at all — the reader believes it.
///
/// Where the source carries no summary, the page says so rather than inventing prose. A page that
/// reads "not documented yet" is a to-do list item; a page padded out with filler reads as
/// finished and never gets fixed.
/// </summary>
internal static class NodeDocs
{
    /// <summary>
    /// Overloads share a method name, so the file name has to carry the parameters as well.
    /// Dynamo builds the same string in <c>DynamoViewModel.GetMinimumQualifiedName</c> — port
    /// names joined with a comma and a space, in brackets — and looks the file up by it.
    /// </summary>
    private const string OverloadFormat = "{0}({1})";

    internal const string Missing = "_This node has no description yet._";

    internal static string FileNameOf(Node node, IReadOnlySet<string> overloaded)
    {
        if (!overloaded.Contains(node.QualifiedName))
        {
            return node.QualifiedName;
        }

        string parameters = string.Join(", ", node.Parameters.Select(parameter => parameter.Name));
        return string.Format(CultureInfo.InvariantCulture, OverloadFormat, node.QualifiedName, parameters);
    }

    /// <summary>
    /// The node names Dynamo will qualify with their parameters because more than one node shares
    /// the name. Worked out per declaring type, because two classes may legitimately both have a
    /// <c>Create</c>.
    /// </summary>
    internal static IReadOnlySet<string> Overloaded(IEnumerable<Node> nodes)
        => nodes.GroupBy(node => node.QualifiedName, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);

    internal static string Compose(Node node, XmlDocs docs, Examples examples, string fileName)
    {
        MemberDoc doc = docs.For(node);

        StringBuilder page = new();
        page.AppendLine("## In Depth");
        page.AppendLine();

        // The signature first, the way Dynamo's own pages introduce a node. It is the one thing a
        // reader checks before anything else: what goes in, in what order.
        page.Append('`').Append(Signature(node)).AppendLine("`");
        page.AppendLine();

        string summary = node.Kind == NodeKind.NodeModel
            ? Longest(doc.Summary, node.Description)
            : doc.Summary;

        page.AppendLine(summary.Length > 0 ? summary : Missing);

        if (node.Parameters.Count > 0)
        {
            page.AppendLine();
            page.AppendLine("The inputs are:");
            page.AppendLine();

            foreach (NodeParameter parameter in node.Parameters)
            {
                page.Append("- `").Append(parameter.Name).Append('`');
                page.Append(" (_").Append(parameter.Type.Display()).Append('_');

                if (parameter.Default is { } given)
                {
                    page.Append(", defaults to `").Append(given).Append('`');
                }

                page.Append(") — ");
                page.AppendLine(doc.Parameter(parameter.Name) ?? "_Not documented yet._");
            }
        }

        if (Outputs(node, doc) is { Length: > 0 } outputs)
        {
            page.AppendLine();
            page.AppendLine(outputs);
        }

        if (node.Category is { } category)
        {
            page.AppendLine();
            page.Append("Found in the library under **").Append(category).AppendLine("**.");
        }

        if (doc.Search.Count > 0)
        {
            page.AppendLine();
            page.Append("Search terms: ")
                .AppendLine(string.Join(", ", doc.Search.Select(term => "`" + term + "`")) + ".");
        }

        // The family's shared rules. A reader who opened this page from the library has not seen
        // them anywhere else, and they are frequently the thing that decides whether the node does
        // what was expected. Skipped when it repeats the node's own summary, which the wrapper
        // classes' one-line "Wrapper class for X." comments otherwise would.
        if (node.Kind == NodeKind.ZeroTouch &&
            docs.ForType(node.DeclaringType) is { Length: > 0 } family &&
            !family.Equals(summary, StringComparison.Ordinal) &&
            !IsBoilerplate(family))
        {
            page.AppendLine();
            page.AppendLine("___");
            page.Append("## About the ").Append(ShortTypeName(node)).AppendLine(" nodes");
            page.AppendLine();
            page.AppendLine(family);
        }

        AppendExample(page, node, examples, fileName);
        return page.ToString();
    }

    /// <summary>
    /// Dynamo's pages end with an example graph and a picture of it. The browser looks for both
    /// beside the Markdown file and under the same name, so nothing needs to be linked by hand:
    /// the image is embedded, and a matching <c>.dyn</c> lights up the panel's insert button.
    ///
    /// The section appears only once there is something to put in it. An image reference to a file
    /// that is not there renders as a broken image in the browser panel, which reads as a
    /// packaging fault rather than as a page nobody has illustrated yet.
    /// </summary>
    private static void AppendExample(StringBuilder page, Node node, Examples examples, string fileName)
    {
        string? image = examples.ImageFor(fileName);
        bool hasGraph = examples.HasGraph(fileName);

        if (image is null && !hasGraph)
        {
            return;
        }

        page.AppendLine();
        page.AppendLine("___");
        page.AppendLine("## Example File");
        page.AppendLine();

        if (hasGraph)
        {
            page.AppendLine("An example graph ships beside this page. Use the panel's insert button to open it.");
            page.AppendLine();
        }

        if (image is not null)
        {
            page.Append("![").Append(node.ShortName).Append("](./").Append(Uri.EscapeDataString(image)).AppendLine(")");
        }
    }

    private static string Outputs(Node node, MemberDoc doc)
    {
        if (node.ReturnKeys.Count > 0)
        {
            StringBuilder text = new();
            text.AppendLine("The outputs are:");
            text.AppendLine();

            foreach (string key in node.ReturnKeys)
            {
                text.Append("- `").Append(key).Append("` — ")
                    .AppendLine(doc.Returns(key) ?? "_Not documented yet._");
            }

            return text.ToString().TrimEnd();
        }

        if (node.Kind == NodeKind.NodeModel)
        {
            // A NodeModel node declares its ports in code rather than in a signature. Naming an
            // output this reader cannot see would be a guess.
            return string.Empty;
        }

        if (node.Returns.Name == "System.Void")
        {
            return "Returns nothing.";
        }

        string name = doc.ReturnName ?? "result";
        string? description = doc.Returns(name) ?? doc.Returns("result");

        return description is null
            ? $"Returns `{name}` (_{node.Returns.Display()}_)."
            : $"Returns `{name}` (_{node.Returns.Display()}_) — {description}";
    }

    private static string Signature(Node node)
    {
        if (node.Kind == NodeKind.NodeModel)
        {
            // A dropdown has no arguments to write out; what identifies it is the name on the node
            // and the class Dynamo places, which is also what the file is named after.
            return node.DisplayName ?? node.ShortName;
        }

        string parameters = string.Join(", ", node.Parameters.Select(parameter =>
            parameter.Default is { } given ? $"{parameter.Name}: {given}" : parameter.Name));

        return $"{ShortTypeName(node)}.{node.Member}({parameters})";
    }

    private static string ShortTypeName(Node node)
        => node.DeclaringType[(node.DeclaringType.LastIndexOf('.') + 1)..];

    private static string Longest(string first, string? second)
        => (second ?? string.Empty).Length > first.Length ? second! : first;

    /// <summary>
    /// The zero-touch wrapper classes nearly all carry the same one-line comment, generated years
    /// ago and never expanded. Repeating "Wrapper class for Element." under an "About the Element
    /// nodes" heading tells a reader nothing and makes the gap look filled.
    /// </summary>
    private static bool IsBoilerplate(string summary)
        => summary.StartsWith("Wrapper class for", StringComparison.OrdinalIgnoreCase) ||
           summary.StartsWith("Wrapper for", StringComparison.OrdinalIgnoreCase);
}
