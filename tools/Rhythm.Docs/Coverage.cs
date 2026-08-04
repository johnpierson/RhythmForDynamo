using System.Text;

namespace Rhythm.Docs;

/// <summary>
/// Counts how much of the generated help is actually written, and says so plainly.
///
/// The generator will happily produce 300 well-formed pages out of 300 empty summaries, and the
/// folder would look finished. This is the number that says otherwise: how many nodes have a
/// summary, how many have every input described, and how many produce a page with nothing on it
/// but a signature. It is printed on every run and written into the folder's README so the figure
/// is in the repository rather than in a terminal somebody once looked at.
/// </summary>
internal sealed class Coverage
{
    private readonly List<Row> _rows = new();

    private sealed record Row(
        Node Node,
        bool HasSummary,
        int Parameters,
        int DocumentedParameters,
        bool HasOutput,
        bool HasImage,
        bool HasGraph);

    internal void Add(Node node, MemberDoc doc, string fileName, Examples examples)
    {
        string summary = node.Kind == NodeKind.NodeModel
            ? (doc.Summary.Length > 0 ? doc.Summary : node.Description ?? string.Empty)
            : doc.Summary;

        int documented = node.Parameters.Count(parameter => doc.Parameter(parameter.Name) is not null);

        bool hasOutput = node.ReturnKeys.Count > 0
            ? node.ReturnKeys.All(key => doc.Returns(key) is not null)
            : doc.Returns(doc.ReturnName ?? "result") is not null;

        _rows.Add(new Row(
            node,
            HasSummary: summary.Trim().Length > 0,
            node.Parameters.Count,
            documented,
            hasOutput,
            examples.ImageFor(fileName) is not null,
            examples.HasGraph(fileName)));
    }

    internal int Total => _rows.Count;

    /// <summary>
    /// Pages carrying nothing a reader could not have worked out from the node itself: no summary,
    /// no described input, no described output. The signature is all they say.
    /// </summary>
    internal IReadOnlyList<Node> Empty
        => _rows.Where(row => !row.HasSummary && row.DocumentedParameters == 0 && !row.HasOutput)
            .Select(row => row.Node)
            .OrderBy(node => node.QualifiedName, StringComparer.Ordinal)
            .ToList();

    internal string Report()
    {
        StringBuilder text = new();

        int summaries = _rows.Count(row => row.HasSummary);
        int withInputs = _rows.Count(row => row.Parameters > 0);
        int fullyDescribedInputs = _rows.Count(row => row.Parameters > 0 && row.DocumentedParameters == row.Parameters);
        int noDescribedInputs = _rows.Count(row => row.Parameters > 0 && row.DocumentedParameters == 0);
        int outputs = _rows.Count(row => row.HasOutput);
        int images = _rows.Count(row => row.HasImage);
        int graphs = _rows.Count(row => row.HasGraph);
        int empty = Empty.Count;

        // Pages, not nodes: two overloads Dynamo cannot tell apart share one, and the section
        // below names them.
        text.AppendLine("| | Pages | Share |");
        text.AppendLine("|---|---:|---:|");
        text.AppendLine(Line("Total", Total, Total));
        text.AppendLine(Line("With a summary", summaries, Total));
        text.AppendLine(Line("Taking at least one input", withInputs, Total));
        text.AppendLine(Line("— every input described", fullyDescribedInputs, withInputs));
        text.AppendLine(Line("— no input described", noDescribedInputs, withInputs));
        text.AppendLine(Line("With the output described", outputs, Total));
        text.AppendLine(Line("With an example image", images, Total));
        text.AppendLine(Line("With an example graph", graphs, Total));
        text.AppendLine(Line("**Empty — signature only**", empty, Total));
        text.AppendLine();
        text.AppendLine("The two input rows are shares of the nodes that take inputs, not of every node.");

        text.AppendLine();
        text.AppendLine("By assembly:");
        text.AppendLine();
        text.AppendLine("| Assembly | Pages | With a summary | Empty |");
        text.AppendLine("|---|---:|---:|---:|");

        foreach (IGrouping<string, Row> group in _rows
            .GroupBy(row => row.Node.Assembly, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            int count = group.Count();
            int described = group.Count(row => row.HasSummary);
            int blank = group.Count(row => !row.HasSummary && row.DocumentedParameters == 0 && !row.HasOutput);

            text.AppendLine($"| {group.Key} | {count} | {described} ({Percent(described, count)}) | {blank} ({Percent(blank, count)}) |");
        }

        return text.ToString();
    }

    /// <summary>A short line for the console, so a contributor sees the number without opening a file.</summary>
    internal string Summary()
    {
        int summaries = _rows.Count(row => row.HasSummary);
        return $"{Total} pages, {summaries} with a summary ({Percent(summaries, Total)}), " +
               $"{Empty.Count} with nothing but a signature ({Percent(Empty.Count, Total)})";
    }

    private static string Line(string label, int value, int total)
        => $"| {label} | {value} | {Percent(value, total)} |";

    private static string Percent(int value, int total)
        => total == 0 ? "—" : (value * 100.0 / total).ToString("0") + "%";
}
