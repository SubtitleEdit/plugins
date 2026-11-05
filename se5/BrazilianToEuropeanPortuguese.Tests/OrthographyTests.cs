using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;
using System.IO;
using System;
using System.Formats.Tar;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class OrthographyTests
{
    [Theory]
    [InlineData("A ação do diretor.", "A acção do director.")]
    [InlineData("Os atores e as atrizes.", "Os actores e as actrizes.")]
    [InlineData("Um projeto ativo.", "Um projecto activo.")]
    [InlineData("A receção do espetáculo.", "A recepção do espectáculo.")]
    [InlineData("O objeto elétrico.", "O objecto eléctrico.")]
    [InlineData("O teto está ótimo.", "O tecto está óptimo.")]
    [InlineData("A arquitetura atual.", "A arquitectura actual.")]
    [InlineData("A proteção dos espetadores.", "A protecção dos espectadores.")]
    [InlineData("AÇÃO E DIREÇÃO", "ACÇÃO E DIRECÇÃO")]
    [InlineData("<i>A ação.</i>\\N", "<i>A acção.</i>\\N")]
    [InlineData("A ideia heroica.", "A ideia heroica.")]
    [InlineData("O facto e o contacto.", "O facto e o contacto.")]
    [InlineData("Para a mãe.", "Para a mãe.")]
    [InlineData("Eles veem e leem.", "Eles vêem e lêem.")]
    public void PreAgreement(string text, string expected)
    {
        var converter = new PortugueseConverter(new ConversionOptions(PostAgreement: false));
        Assert.Equal(expected, converter.Convert(text));
    }

    [Theory]
    [InlineData("A acção do director.", "A ação do diretor.")]
    [InlineData("Um projecto activo.", "Um projeto ativo.")]
    [InlineData("O objecto eléctrico.", "O objeto elétrico.")]
    public void PostAgreement(string text, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(text));

    [Fact]
    public void DisablingSpellingDisablesAgreementConversion()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Spelling: false, PostAgreement: false));
        Assert.Equal("O projeto.", converter.Convert("O projeto."));
    }

    [Fact]
    public void IgnoredPhrasesRemainProtected()
    {
        var list = new PortugueseLocalWordList();
        list.Ignored.Add("projeto");
        Assert.Equal("O projeto.", new PortugueseConverter(new ConversionOptions(PostAgreement: false), list).Convert("O projeto."));
    }

    [Fact]
    public void SettingsRoundTripAndLegacyDefaults()
    {
        var options = new ConversionOptions(Vocabulary: false, PostAgreement: false, ValidateSpelling: true);
        Assert.Equal(options, ConversionOptions.Load(new PluginRequest
        {
            Settings = options.ToJson(), SettingsVersion = ConversionOptions.Version,
        }));
        var legacy = System.Text.Json.JsonSerializer.SerializeToElement(new { spelling = false, youToTu = false });
        Assert.Equal(new ConversionOptions(Spelling: false, YouToTu: false),
            ConversionOptions.Load(new PluginRequest { Settings = legacy, SettingsVersion = 1 }));
    }

    [Fact]
    public void DictionariesValidateAffixedWordsAndExcludeMarkup()
    {
        var after = NaturaDictionary.Load(true);
        var before = NaturaDictionary.Load(false);
        Assert.True(after.Check("ações"));
        Assert.True(before.Check("acções"));
        Assert.True(after.Check("cantaríamos"));
        Assert.Equal(new[] { "zzzzinvalidword" }, after.UnknownWords(new[] { "<i>Bom dia.</i> {\\an8}zzzzinvalidword" }));
    }

    [Fact]
    public void InvalidArchiveIsRejected()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        Assert.Throws<InvalidDataException>(() => NaturaDictionary.ReadArchive(stream));
    }

    [Fact]
    public void ArchiveWithoutLicenceIsRejected()
    {
        using var stream = new MemoryStream();
        using (var gzip = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true))
        using (var writer = new TarWriter(gzip, leaveOpen: true))
        {
            foreach (var name in new[] { "pt_PT.dic", "pt_PT.aff" })
            {
                using var data = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("dummy"));
                writer.WriteEntry(new PaxTarEntry(TarEntryType.RegularFile, "test/" + name) { DataStream = data });
            }
        }
        stream.Position = 0;
        Assert.Throws<InvalidDataException>(() => NaturaDictionary.ReadArchive(stream));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateChecksDownloadsAndRetainsDictionaryOnFailure(bool postAgreement)
    {
        var directory = Path.Combine(Path.GetTempPath(), "natura-test-" + Guid.NewGuid().ToString("N"));
        var file = Path.Combine(directory, NaturaDictionary.FileName(postAgreement));
        using var source = typeof(NaturaDictionary).Assembly.GetManifestResourceStream(
            "BrazilianToEuropeanPortuguese.Dictionaries." + NaturaDictionary.FileName(postAgreement))!;
        using var bytes = new MemoryStream();
        source.CopyTo(bytes);
        var handler = new DictionaryHandler(bytes.ToArray());
        using var client = new HttpClient(handler);
        try
        {
            Assert.Contains("up to date", await NaturaDictionary.UpdateAsync(postAgreement, false, client, directory));
            Assert.False(Directory.Exists(directory));
            Assert.True(NaturaDictionary.Load(postAgreement, directory).Check("casa"));
            Assert.Contains("downloaded and validated", await NaturaDictionary.UpdateAsync(postAgreement, true, client, directory));
            Assert.Equal(bytes.ToArray(), File.ReadAllBytes(file));
            Assert.True(NaturaDictionary.Load(postAgreement, directory).Check("casa"));
            handler.Bytes = new byte[] { 1, 2, 3, 4 };
            await Assert.ThrowsAsync<InvalidDataException>(() => NaturaDictionary.UpdateAsync(postAgreement, true, client, directory));
            Assert.Equal(bytes.ToArray(), File.ReadAllBytes(file));
            handler.Status = HttpStatusCode.ServiceUnavailable;
            await Assert.ThrowsAsync<HttpRequestException>(() => NaturaDictionary.UpdateAsync(postAgreement, true, client, directory));
            Assert.Equal(bytes.ToArray(), File.ReadAllBytes(file));
            // A corrupt local file must not prevent manual recovery.
            File.WriteAllBytes(file, handler.Bytes);
            handler.Bytes = bytes.ToArray();
            handler.Status = HttpStatusCode.OK;
            Assert.Contains("Update available", await NaturaDictionary.UpdateAsync(postAgreement, false, client, directory));
            await NaturaDictionary.UpdateAsync(postAgreement, true, client, directory);
            Assert.True(NaturaDictionary.Load(postAgreement, directory).Check("casa"));
        }
        finally
        {
            if (File.Exists(file)) File.Delete(file);
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    private sealed class DictionaryHandler(byte[] bytes) : HttpMessageHandler
    {
        public byte[] Bytes { get; set; } = bytes;
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("natura.di.uminho.pt", request.RequestUri!.Host);
            Assert.Null(request.Content);
            return Task.FromResult(new HttpResponseMessage(Status) { Content = new ByteArrayContent(Bytes) });
        }
    }

    [Fact]
    public void UnchangedBatchStillReportsValidation()
    {
        var request = new PluginRequest
        {
            RequestType = PluginRequestType.Batch,
            Settings = new ConversionOptions(ValidateSpelling: true).ToJson(),
            SettingsVersion = ConversionOptions.Version,
            Subtitle = new PluginSubtitle { SubRip = "1\n00:00:01,000 --> 00:00:02,000\nzzzzinvalidword\n" },
        };
        var response = BatchMode.BuildBatchResponse(request, Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml"));
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Contains("zzzzinvalidword", response.Message);
    }

    [Fact]
    public void BatchUsesOrthographyAndReportsUnrecognisedWords()
    {
        var request = new PluginRequest
        {
            RequestType = PluginRequestType.Batch,
            Settings = new ConversionOptions(PostAgreement: false, ValidateSpelling: true).ToJson(),
            SettingsVersion = ConversionOptions.Version,
            Subtitle = new PluginSubtitle
            {
                SubRip = "1\n00:00:01,000 --> 00:00:02,000\nO projeto. zzzzinvalidword\n",
            },
        };
        var response = BatchMode.BuildBatchResponse(request, Path.Combine(Path.GetTempPath(), System.Guid.NewGuid() + ".xml"));
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Contains("O projecto.", response.Subtitle!.Native);
        Assert.Contains("zzzzinvalidword", response.Message);
        Assert.False(response.Settings!.Value.GetProperty("postAgreement").GetBoolean());
    }
}
