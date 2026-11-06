using System.Text;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public sealed record VocabularyContribution(string Brazilian, string BrazilianExample, string European,
    string EuropeanExample, string Description);

internal static class VocabularyContributions
{
    internal static readonly string[] Headers =
        { "pt-BR", "Example pt-BR", "pt-PT", "Example pt-PT", "Description" };

    internal static void Validate(VocabularyContribution entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Brazilian) || string.IsNullOrWhiteSpace(entry.European))
            throw new FormatException("PT-BR and PT-PT are required for every entry.");
    }

    internal static string Serialize(IEnumerable<VocabularyContribution> entries)
    {
        var rows = new List<string[]>();
        foreach (var entry in entries)
        {
            Validate(entry);
            rows.Add(new[] { entry.Brazilian, entry.BrazilianExample, entry.European,
                entry.EuropeanExample, entry.Description });
        }
        return SemicolonCsv.Serialize(Headers, rows);
    }

    internal static IReadOnlyList<VocabularyContribution> Parse(string csv)
    {
        var rows = SemicolonCsv.Parse(csv, Headers);
        var entries = new List<VocabularyContribution>();
        for (var i = 0; i < rows.Count; i++)
        {
            var fields = rows[i];
            var entry = new VocabularyContribution(fields[0], fields[1], fields[2], fields[3], fields[4]);
            try { Validate(entry); }
            catch (FormatException ex) { throw new FormatException($"CSV record {i + 2}: {ex.Message}", ex); }
            entries.Add(entry);
        }
        return entries;
    }
}

internal static class SemicolonCsv
{
    internal static string Serialize(string[] headers, IEnumerable<string[]> rows) =>
        string.Join(";", headers) + "\r\n" +
        string.Concat(rows.Select(row => string.Join(";", row.Select(Quote)) + "\r\n"));

    private static bool NeedsProtection(string value) =>
        value.Length > 0 && (value[0] == '\'' || "=+-@".Contains(value.TrimStart().FirstOrDefault()) ||
            value[0] is '\t' or '\r' or '\n');

    private static string Quote(string value)
    {
        // Quoting alone does not stop spreadsheet formula execution.
        if (NeedsProtection(value)) value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static string Unprotect(string value) =>
        value.StartsWith('\'') && NeedsProtection(value[1..]) ? value[1..] : value;

    internal static IReadOnlyList<string[]> Parse(string csv, string[] headers)
    {
        var rows = ReadRows(csv.TrimStart('\uFEFF'));
        if (rows.Count == 0 || !rows[0].SequenceEqual(headers, StringComparer.OrdinalIgnoreCase))
            throw new FormatException("Expected semicolon-separated columns: " + string.Join("; ", headers) + ".");
        var entries = new List<string[]>();
        for (var i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count != headers.Length)
                throw new FormatException($"CSV record {i + 1}: expected {headers.Length} columns, found {row.Count}.");
            var fields = row.Select(Unprotect).ToArray();
            entries.Add(fields);
        }
        return entries;
    }

    private static List<List<string>> ReadRows(string csv)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        var closed = false;
        for (var i = 0; i < csv.Length; i++)
        {
            var c = csv[i];
            if (quoted)
            {
                if (c != '"') field.Append(c);
                else if (i + 1 < csv.Length && csv[i + 1] == '"') { field.Append('"'); i++; }
                else { quoted = false; closed = true; }
                continue;
            }
            if (c == '"' && field.Length == 0 && !closed) { quoted = true; continue; }
            if (c is ';' or '\r' or '\n')
            {
                row.Add(field.ToString());
                field.Clear();
                closed = false;
                if (c == ';') continue;
                if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
                if (row.Count != 1 || row[0].Length != 0) rows.Add(row);
                row = new List<string>();
                continue;
            }
            if (closed || c == '"')
                throw new FormatException($"Invalid CSV quoting near character {i + 1}.");
            field.Append(c);
        }
        if (quoted) throw new FormatException("Unclosed quoted CSV field.");
        if (field.Length > 0 || closed || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }
        return rows;
    }
}
