using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Rhythm.Docs;

/// <summary>The documentation the compiler emitted for one node.</summary>
internal sealed class MemberDoc
{
    internal static readonly MemberDoc Empty = new();

    internal string Summary { get; init; } = string.Empty;

    /// <summary>The name on the first <c>returns</c> tag, which Dynamo shows as the output port label.</summary>
    internal string? ReturnName { get; init; }

    internal IReadOnlyList<string> Search { get; init; } = Array.Empty<string>();

    internal IReadOnlyDictionary<string, string> Parameters { get; init; }
        = new Dictionary<string, string>(StringComparer.Ordinal);

    internal IReadOnlyDictionary<string, string> ReturnDescriptions { get; init; }
        = new Dictionary<string, string>(StringComparer.Ordinal);

    internal string? Parameter(string name)
        => Parameters.TryGetValue(name, out string? text) && text.Length > 0 ? text : null;

    internal string? Returns(string name)
        => ReturnDescriptions.TryGetValue(name, out string? text) && text.Length > 0 ? text : null;

    internal bool IsEmpty => this == Empty;
}

/// <summary>
/// Reads the XML documentation files the compiler writes beside the assemblies.
///
/// This is the same file Dynamo itself reads for port names and tooltips, which is the point: the
/// generated help and the tooltip a user sees hovering a port come from one source, so the two
/// cannot say different things. Several assemblies are merged into one lookup because a member ID
/// is fully qualified and therefore unique across all of them.
/// </summary>
internal sealed class XmlDocs
{
    private readonly Dictionary<string, MemberDoc> _members = new(StringComparer.Ordinal);

    internal static XmlDocs Load(IEnumerable<string> paths)
    {
        XmlDocs docs = new();

        foreach (string path in paths)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"No XML documentation beside the assembly at {path}. Every Rhythm project sets " +
                    "GenerateDocumentationFile, so a missing file means the deploy folder is incomplete.",
                    path);
            }

            foreach (XElement member in XDocument.Load(path).Descendants("member"))
            {
                string? name = member.Attribute("name")?.Value;

                // Methods for zero-touch nodes, types for the NodeModel nodes and for the family
                // summary each zero-touch page carries.
                if (name is null ||
                    !(name.StartsWith("M:", StringComparison.Ordinal) ||
                      name.StartsWith("T:", StringComparison.Ordinal)))
                {
                    continue;
                }

                docs._members[name] = Read(member);
            }
        }

        return docs;
    }

    internal MemberDoc For(Node node)
        => _members.TryGetValue(node.DocId, out MemberDoc? doc) ? doc : MemberDoc.Empty;

    /// <summary>
    /// The summary on the class a zero-touch node belongs to.
    ///
    /// Worth repeating on every page: it is where the rules shared by a whole family live, and a
    /// reader who arrived at one node from the library has not read it anywhere else.
    /// </summary>
    internal string ForType(string fullName)
        => _members.TryGetValue("T:" + fullName, out MemberDoc? doc) ? doc.Summary : string.Empty;

    private static MemberDoc Read(XElement member)
    {
        Dictionary<string, string> parameters = new(StringComparer.Ordinal);

        foreach (XElement parameter in member.Elements("param"))
        {
            if (parameter.Attribute("name")?.Value is { } name)
            {
                parameters[name] = Flatten(parameter);
            }
        }

        // A node with several outputs carries one returns tag per output, each named. A node with
        // one output carries a single tag whose name is the output port's label.
        Dictionary<string, string> returns = new(StringComparer.Ordinal);
        string? firstReturnName = null;

        foreach (XElement value in member.Elements("returns"))
        {
            string name = value.Attribute("name")?.Value ?? "result";
            returns[name] = Flatten(value);
            firstReturnName ??= value.Attribute("name")?.Value;
        }

        return new MemberDoc
        {
            Summary = Flatten(member.Element("summary")),
            ReturnName = firstReturnName,
            Parameters = parameters,
            ReturnDescriptions = returns,
            Search = (member.Element("search")?.Value ?? string.Empty)
                .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        };
    }

    /// <summary>
    /// Turns a documentation element into Markdown: <c>see cref</c> and <c>c</c> become inline
    /// code, paragraph breaks survive, and the hard wrapping of the source comment does not.
    /// </summary>
    private static string Flatten(XElement? element)
    {
        if (element is null)
        {
            return string.Empty;
        }

        StringBuilder text = new();

        foreach (XNode node in element.Nodes())
        {
            switch (node)
            {
                case XText raw:
                    text.Append(raw.Value);
                    break;

                case XElement child when child.Name == "see" || child.Name == "c":
                    text.Append('`').Append(Referenced(child)).Append('`');
                    break;

                case XElement child when child.Name == "paramref" || child.Name == "typeparamref":
                    text.Append('`').Append(child.Attribute("name")?.Value ?? string.Empty).Append('`');
                    break;

                case XElement child when child.Name == "b" || child.Name == "i":
                    text.Append("**").Append(child.Value.Trim()).Append("**");
                    break;

                case XElement child:
                    text.Append(child.Value);
                    break;
            }
        }

        return Reflow(text.ToString());
    }

    /// <summary>A cref reads as <c>T:Rhythm.Revit.Elements.Elements</c>; only the last part is useful.</summary>
    private static string Referenced(XElement element)
    {
        string reference = element.Attribute("cref")?.Value ?? element.Value;

        if (reference.Length > 2 && reference[1] == ':')
        {
            reference = reference[2..];
        }

        int parenthesis = reference.IndexOf('(', StringComparison.Ordinal);
        if (parenthesis >= 0)
        {
            reference = reference[..parenthesis];
        }

        string[] parts = reference.Split('.');
        return parts.Length <= 2 ? reference : string.Join('.', parts[^2..]);
    }

    /// <summary>
    /// Unwraps the source comment's hard line breaks while keeping blank-line paragraph breaks.
    /// Doc comments are wrapped to fit a code editor; the browser panel wraps for itself, and
    /// leaving both in place produces a ragged column half the width of the pane.
    /// </summary>
    private static string Reflow(string text)
    {
        string[] paragraphs = Regex.Split(text.Trim(), @"\r?\n[ \t]*\r?\n");

        return string.Join(
            "\n\n",
            paragraphs
                .Select(paragraph => Regex.Replace(paragraph, @"\s*\r?\n\s*", " ").Trim())
                .Where(paragraph => paragraph.Length > 0));
    }
}
