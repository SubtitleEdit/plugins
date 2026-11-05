using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>
/// User additions to the built-in list, saved as Plugins/BrazilianToEuropeanPortuguese.xml:
/// <code>
/// &lt;Words&gt;
///   &lt;Word br="geladeira" pt="frigorífico" /&gt;
///   &lt;Ignore br="garota" /&gt;
/// &lt;/Words&gt;
/// </code>
/// <c>Word</c> adds (or overrides) a conversion; <c>Ignore</c> leaves a word or phrase untouched.
/// </summary>
public sealed class PortugueseLocalWordList
{
    public const string FileName = "BrazilianToEuropeanPortuguese.xml";

    public List<(string Br, string Pt)> Words { get; } = new();
    public List<string> Ignored { get; } = new();

    public static string GetDefaultPath()
    {
        var pluginFolder = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var pluginsFolder = Path.GetDirectoryName(pluginFolder) ?? pluginFolder;
        return Path.Combine(pluginsFolder, FileName);
    }

    /// <summary>A missing file is an empty list. Throws on malformed XML.</summary>
    public static PortugueseLocalWordList Load(string path)
    {
        var list = new PortugueseLocalWordList();
        if (!File.Exists(path))
        {
            return list;
        }

        var xml = XDocument.Load(path);
        if (xml.Root?.Name != "Words")
        {
            throw new InvalidDataException($"Root element must be <Words> in {path}");
        }

        foreach (var element in xml.Root.Elements("Word"))
        {
            var br = element.Attribute("br")?.Value.Trim();
            var pt = element.Attribute("pt")?.Value.Trim();
            if (!string.IsNullOrEmpty(br) && !string.IsNullOrEmpty(pt))
            {
                list.Words.Add((br!, pt!));
            }
        }

        foreach (var element in xml.Root.Elements("Ignore"))
        {
            var br = element.Attribute("br")?.Value.Trim();
            if (!string.IsNullOrEmpty(br))
            {
                list.Ignored.Add(br!);
            }
        }

        return list;
    }

    public void Save(string path)
    {
        var root = new XElement("Words");
        foreach (var (br, pt) in Words)
        {
            root.Add(new XElement("Word", new XAttribute("br", br), new XAttribute("pt", pt)));
        }

        foreach (var br in Ignored)
        {
            root.Add(new XElement("Ignore", new XAttribute("br", br)));
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        new XDocument(new XDeclaration("1.0", "utf-8", null), root).Save(path);
    }
}
