using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class EmphaticAndPolicyDialogueTests
{
    public static TheoryData<string, string?, string> Examples => new()
    {
        { "-Por favor.\n-Como diabos ela saberia?", "Oh, please. How the hell would she know?",
            "-Por favor.\n-Como diabos saberia ela?" },
        { "Como diabos ele saberia?", null, "Como diabos saberia ele?" },
        { "Como diabos elas saberiam?", null, "Como diabos saberiam elas?" },
        { "Como raios ela sabe?", null, "Como raios sabe ela?" },
        { "Como raio eles sabiam?", null, "Como raio sabiam eles?" },
        { "Como caraças ela soube?", null, "Como caraças soube ela?" },
        { "Como diabos ela faria?", null, "Como diabos faria ela?" },
        { "Como diabos elas trabalharão?", null, "Como diabos trabalharão elas?" },
        { "Como diabos ela o saberia?", null, "Como diabos o saberia ela?" },
        { "<i>Como diabos ela saberia?</i>", null, "<i>Como diabos saberia ela?</i>" },
        { "COMO DIABOS ELA SABERIA?", null, "COMO DIABOS SABERIA ELA?" },
        { "Dutton foi uma burocrata\naté 15 minutos atrás.", "Dutton was a dog catcher till 15 minutes ago.",
            "Dutton era uma burocrata\naté 15 minutos atrás." },
        { "Ela foi médica até há duas horas.", "She was a doctor until two hours ago.",
            "Ela era médica até há duas horas." },
        { "Eles foram professores até três dias atrás.", "They were teachers until three days ago.",
            "Eles eram professores até três dias atrás." },
        { "Ele foi um engenheiro até há um minuto.", "He was an engineer until a minute ago.",
            "Ele era um engenheiro até há um minuto." },
        { "DUTTON FOI UMA BUROCRATA ATÉ 15 MINUTOS ATRÁS.", "Dutton was a bureaucrat till 15 minutes ago.",
            "DUTTON ERA UMA BUROCRATA ATÉ 15 MINUTOS ATRÁS." },
        { "prioriza visar ao\ncrime de rua...", "prioritizes targeting street crime...",
            "dá prioridade ao combate\naos crimes de rua..." },
        { "O programa prioriza visar ao crime de rua.", "The program prioritizes targeting street crime.",
            "O programa dá prioridade ao combate aos crimes de rua." },
        { "Os programas priorizam visar os crimes de rua.", "The programs prioritize targeting street crime.",
            "Os programas dão prioridade ao combate aos crimes de rua." },
        { "O programa priorizava visar o crime de rua.", "The program prioritized targeting street crime.",
            "O programa dava prioridade ao combate aos crimes de rua." },
        { "O programa priorizou visar à criminalidade de rua.", "The program prioritized targeting street crime.",
            "O programa deu prioridade ao combate aos crimes de rua." },
        { "O programa priorizará visar ao crime de rua.", "The program will prioritize targeting street crime.",
            "O programa dará prioridade ao combate aos crimes de rua." },
        { "O programa priorizaria visar ao crime de rua.", "The program would prioritize targeting street crime.",
            "O programa daria prioridade ao combate aos crimes de rua." },
        { "PRIORIZA VISAR AO CRIME DE RUA.", "prioritizes targeting street crime.",
            "DÁ PRIORIDADE AO COMBATE AOS CRIMES DE RUA." },
    };

    [Theory]
    [MemberData(nameof(Examples))]
    public void GuardedConstructionsPreserveMeaningAndAreStable(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        var reference = ReferenceHint.FromText(english);
        var result = converter.Convert(source, reference: reference);
        Assert.Equal(expected, result);
        Assert.Equal(result, converter.Convert(result, reference: reference));
    }

    [Theory]
    [InlineData("Ela perguntou como diabos ela saberia.", null)]
    [InlineData("Ele perguntou\ncomo diabos ela saberia?", null)]
    [InlineData("Como diabos é que ela saberia?", null)]
    [InlineData("Como ela saberia?", null)]
    [InlineData("Como diabos saberia ela?", null)]
    [InlineData("Como diabos ela saber?", null)]
    [InlineData("Como diabos ela sabem?", null)]
    [InlineData("Dutton foi uma burocrata até 15 minutos atrás.", null)]
    [InlineData("Dutton foi uma burocrata até 2010.", "Dutton was a bureaucrat until 2010.")]
    [InlineData("Ela foi médica durante dez anos até há duas horas.", "She was a doctor for ten years until two hours ago.")]
    [InlineData("Ela não foi médica até há duas horas.", "She was not a doctor until two hours ago.")]
    [InlineData("Ela foi agredida até há duas horas.", "She was attacked until two hours ago.")]
    [InlineData("A reunião foi útil até há duas horas.", "The meeting was useful until two hours ago.")]
    [InlineData("Ela foi médica até duas horas.", "She was a doctor until two hours ago.")]
    [InlineData("O programa prioriza visar ao crime de rua.", null)]
    [InlineData("O programa prioriza visar ao crime de rua.", "The program aims to prioritize street crime.")]
    [InlineData("O programa prioriza visar ao crime de rua.", "The program prioritizes targeting organized crime.")]
    [InlineData("O programa prioriza visar ao crime de rua.", "The program prioritizes targeting street crime victims.")]
    [InlineData("O programa prioriza visar ao crime de rua organizado.", "The program prioritizes targeting street crime.")]
    [InlineData("O programa visa priorizar os crimes de rua.", "The program prioritizes targeting street crime.")]
    [InlineData("O programa prioriza combater os crimes de rua.", "The program prioritizes targeting street crime.")]
    [InlineData("O programa visa o mercado externo.", "The program targets foreign markets.")]
    public void IndirectQuestionsCompletedServicePassivesAndDifferentPolicyScopesAreProtected(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Como diabos ela saberia?", null)]
    [InlineData("Como diabos ela o saberia?", null)]
    [InlineData("Dutton foi uma burocrata até 15 minutos atrás.", "Dutton was a dog catcher till 15 minutes ago.")]
    [InlineData("O programa prioriza visar ao crime de rua.", "The program prioritizes targeting street crime.")]
    public void EveryWordBoundaryPreservesMeaningAndLineCount(string source, string? english)
    {
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        var reference = ReferenceHint.FromText(english);
        var expected = converter.Convert(source, reference: reference);
        for (var index = 0; index < source.Length; index++)
        {
            if (source[index] != ' ') continue;
            foreach (var newline in new[] { "\n", "\r\n" })
            {
                var trace = new List<RuleChange>();
                var result = converter.Convert(source[..index] + newline + source[(index + 1)..], reference: reference, trace: trace);
                Assert.True(expected == Regex.Replace(result, @"\s+", " "),
                    $"Wrap at {index}, newline length {newline.Length}: {result}; trace: {string.Join("; ", trace)}");
                Assert.Equal(1, result.Count(c => c == '\n'));
                Assert.Equal(result, converter.Convert(result, reference: reference));
            }
        }
    }

    [Theory]
    [InlineData("Como diabos ela saberia?", null, "natural-word-order", "saberia", "Como diabos compreenderia ela?")]
    [InlineData("Dutton foi uma burocrata até 15 minutos atrás.", "Dutton was a dog catcher till 15 minutes ago.",
        "natural-word-order", "foi", "Dutton tornou-se uma burocrata até 15 minutos atrás.")]
    [InlineData("O programa prioriza visar ao crime de rua.", "The program prioritizes targeting street crime.",
        "dialogue-vocabulary", "prioriza", "O programa prefere visar ao crime de rua.")]
    public void StageSwitchesIgnoredPhrasesAndCustomWordsRemainAuthoritative(
        string source, string? english, string stage, string word, string customExpected)
    {
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(YouToTu: false, DisabledRules: stage))
            .Convert(source, reference: reference));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add(word);
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(YouToTu: false), localWordList: words)
            .Convert(source, reference: reference));
        words.Ignored.Clear();
        words.Words.Add((word, word == "saberia" ? "compreenderia" : word == "foi" ? "tornou-se" : "prefere"));
        Assert.Equal(customExpected, new PortugueseConverter(new ConversionOptions(YouToTu: false), localWordList: words)
            .Convert(source, reference: reference));
    }

    [Fact]
    public void OptionsAndIndependentSpeakersRemainAuthoritative()
    {
        const string question = "Como diabos ela saberia?";
        foreach (var options in new[] { new ConversionOptions(Colloquial: false),
            new ConversionOptions(Pronouns: false) })
            Assert.Equal(question, new PortugueseConverter(options).Convert(question));
        const string policy = "O programa prioriza visar ao crime de rua.";
        Assert.Equal(policy, new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert(policy, reference: ReferenceHint.FromText("The program prioritizes targeting street crime.")));
        const string speakers = "-Como diabos?\n-Ela saberia.";
        Assert.Equal(speakers, new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(speakers));
    }

    [Theory]
    [InlineData("Como\nela o vê?", "Como\nela o vê?")]
    [InlineData("Como diabos\nela o saberia?", "Como diabos\no saberia ela?")]
    [InlineData("Como diabos eu vi ela?", "Como diabos eu a vi?")]
    [InlineData("Ela saberia isso.", "Ela saberia isso.")]
    [InlineData("Ela o saberia.", "Ela sabê-lo-ia.")]
    [InlineData("Ela disse.\nEla o vê.", "Ela disse.\nEla vê-o.")]
    [InlineData("Ainda não significa que\neu o deixaria ficar lá.", "Ainda não significa que\neu o deixaria ficar lá.")]
    public void SharedPronounGuardsKeepSubjectsAndObjectsDistinct(string source, string expected) =>
        Assert.Equal(expected, new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source));

    [Fact]
    public void SharedSubtitleConversionUsesAlignedPolicyAndOccupationReferences()
    {
        var blocks = new[] {
            new SrtBlock { StartMs = 1000, EndMs = 2000, Text = "-Por favor.\n-Como diabos ela saberia?" },
            new SrtBlock { StartMs = 2100, EndMs = 3000, Text = "Dutton foi uma burocrata\naté 15 minutos atrás." },
            new SrtBlock { StartMs = 3100, EndMs = 4000, Text = "prioriza visar ao\ncrime de rua..." },
        };
        var result = SubtitleConversion.Convert(blocks, ConversionOptions.Default, english: new string?[] {
            "Oh, please. How the hell would she know?", "Dutton was a dog catcher till 15 minutes ago.",
            "prioritizes targeting street crime..." });
        Assert.Equal("-Por favor.\n-Como diabos saberia ela?", result[0].Text);
        Assert.Contains("era uma burocrata", result[1].Text);
        Assert.Equal("dá prioridade ao combate\naos crimes de rua...", result[2].Text);
        Assert.Equal("prioriza visar ao\ncrime de rua...", blocks[2].Text);
        Assert.Equal(3100, blocks[2].StartMs);
        Assert.Equal(4000, blocks[2].EndMs);
    }
}
