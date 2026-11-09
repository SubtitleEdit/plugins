using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal sealed class PersonalNameArticles
{
    private readonly Dictionary<string, string> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly ISet<string>? _unresolved;
    private static readonly Regex NamePattern = new(@"^\p{L}[\p{L}\p{M}]*$", RegexOptions.CultureInvariant);

    public PersonalNameArticles(string text, ISet<string>? unresolved = null)
    {
        _unresolved = unresolved;
        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0) continue;
            var parts = line.Split('=');
            if (parts.Length != 2 || !NamePattern.IsMatch(parts[0].Trim()))
                throw new InvalidDataException($"Name articles, line {i + 1}: use Name = o, Name = a, or Name = none (one single-word name per line).");
            var name = parts[0].Trim();
            var article = parts[1].Trim().ToLowerInvariant();
            if (article is not ("o" or "a" or "none"))
                throw new InvalidDataException($"Name articles, line {i + 1}: article must be o, a, or none.");
            if (!_entries.TryAdd(name, article))
                throw new InvalidDataException($"Name articles, line {i + 1}: {name} is listed more than once.");
        }
    }

    public bool TryGet(string name, out string article)
    {
        article = "";
        return name.Length > 0 && char.IsUpper(name[0]) && _entries.TryGetValue(name, out article!);
    }

    public void ReportUnknown(string name)
    {
        if (_unresolved != null && name.Length > 1 && char.IsUpper(name[0]) &&
            !NaturaDictionary.Load(true).Check(name.ToLowerInvariant()))
            _unresolved.Add(name);
    }
}
