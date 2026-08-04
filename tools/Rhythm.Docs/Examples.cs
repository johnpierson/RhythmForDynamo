namespace Rhythm.Docs;

/// <summary>
/// The example graphs and screenshots already sitting in the output folder.
///
/// Nothing is copied or renamed here. The assets are checked in under the name Dynamo resolves
/// them by — <c>Rhythm.Helpers.Helpers.Toggle.dyn</c> beside <c>Rhythm.Helpers.Helpers.Toggle.md</c>
/// — so the file name is the link, and an example that stops matching a node shows up as an
/// orphan in the report rather than quietly documenting nothing.
/// </summary>
internal sealed class Examples
{
    private static readonly string[] ImageExtensions = { ".png", ".gif", ".jpg", ".jpeg" };

    private readonly Dictionary<string, string> _images = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _graphs = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _legacyGraphs = new();

    private Examples()
    {
    }

    /// <summary>
    /// Dynamo 2.0 replaced the XML graph format with JSON and cannot open the old one. A graph
    /// still in the old format is not an example the insert button can deliver, so it is not
    /// counted as one — a page promising a graph that fails to open is worse than a page with no
    /// example section at all.
    /// </summary>
    private static bool IsCurrentFormat(string path)
    {
        using StreamReader reader = new(path);

        for (int character = reader.Read(); character >= 0; character = reader.Read())
        {
            if (!char.IsWhiteSpace((char)character) && character != '﻿')
            {
                return character == '{';
            }
        }

        return false;
    }

    internal static Examples In(string folder)
    {
        Examples examples = new();

        if (!Directory.Exists(folder))
        {
            return examples;
        }

        foreach (string path in Directory.EnumerateFiles(folder))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);

            if (extension.Equals(".dyn", StringComparison.OrdinalIgnoreCase))
            {
                if (IsCurrentFormat(path))
                {
                    examples._graphs.Add(name);
                }
                else
                {
                    examples._legacyGraphs.Add(Path.GetFileName(path));
                }
            }
            else if (ImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) &&
                     name.EndsWith("_img", StringComparison.Ordinal))
            {
                examples._images[name[..^"_img".Length]] = Path.GetFileName(path);
            }
        }

        return examples;
    }

    /// <summary>The image file to embed on a node's page, or null when nobody has illustrated it.</summary>
    internal string? ImageFor(string fileName)
        => _images.TryGetValue(fileName, out string? image) ? image : null;

    internal bool HasGraph(string fileName) => _graphs.Contains(fileName);

    internal int ImageCount => _images.Count;

    internal int GraphCount => _graphs.Count;

    /// <summary>Graphs in the pre-2.0 XML format, which no supported Dynamo can open.</summary>
    internal IReadOnlyList<string> LegacyGraphs => _legacyGraphs;

    /// <summary>
    /// Assets whose name matches no node.
    ///
    /// Usually a node that was renamed or withdrawn, and always worth saying out loud: the asset
    /// is still in the repository taking up space in every package, illustrating nothing.
    /// </summary>
    internal IEnumerable<string> Orphans(IReadOnlySet<string> fileNames)
        => _images.Keys.Where(name => !fileNames.Contains(name)).Select(name => name + "_img")
            .Concat(_graphs.Where(name => !fileNames.Contains(name)).Select(name => name + ".dyn"))
            .OrderBy(name => name, StringComparer.Ordinal);
}
