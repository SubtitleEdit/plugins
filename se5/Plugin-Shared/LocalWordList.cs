using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SubtitleEdit.Plugins.Shared;

/// <summary>
/// User-maintained additions to the bundled word list, stored as XML next to the plugin
/// folders (Plugins/AmericanToBritish.xml - same file name and format as the SE4 plugin, so an
/// existing SE4 list keeps working):
/// <code>
/// &lt;Words&gt;
///   &lt;Word us="dumb" br="stupid" /&gt;
///   &lt;Ignore us="color" /&gt;
/// &lt;/Words&gt;
/// </code>
/// <c>Word</c> adds (or overrides) a pair; <c>Ignore</c> stops a built-in pair from being used.
/// Words are keyed by their American spelling.
/// </summary>
public sealed class LocalWordList
{
    public List<(string Us, string Br)> Words { get; } = new();
    public List<string> Ignored { get; } = new();

    public bool IsEmpty => Words.Count == 0 && Ignored.Count == 0;

    /// <summary>Plugins/&lt;fileName&gt;, i.e. the parent of this plugin's own folder.</summary>
    public static string GetDefaultPath(string fileName)
    {
        var pluginFolder = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var pluginsFolder = Path.GetDirectoryName(pluginFolder) ?? pluginFolder;
        return Path.Combine(pluginsFolder, fileName);
    }

    /// <summary>Loads the list; a missing file is an empty list. Throws on malformed XML.</summary>
    public static LocalWordList Load(string path)
    {
        var list = new LocalWordList();
        if (!File.Exists(path))
        {
            return list;
        }

        var xml = LoadXml(path);
        if (xml.Root?.Name != "Words")
        {
            throw new InvalidDataException($"Root element must be <Words> in {path}");
        }

        foreach (var element in xml.Root.Elements("Word"))
        {
            var us = element.Attribute("us")?.Value.Trim();
            var br = element.Attribute("br")?.Value.Trim();
            if (!string.IsNullOrEmpty(us) && !string.IsNullOrEmpty(br))
            {
                list.Words.Add((us!, br!));
            }
        }

        foreach (var element in xml.Root.Elements("Ignore"))
        {
            var us = element.Attribute("us")?.Value.Trim();
            if (!string.IsNullOrEmpty(us))
            {
                list.Ignored.Add(us!);
            }
        }

        return list;
    }

    /// <summary>
    /// Also accepts bare &lt;Word .../&gt; lines without the &lt;Words&gt; root - an easy mistake
    /// when hand-writing the file ("There are multiple root elements").
    /// </summary>
    private static XDocument LoadXml(string path)
    {
        try
        {
            return XDocument.Load(path);
        }
        catch (XmlException)
        {
            var text = File.ReadAllText(path);
            var declarationEnd = text.StartsWith("<?xml", StringComparison.Ordinal) ? text.IndexOf("?>", StringComparison.Ordinal) : -1;
            if (declarationEnd >= 0)
            {
                text = text.Substring(declarationEnd + 2);
            }

            try
            {
                var wrapped = XDocument.Parse("<Words>" + text + "</Words>");
                if (wrapped.Root!.Elements().All(e => e.Name == "Word" || e.Name == "Ignore"))
                {
                    return wrapped;
                }
            }
            catch (XmlException)
            {
                // fall through to the original error
            }

            throw;
        }
    }

    public void Save(string path)
    {
        var root = new XElement("Words",
            Words.Select(w => new XElement("Word", new XAttribute("us", w.Us), new XAttribute("br", w.Br))),
            Ignored.Select(us => new XElement("Ignore", new XAttribute("us", us))));

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        new XDocument(new XDeclaration("1.0", "utf-8", null), root).Save(path);
    }
}
