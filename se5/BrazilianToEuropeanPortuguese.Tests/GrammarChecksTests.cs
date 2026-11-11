using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class GrammarChecksTests
{
    private static readonly ConversionOptions Enabled = new(GrammarCheck: true, GrammarAutoFix: true);
    private static readonly IProgress<string> Progress = new Progress<string>(_ => { });
    private static CheckLine Line(string text = "Eles tem razão.", string? english = "They are right.") =>
        new(0, text, text, null, english);
    private static GrammarMatch Match(string text, string from = "tem", string to = "têm", string rule = "AGREEMENT") =>
        new(text.IndexOf(from, StringComparison.Ordinal), from.Length, rule, "Check agreement.", new[] { to });
    private static Task<float[]> SameMeaning(string _, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(new[] { 1f, 0f });
    }

    private static async Task<(Dictionary<int, string> Texts, List<string> Notes, string Summary)> Run(
        IReadOnlyList<CheckLine> lines, ConversionOptions? options = null, PortugueseLocalWordList? words = null,
        Func<string, CancellationToken, Task<IReadOnlyList<GrammarMatch>>>? check = null,
        Func<string, CancellationToken, Task<float[]>>? embed = null)
    {
        var texts = lines.ToDictionary(l => l.Index, l => l.Converted);
        var notes = new List<string>();
        check ??= (text, _) => Task.FromResult<IReadOnlyList<GrammarMatch>>(
            text.Contains("tem", StringComparison.Ordinal) ? new[] { Match(text) } : Array.Empty<GrammarMatch>());
        var summary = await GrammarChecks.RunAsync(lines, options ?? Enabled, words, texts,
            (_, note) => notes.Add(note), check, embed, Progress, CancellationToken.None);
        return (texts, notes, summary);
    }

    [Fact]
    public async Task DefaultAndDisabledStageMakeNoRequests()
    {
        Task<IReadOnlyList<GrammarMatch>> Fail(string _, CancellationToken __) => throw new Exception("Unexpected server request.");
        foreach (var options in new[] { ConversionOptions.Default, Enabled with { DisabledRules = "languagetool" } })
        {
            var result = await Run(new[] { Line() }, options, check: Fail);
            Assert.Equal(Line().Converted, result.Texts[0]);
            Assert.Empty(result.Notes);
            Assert.Empty(result.Summary);
        }
    }

    [Fact]
    public async Task ReviewModeNeverEmbedsOrChangesText()
    {
        var result = await Run(new[] { Line() }, Enabled with { GrammarAutoFix = false },
            embed: (_, _) => throw new Exception("Review must not embed."));
        Assert.Equal("Eles tem razão.", result.Texts[0]);
        Assert.Contains("review only", Assert.Single(result.Notes));
        Assert.Contains("0 allowlisted", result.Summary);
    }

    [Theory]
    [InlineData("misspelling", "TYPOS", "spelling")]
    [InlineData("typographical", "PUNCTUATION", "typography")]
    [InlineData("whitespace", "", "typography")]
    [InlineData("style", "REDUNDANCY", "style")]
    [InlineData("register", "", "style")]
    [InlineData("grammar", "", "grammar")]
    [InlineData("", "GRAMMAR", "grammar")]
    [InlineData("", "", "other")]
    [InlineData("new-server-type", "NEW_CATEGORY", "other")]
    public async Task DisabledGroupsProduceNoAlertsOrCorrections(string issueType, string category, string group)
    {
        var match = Match("Eles tem razão.") with { IssueType = issueType, Category = category };
        Assert.Equal(group, GrammarRuleSelection.Group(match));
        var result = await Run(new[] { Line() }, Enabled with { GrammarDisabledGroups = group },
            check: (_, _) => Task.FromResult<IReadOnlyList<GrammarMatch>>(new[] { match }),
            embed: (_, _) => throw new Exception("Disabled checks must not embed."));
        Assert.Empty(result.Notes);
        Assert.Contains("0 cue(s)", result.Summary);
        Assert.Equal("Eles tem razão.", result.Texts[0]);
    }

    [Fact]
    public async Task IndividualExclusionsDoNotDisableOtherRules()
    {
        var result = await Run(new[] { Line() }, Enabled with { GrammarDisabledRules = "AGREEMENT" },
            check: (text, _) => Task.FromResult<IReadOnlyList<GrammarMatch>>(new[]
            {
                Match(text), Match(text, rule: "OTHER_RULE") with { Replacements = Array.Empty<string>() },
            }), embed: SameMeaning);
        Assert.Equal("Eles tem razão.", result.Texts[0]);
        Assert.Contains("[OTHER_RULE]", Assert.Single(result.Notes));
        Assert.DoesNotContain("[AGREEMENT]", result.Notes[0]);
    }

    [Fact]
    public async Task DisabledGroupsAlsoApplyDuringGrammarRecheck()
    {
        var result = await Run(new[] { Line() }, Enabled with { GrammarDisabledGroups = "style" },
            check: (text, _) => Task.FromResult<IReadOnlyList<GrammarMatch>>(text.Contains("tem", StringComparison.Ordinal)
                ? new[] { Match(text) with { IssueType = "grammar" } }
                : new[] { new GrammarMatch(0, 4, "WORDINESS", "Style only.", Array.Empty<string>(), "style") }),
            embed: SameMeaning);
        Assert.Equal("Eles têm razão.", result.Texts[0]);
        Assert.Contains("corrected", Assert.Single(result.Notes));
    }

    [Fact]
    public void GrammarSelectionsRoundTripAndLegacySettingsKeepAllGroups()
    {
        var options = Enabled with { GrammarDisabledRules = "DASH_ENUMERATION_SPACE_RULE,MORFOLOGIK_RULE_PT_PT", GrammarDisabledGroups = "spelling,style,typography" };
        Assert.Equal(options, ConversionOptions.Load(new PluginRequest { SettingsVersion = 1, Settings = options.ToJson() }));
        var legacy = ConversionOptions.Load(new PluginRequest { SettingsVersion = 1, Settings = JsonSerializer.SerializeToElement(new { grammarCheck = true }) });
        Assert.Empty(legacy.GrammarDisabledRules);
        Assert.Empty(legacy.GrammarDisabledGroups);
        Assert.Equal("A,B", GrammarRuleSelection.Validate("B,\nA,A"));
        Assert.Equal(new[] { "A", "B" }, GrammarRuleSelection.ObservedIds(new[] { "LanguageTool [B]: test\nLanguageTool [A]: test", null }));
    }

    [Theory]
    [InlineData("{\"grammarDisabledRules\":false}")]
    [InlineData("{\"grammarDisabledRules\":\"BAD ID\"}")]
    [InlineData("{\"grammarDisabledRules\":\"A&text=secret\"}")]
    [InlineData("{\"grammarDisabledGroups\":\"unknown\"}")]
    [InlineData("{\"grammarDisabledGroups\":[]}")]
    public void InvalidGrammarSelectionsFailExplicitly(string json) =>
        Assert.Throws<InvalidDataException>(() => ConversionOptions.Load(new PluginRequest
        { SettingsVersion = 1, Settings = JsonSerializer.Deserialize<JsonElement>(json) }));

    [Fact]
    public async Task AutomaticAgreementNeedsModelAndRechecksGrammar()
    {
        var missing = await Run(new[] { Line() });
        Assert.Equal("Eles tem razão.", missing.Texts[0]);
        Assert.Contains("requires enabled, installed MiniLM", Assert.Single(missing.Notes));
        var requests = new List<string>();
        var accepted = await Run(new[] { Line() }, embed: SameMeaning, check: (text, _) =>
        {
            requests.Add(text);
            return Task.FromResult<IReadOnlyList<GrammarMatch>>(text.Contains("tem", StringComparison.Ordinal)
                ? new[] { Match(text) } : Array.Empty<GrammarMatch>());
        });
        Assert.Equal("Eles têm razão.", accepted.Texts[0]);
        Assert.Equal(new[] { "Eles tem razão.", "Eles têm razão." }, requests);
        Assert.Contains("corrected", Assert.Single(accepted.Notes));
    }

    [Theory]
    [InlineData("Eles tem razão.", "Eles têm razão.", "They are right.", true)]
    [InlineData("Elas vem aqui.", "Elas vêm aqui.", "They come here.", true)]
    [InlineData("Ele têm razão.", "Ele tem razão.", "He is right.", true)]
    [InlineData("Ela vêm aqui.", "Ela vem aqui.", "She comes here.", true)]
    [InlineData("ELES TEM RAZÃO.", "ELES TÊM RAZÃO.", "They are right.", true)]
    [InlineData("Eles não tem razão.", "Eles não têm razão.", "They are not right.", true)]
    [InlineData("Eles tem razão.", "Eles têm razão.", "He is right.", false)]
    [InlineData("Eles tem razão.", "Eles têm razão.", null, false)]
    [InlineData("Eles tem razão.", "Eles têm razão.", "Right.", false)]
    [InlineData("Tu tem razão.", "Tu tens razão.", "You are right.", false)]
    [InlineData("A equipa têm razão.", "A equipa tem razão.", "The team is right.", false)]
    [InlineData("Eles tem razão.", "Eles tinham razão.", "They were right.", false)]
    [InlineData("Ela falarei nisto.", "Ela falaria nisto.", "She would talk about this.", false)]
    [InlineData("\"Eles tem razão.\"", "\"Eles têm razão.\"", "They are right.", false)]
    [InlineData("'Eles tem razão.'", "'Eles têm razão.'", "They are right.", false)]
    [InlineData("-Eles tem razão.", "-Eles têm razão.", "They are right.", false)]
    [InlineData("<i>Eles tem razão.</i>", "<i>Eles têm razão.</i>", "They are right.", false)]
    public void AllowlistIsBounded(string before, string after, string? english, bool expected) =>
        Assert.Equal(expected, GrammarChecks.Eligible(before, after, english, Enabled));

    [Fact]
    public void DisabledAgreementSettingsRemainAuthoritative()
    {
        Assert.False(GrammarChecks.Eligible("Eles tem razão.", "Eles têm razão.", "They are right.", Enabled with { YouToTu = false }));
        Assert.False(GrammarChecks.Eligible("Eles tem razão.", "Eles têm razão.", "They are right.", Enabled with { DisabledRules = "address" }));
    }

    [Fact]
    public async Task UnlistedVocabularyAndTenseAreSuggestionsOnlyEvenWithPerfectSimilarity()
    {
        var line = Line("Ela falarei nisto.", "She would talk about this.");
        var result = await Run(new[] { line }, embed: SameMeaning, check: (text, _) =>
            Task.FromResult<IReadOnlyList<GrammarMatch>>(new[] { Match(text, "falarei", "falaria") }));
        Assert.Equal(line.Converted, result.Texts[0]);
        Assert.Contains("not a unique allowlisted", Assert.Single(result.Notes));
    }

    [Fact]
    public async Task SafeSentenceGroupPreservesCueBoundariesAndInternalNewlines()
    {
        var sentence = new SentenceMeaning(0, 1, "Eles tem razão, de facto.", "They are right, in fact.");
        var lines = new[]
        {
            Line("Eles tem\nrazão,", "They are right,") with { MeaningSentence = sentence },
            new CheckLine(1, "de facto.", "de facto.", null, "in fact.", sentence),
        };
        var result = await Run(lines, embed: SameMeaning);
        Assert.Equal("Eles têm\nrazão,", result.Texts[0]);
        Assert.Equal("de facto.", result.Texts[1]);
        Assert.Equal(2, result.Notes.Count);
    }

    [Fact]
    public async Task IncompleteSelectedGroupDoesNotReadUnselectedContext()
    {
        var sentence = new SentenceMeaning(0, 1, "Eles tem razão.", "They are right.");
        var seen = new List<string>();
        var line = Line("razão.", "right.") with { Index = 1, MeaningSentence = sentence };
        await Run(new[] { line }, check: (text, _) =>
        {
            seen.Add(text);
            return Task.FromResult<IReadOnlyList<GrammarMatch>>(Array.Empty<GrammarMatch>());
        });
        Assert.Equal(new[] { "razão." }, seen);
    }

    [Fact]
    public async Task SubjectInPreviousSafeCueCanConfirmAgreementWithoutMovingWords()
    {
        var sentence = new SentenceMeaning(0, 1, "Eles tem razão.", "They are right.");
        var lines = new[]
        {
            Line("Eles", "They") with { MeaningSentence = sentence },
            new CheckLine(1, "tem razão.", "tem razão.", null, "are right.", sentence),
        };
        var result = await Run(lines, embed: SameMeaning);
        Assert.Equal("Eles", result.Texts[0]);
        Assert.Equal("têm razão.", result.Texts[1]);
    }

    [Theory]
    [InlineData("Eles tem razão.", "Eles têm razão.")]
    [InlineData("Eles\ntem razão.", "Eles\ntêm razão.")]
    public async Task LanguageToolPhraseReplacementsPreserveWhitespaceAndRejectSubjectChanges(string before, string expected)
    {
        var result = await Run(new[] { Line(before) }, embed: SameMeaning, check: (text, _) =>
            Task.FromResult<IReadOnlyList<GrammarMatch>>(text.Contains("tem", StringComparison.Ordinal)
                ? new[] { new GrammarMatch(0, text.IndexOf("tem", StringComparison.Ordinal) + 3, "GENERAL_VERB_AGREEMENT_ERRORS",
                    "Concordância.", new[] { "Eles têm", "Ele tem" }) } : Array.Empty<GrammarMatch>()));
        Assert.Equal(expected, result.Texts[0]);
    }

    [Fact]
    public async Task ActualPhraseReplacementMaySpanSafeCueBoundaryButOnlyVerbChanges()
    {
        var sentence = new SentenceMeaning(0, 1, "Eles tem razão.", "They are right.");
        var lines = new[]
        {
            Line("Eles", "They") with { MeaningSentence = sentence },
            new CheckLine(1, "tem razão.", "tem razão.", null, "are right.", sentence),
        };
        var result = await Run(lines, embed: SameMeaning, check: (text, _) =>
            Task.FromResult<IReadOnlyList<GrammarMatch>>(text.Contains("tem", StringComparison.Ordinal)
                ? new[] { new GrammarMatch(0, 8, "GENERAL_VERB_AGREEMENT_ERRORS", "Concordância.",
                    new[] { "Eles têm", "Ele tem" }) } : Array.Empty<GrammarMatch>()));
        Assert.Equal("Eles", result.Texts[0]);
        Assert.Equal("têm razão.", result.Texts[1]);
    }

    [Fact]
    public async Task LowSimilarityWithholdsCandidateBeforeGrammarRecheck()
    {
        var requests = 0;
        var result = await Run(new[] { Line() }, check: (text, _) =>
        {
            requests++;
            return Task.FromResult<IReadOnlyList<GrammarMatch>>(new[] { Match(text) });
        }, embed: (text, _) => Task.FromResult(text.Contains("têm", StringComparison.Ordinal)
            ? new[] { 0f, 1f } : new[] { 1f, 0f }));
        Assert.Equal(1, requests);
        Assert.Equal("Eles tem razão.", result.Texts[0]);
        Assert.Contains("meaning safeguard rejected", Assert.Single(result.Notes));
    }

    [Fact]
    public async Task IgnoredAndCustomWordsCannotBeChangedOrUsedAsAgreementEvidence()
    {
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("Eles");
        var ignored = await Run(new[] { Line() }, words: words, embed: SameMeaning);
        Assert.Equal("Eles tem razão.", ignored.Texts[0]);
        words.Ignored.Clear();
        words.Words.Add(("custom", "tem"));
        var custom = await Run(new[] { Line() }, words: words, embed: SameMeaning,
            check: (_, _) => Task.FromResult<IReadOnlyList<GrammarMatch>>(new[] { new GrammarMatch(5, 3, "TEST", "Protected.", new[] { "têm" }) }));
        Assert.Equal("Eles tem razão.", custom.Texts[0]);
        Assert.Empty(custom.Notes);
        var original = "<i>😀 Eles tem</i>\n{\\an8}razão.";
        var masked = GrammarChecks.Mask(original, words);
        Assert.Equal(original.Length, masked.Length);
        Assert.DoesNotContain("tem", masked);
        Assert.DoesNotContain("<i>", masked);
    }

    [Fact]
    public async Task OverlappingDiagnosticsRemainReviewOnly()
    {
        var result = await Run(new[] { Line() }, embed: SameMeaning, check: (text, _) =>
            Task.FromResult<IReadOnlyList<GrammarMatch>>(new[] { Match(text), Match(text) with { Rule = "OTHER" } }));
        Assert.Equal(Line().Converted, result.Texts[0]);
        Assert.Equal(2, result.Notes.Count);
    }

    [Fact]
    public async Task InsertionAndNoReplacementFindingsRemainVisibleForReview()
    {
        var result = await Run(new[] { Line() }, check: (_, _) =>
            Task.FromResult<IReadOnlyList<GrammarMatch>>(new[]
            {
                new GrammarMatch(4, 0, "INSERTION", "Missing punctuation.", new[] { "," }),
                new GrammarMatch(9, 5, "REVIEW", "Check this construction.", Array.Empty<string>()),
            }));
        Assert.Equal(2, result.Notes.Count);
        Assert.Contains(result.Notes, n => n.Contains("INSERTION"));
        Assert.Contains(result.Notes, n => n.Contains("no replacement supplied"));
        Assert.Equal(Line().Converted, result.Texts[0]);
    }

    [Fact]
    public async Task OnlyTheFixedDiagnosticIsLabelledCorrected()
    {
        var calls = 0;
        var result = await Run(new[] { Line() }, embed: SameMeaning, check: (text, _) =>
        {
            calls++;
            var other = new GrammarMatch(9, 5, "AGREEMENT", "Review this word.", Array.Empty<string>());
            return Task.FromResult<IReadOnlyList<GrammarMatch>>(calls == 1 ? new[] { Match(text), other } : new[] { other });
        });
        Assert.Equal("Eles têm razão.", result.Texts[0]);
        Assert.Single(result.Notes, n => n.Contains("; corrected"));
        Assert.Single(result.Notes, n => n.Contains("; review (not automatically changed)"));
    }

    [Fact]
    public async Task NewOrUnresolvedDiagnosticsRejectCandidate()
    {
        foreach (var unresolved in new[] { true, false })
        {
            var calls = 0;
            var result = await Run(new[] { Line() }, embed: SameMeaning, check: (text, _) =>
            {
                calls++;
                return Task.FromResult<IReadOnlyList<GrammarMatch>>(calls == 1 ? new[] { Match(text) } :
                    new[] { new GrammarMatch(0, 4, unresolved ? "AGREEMENT" : "NEW", "New finding.", Array.Empty<string>()) });
            });
            Assert.Equal(2, calls);
            Assert.Equal(Line().Converted, result.Texts[0]);
            Assert.Contains("recheck", Assert.Single(result.Notes));
        }
    }

    [Theory]
    [InlineData(.8f, .8f, .8f, true)]
    [InlineData(.7001f, .8f, .8f, true)]
    [InlineData(.699f, .8f, .8f, false)]
    [InlineData(.8f, .8f, .759f, false)]
    [InlineData(.8f, .5f, .449f, false)]
    [InlineData(.8f, .4f, .8f, false)]
    [InlineData(float.NaN, .8f, .8f, false)]
    public async Task MeaningGateUsesOriginalCandidateAndEnglish(float similarity, float beforeEnglish, float afterEnglish, bool expected)
    {
        Task<float[]> Embed(string text, CancellationToken _) => Task.FromResult(text switch
        {
            "before" => new[] { 1f, 0f, 0f },
            "after" => new[] { similarity, 1f, 0f },
            _ => new[] { beforeEnglish, afterEnglish - similarity * beforeEnglish, 0f },
        });
        Assert.Equal(expected, await GrammarChecks.MeaningSafeAsync("before", "after", "english", Embed, CancellationToken.None));
    }

    [Fact]
    public async Task EmbeddingErrorsPropagateAndNeverApplyChanges()
    {
        await Assert.ThrowsAsync<IOException>(() => Run(new[] { Line() }, embed: (_, _) => throw new IOException("model failed")));
    }

    [Fact]
    public void OptionsRoundTripAndOlderSettingsStayOff()
    {
        var options = Enabled with { GrammarServer = "http://languagetool:8010/" };
        var request = new PluginRequest { Settings = options.ToJson(), SettingsVersion = ConversionOptions.Version };
        Assert.Equal(options, ConversionOptions.Load(request));
        request.Settings = JsonSerializer.SerializeToElement(new { spelling = true });
        Assert.False(ConversionOptions.Load(request).GrammarCheck);
        Assert.False(ConversionOptions.Load(request).GrammarAutoFix);
        request.Settings = JsonSerializer.SerializeToElement(new { grammarCheck = "yes" });
        Assert.Throws<InvalidDataException>(() => ConversionOptions.Load(request));
    }

    private static string Response(string text, params GrammarMatch[] matches) => JsonSerializer.Serialize(new
    {
        language = new { code = "pt-PT" },
        matches = matches.Select(m => new
        {
            offset = m.Offset, length = m.Length, message = m.Message,
            rule = new { id = m.Rule, issueType = m.IssueType, category = new { id = m.Category } },
            replacements = m.Replacements.Select(value => new { value }),
        }),
    });

    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            send(request, cancellationToken);
    }

    [Fact]
    public async Task ClientSendsRuleExclusionsAndPreservesClassification()
    {
        using var client = new LanguageToolClient("http://localhost:8081/", new Handler(async (request, token) =>
        {
            var form = Uri.UnescapeDataString(await request.Content!.ReadAsStringAsync(token));
            Assert.Contains("disabledRules=DASH_ENUMERATION_SPACE_RULE,MORFOLOGIK_RULE_PT_PT", form);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Response("Eles tem razão.", Match("Eles tem razão.") with { IssueType = "grammar", Category = "GRAMMAR" })),
            };
        }), "MORFOLOGIK_RULE_PT_PT,DASH_ENUMERATION_SPACE_RULE");
        var match = Assert.Single(await client.CheckAsync("Eles tem razão.", CancellationToken.None));
        Assert.Equal("grammar", match.IssueType);
        Assert.Equal("GRAMMAR", match.Category);
    }

    [Theory]
    [InlineData("http://127.0.0.1:8081/", "http://127.0.0.1:8081/v2/check")]
    [InlineData("http://languagetool:8010/v2", "http://languagetool:8010/v2/check")]
    [InlineData("https://grammar.local/v2/check", "https://grammar.local/v2/check")]
    public void ServerUrlSupportsLocalDockerAndHttps(string server, string expected) =>
        Assert.Equal(expected, LanguageToolClient.CheckUri(server).AbsoluteUri);

    [Theory]
    [InlineData("")]
    [InlineData("file:///tmp/grammar")]
    [InlineData("http://user:password@localhost/")]
    [InlineData("http://localhost/?text=hello")]
    [InlineData("http://localhost/#fragment")]
    [InlineData("https://api.languagetool.org")]
    [InlineData("https://api.languagetoolplus.com/v2")]
    public void InvalidAndCloudUrlsAreRejected(string server) =>
        Assert.Throws<InvalidDataException>(() => LanguageToolClient.CheckUri(server));

    [Fact]
    public async Task ClientPostsPtPtCachesWithinRunAndUsesUtf16Offsets()
    {
        var calls = 0;
        const string text = "😀 Eles tem razão.";
        using var client = new LanguageToolClient("http://localhost:8081/", new Handler(async (request, token) =>
        {
            calls++;
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/v2/check", request.RequestUri!.AbsolutePath);
            var form = await request.Content!.ReadAsStringAsync(token);
            Assert.Contains("language=pt-PT", form);
            Assert.Contains("text=", form);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Response(text, Match(text))) };
        }));
        var first = await client.CheckAsync(text, CancellationToken.None);
        Assert.Equal(8, Assert.Single(first).Offset);
        Assert.Same(first, await client.CheckAsync(text, CancellationToken.None));
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("not json")]
    [InlineData("{\"language\":{\"code\":\"pt-BR\"},\"matches\":[]}")]
    [InlineData("{\"language\":{\"code\":\"pt-PT\"},\"warnings\":{\"incompleteResults\":true},\"matches\":[]}")]
    [InlineData("{\"language\":{\"code\":\"pt-PT\"},\"warnings\":{\"incompleteResults\":\"true\"},\"matches\":[]}")]
    [InlineData("{\"language\":{\"code\":\"pt-PT\"},\"matches\":[{\"offset\":999,\"length\":3}]}")]
    [InlineData("{\"language\":{\"code\":\"pt-PT\"},\"matches\":[{\"offset\":\"0\",\"length\":3}]}")]
    public async Task InvalidResponsesFailExplicitly(string body)
    {
        using var client = new LanguageToolClient("http://localhost:8081/", new Handler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) })));
        await Assert.ThrowsAsync<InvalidDataException>(() => client.CheckAsync("Eles tem razão.", CancellationToken.None));
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Redirect)]
    public async Task HttpFailuresAreNotSuccessShaped(HttpStatusCode status)
    {
        using var client = new LanguageToolClient("http://localhost:8081/", new Handler((_, _) =>
            Task.FromResult(new HttpResponseMessage(status))));
        await Assert.ThrowsAsync<HttpRequestException>(() => client.CheckAsync("Eles tem razão.", CancellationToken.None));
    }

    [Fact]
    public async Task TimeoutAndUserCancellationAreDistinct()
    {
        using var client = new LanguageToolClient("http://localhost:8081/", new Handler((_, _) => throw new TaskCanceledException()));
        Assert.Contains("timed out", (await Assert.ThrowsAsync<IOException>(() => client.CheckAsync("text", CancellationToken.None))).Message);
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.CheckAsync("text", cancel.Token));
    }

    [Theory]
    [InlineData("", "", 1)]
    [InlineData("TEST", "", 0)]
    [InlineData("", "style", 0)]
    public async Task BatchUsesSelfHostedProtocolAndPreservesTimingsAndTextInReviewMode(string disabledRules, string disabledGroups, int findings)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var server = Task.Run(async () =>
        {
            using var socket = await listener.AcceptTcpClientAsync(limit.Token);
            await using var stream = socket.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
            Assert.StartsWith("POST /v2/check ", await reader.ReadLineAsync(limit.Token));
            var length = 0;
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync(limit.Token)))
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) length = int.Parse(line.Split(':')[1]);
            var content = new char[length];
            Assert.Equal(length, await reader.ReadBlockAsync(content.AsMemory(), limit.Token));
            Assert.Contains("language=pt-PT", new string(content));
            Assert.Contains("disabledRules=" + disabledRules, new string(content));
            var body = Encoding.UTF8.GetBytes(Response("Olá, mundo.", new GrammarMatch(0, 3, "TEST", "Test suggestion.", new[] { "Bom dia" }, "style")));
            var header = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(header, limit.Token);
            await stream.WriteAsync(body, limit.Token);
        }, limit.Token);
        var request = new PluginRequest
        {
            RequestType = PluginRequestType.Batch,
            Subtitle = new PluginSubtitle { SubRip = "1\n00:00:01,234 --> 00:00:03,456\nOlá, mundo.\n\n" },
            SettingsVersion = ConversionOptions.Version,
            Settings = new ConversionOptions(GrammarCheck: true, GrammarServer: $"http://127.0.0.1:{port}/",
                GrammarExternalServer: true, GrammarDisabledRules: disabledRules, GrammarDisabledGroups: disabledGroups).ToJson(),
        };
        var result = await Task.Run(() => BatchMode.BuildBatchResponse(request, Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml"),
            new EmbeddingSettings(CheckMeaning: false)), limit.Token);
        await server;
        Assert.Equal(PluginStatus.Ok, result.Status);
        Assert.Null(result.Subtitle);
        Assert.Contains($"{findings} cue(s) with findings", result.Message);
        Assert.Contains("0 allowlisted", result.Message);
        Assert.Equal("1\n00:00:01,234 --> 00:00:03,456\nOlá, mundo.\n\n", request.Subtitle.SubRip);
    }

    [Fact]
    public void BatchReportsServerFailureAndRetainsRuleOutput()
    {
        var request = new PluginRequest
        {
            RequestType = PluginRequestType.Batch,
            Subtitle = new PluginSubtitle { SubRip = "1\n00:00:01,234 --> 00:00:03,456\nPegue o ônibus.\n\n" },
            SettingsVersion = ConversionOptions.Version,
            Settings = new ConversionOptions(GrammarCheck: true, GrammarServer: "http://127.0.0.1:0/", GrammarExternalServer: true).ToJson(),
        };
        var response = BatchMode.BuildBatchResponse(request, Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml"),
            new EmbeddingSettings(CheckMeaning: false));
        Assert.Contains("Optional checks failed; rule-only output retained", response.Message);
        Assert.Contains("LanguageTool", response.Message);
        Assert.Equal(PluginStatus.Ok, response.Status);
        var output = Assert.Single(SubRipParser.Parse(response.Subtitle!.Native));
        Assert.Contains("autocarro", output.Text);
        Assert.Equal(1234, output.StartMs);
        Assert.Equal(3456, output.EndMs);
    }
}
