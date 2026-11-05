using System.Text.Json;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public static class CorrectionExport
{
    public static JsonElement Create(IEnumerable<ChangeProposal> proposals) => JsonSerializer.SerializeToElement(new
    {
        schemaVersion = 1,
        cases = proposals.Where(p => p.Include || p.ExpectedText != p.ConvertedText).Select(p => new
        {
            original = p.OriginalText,
            expected = p.ExpectedText,
            rules = p.Rules.Select(r => r.Id),
            sentenceGroup = p.SentenceGroup,
        }),
    });
}
