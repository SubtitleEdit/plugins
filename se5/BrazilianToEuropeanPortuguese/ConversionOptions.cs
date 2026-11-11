using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public enum AddressMode { Automatic, Informal, Formal }

/// <summary>Which rule groups run. Remembered by Subtitle Edit (window and Batch convert).</summary>
public sealed record ConversionOptions(
    bool Spelling = true,
    bool Vocabulary = true,
    bool Progressive = true,
    bool Pronouns = true,
    bool Colloquial = true,
    bool YouToTu = true,
    bool PostAgreement = true,
    bool ValidateSpelling = false,
    bool UseReference = true,
    AddressMode Address = AddressMode.Automatic,
    bool SentenceContext = false,
    bool Rewrap = false,
    int LineLength = 43,
    int MaxLines = 2,
    int CharactersPerSecond = 20,
    string DisabledRules = "",
    bool Conversational = false,
    bool LayoutWarnings = false,
    string NameArticles = "",
    bool AskNameArticles = true,
    bool GrammarCheck = false,
    bool GrammarAutoFix = false,
    string GrammarServer = "http://127.0.0.1:8081/",
    bool GrammarExternalServer = false)
{
    public const int Version = 1;

    public static ConversionOptions Default => new();

    public JsonElement ToJson() => JsonSerializer.SerializeToElement(new
    {
        spelling = Spelling,
        vocabulary = Vocabulary,
        progressive = Progressive,
        pronouns = Pronouns,
        colloquial = Colloquial,
        youToTu = YouToTu,
        postAgreement = PostAgreement,
        validateSpelling = ValidateSpelling,
        useReference = UseReference,
        address = Address.ToString(),
        sentenceContext = SentenceContext,
        rewrap = Rewrap,
        lineLength = LineLength,
        maxLines = MaxLines,
        charactersPerSecond = CharactersPerSecond,
        disabledRules = DisabledRules,
        conversational = Conversational,
        layoutWarnings = LayoutWarnings,
        nameArticles = NameArticles,
        askNameArticles = AskNameArticles,
        grammarCheck = GrammarCheck,
        grammarAutoFix = GrammarAutoFix,
        grammarServer = GrammarServer,
        grammarExternalServer = GrammarExternalServer,
    });

    public static ConversionOptions Load(PluginRequest request)
    {
        var settings = request.Settings;
        if (request.SettingsVersion != Version || settings is not { ValueKind: JsonValueKind.Object })
        {
            return Default;
        }

        var json = settings.Value;
        return new ConversionOptions(
            Read(json, "spelling"),
            Read(json, "vocabulary"),
            Read(json, "progressive"),
            Read(json, "pronouns"),
            Read(json, "colloquial"),
            Read(json, "youToTu"),
            Read(json, "postAgreement"),
            json.TryGetProperty("validateSpelling", out var validation) && validation.ValueKind == JsonValueKind.True,
            Read(json, "useReference"),
            ReadAddress(json),
            ReadOptIn(json, "sentenceContext"),
            ReadOptIn(json, "rewrap"),
            ReadNumber(json, "lineLength", 43, 20, 100),
            ReadNumber(json, "maxLines", 2, 1, 4),
            ReadNumber(json, "charactersPerSecond", 20, 5, 60),
            ReadDisabledRules(json),
            ReadOptIn(json, "conversational"),
            ReadOptIn(json, "layoutWarnings"),
            ReadNameArticles(json),
            !json.TryGetProperty("askNameArticles", out _) || ReadOptIn(json, "askNameArticles"),
            ReadOptIn(json, "grammarCheck"),
            ReadOptIn(json, "grammarAutoFix"),
            ReadGrammarServer(json),
            ReadOptIn(json, "grammarExternalServer"));
    }

    public bool RuleEnabled(string id) => DisabledRules.Length == 0 || !DisabledRules.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Contains(id, StringComparer.Ordinal);

    private static bool ReadOptIn(JsonElement json, string name)
    {
        if (!json.TryGetProperty(name, out var value)) return false;
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new InvalidDataException($"Invalid {name} in plugin settings (expected a boolean)."),
        };
    }

    private static string ReadDisabledRules(JsonElement json)
    {
        if (!json.TryGetProperty("disabledRules", out var value)) return "";
        if (value.ValueKind == JsonValueKind.String) return value.GetString()!;
        throw new InvalidDataException("Invalid disabledRules in plugin settings (expected a string).");
    }

    private static string ReadNameArticles(JsonElement json)
    {
        if (!json.TryGetProperty("nameArticles", out var value)) return "";
        if (value.ValueKind != JsonValueKind.String)
            throw new InvalidDataException("Invalid nameArticles in plugin settings (expected a string).");
        var text = value.GetString()!;
        _ = new PersonalNameArticles(text);
        return text;
    }

    private static string ReadGrammarServer(JsonElement json)
    {
        if (!json.TryGetProperty("grammarServer", out var value)) return Default.GrammarServer;
        if (value.ValueKind != JsonValueKind.String)
            throw new InvalidDataException("Invalid grammarServer in plugin settings (expected a URL string).");
        var server = value.GetString()!;
        _ = LanguageToolClient.CheckUri(server);
        return server;
    }

    private static AddressMode ReadAddress(JsonElement json)
    {
        if (!json.TryGetProperty("address", out var value)) return AddressMode.Automatic;
        if (value.ValueKind == JsonValueKind.String &&
            Enum.TryParse<AddressMode>(value.GetString(), out var address) && Enum.IsDefined(address)) return address;
        throw new InvalidDataException("Invalid address mode in plugin settings.");
    }

    private static int ReadNumber(JsonElement json, string name, int fallback, int minimum, int maximum)
    {
        if (!json.TryGetProperty(name, out var value)) return fallback;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) && number >= minimum && number <= maximum) return number;
        throw new InvalidDataException($"Invalid {name} in plugin settings (expected {minimum}-{maximum}).");
    }

    private static bool Read(JsonElement json, string name) =>
        !json.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.False;
}
