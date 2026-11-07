using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ConverterTests
{
    private static readonly PortugueseConverter Converter = new();

    private static void AssertConverts(string br, string pt) => Assert.Equal(pt, Converter.Convert(br));

    [Theory]
    [InlineData("Uma sacola nova.", "Um saco novo.")]
    [InlineData("As sacolas vazias.", "Os sacos vazios.")]
    [InlineData("Os shorts novos.", "Os calções novos.")]
    [InlineData("O aipim cozido.", "A mandioca cozida.")]
    [InlineData("Um misto-quente.", "Uma tosta mista.")]
    [InlineData("Os mistos-quentes.", "As tostas mistas.")]
    [InlineData("Uma chave de fenda.", "Uma chave de fendas.")]
    [InlineData("As chaves de fenda.", "As chaves de fendas.")]
    [InlineData("A arruela está gasta.", "A anilha está gasta.")]
    [InlineData("As arruelas novas.", "As anilhas novas.")]
    [InlineData("Uma furadeira nova.", "Um berbequim novo.")]
    [InlineData("As furadeiras novas.", "Os berbequins novos.")]
    [InlineData("A embreagem.", "A embraiagem.")]
    [InlineData("As embreagens.", "As embraiagens.")]
    [InlineData("Uma porca borboleta.", "Uma porca de orelhas.")]
    [InlineData("As porcas borboleta.", "As porcas de orelhas.")]
    [InlineData("As porcas borboletas.", "As porcas de orelhas.")]
    [InlineData("O torque de aperto.", "O binário de aperto.")]
    [InlineData("Os torques de aperto.", "Os binários de aperto.")]
    [InlineData("Eu rosqueei a tampa.", "Eu enrosquei a tampa.")]
    [InlineData("Ela rosqueava a tampa.", "Ela enroscava a tampa.")]
    [InlineData("Para rosquear a tampa.", "Para enroscar a tampa.")]
    [InlineData("Ele parafusou a peça.", "Ele aparafusou a peça.")]
    [InlineData("Ela desparafusou a tampa.", "Ela desaparafusou a tampa.")]
    [InlineData("Vou parafusá-lo.", "Vou aparafusá-lo.")]
    [InlineData("Ele desengraxou a superfície.", "Ele desengordurou a superfície.")]
    [InlineData("Ela destravou a porta.", "Ela desbloqueou a porta.")]
    [InlineData("Ele afrouxou a porca.", "Ele desapertou a porca.")]
    [InlineData("Ela engraxou o mecanismo.", "Ela lubrificou o mecanismo.")]
    [InlineData("Eu digitei o código.", "Eu introduzi o código.")]
    [InlineData("Eu digito o código.", "Eu introduzo o código.")]
    [InlineData("Tu digitas o código.", "Tu introduzes o código.")]
    [InlineData("Eles digitam dados.", "Eles introduzem dados.")]
    [InlineData("Para digitar a palavra-passe.", "Para introduzir a palavra-passe.")]
    [InlineData("Ele digita a senha.", "Ele introduz a palavra-passe.")]
    [InlineData("Eu subi um arquivo.", "Eu carreguei um ficheiro.")]
    [InlineData("Eu subo o arquivo.", "Eu carrego o ficheiro.")]
    [InlineData("Ela sobe os arquivos.", "Ela carrega os ficheiros.")]
    [InlineData("Eles sobem documentos para a plataforma.", "Eles carregam documentos para a plataforma.")]
    [InlineData("Tu sobes o ficheiro.", "Tu carregas o ficheiro.")]
    [InlineData("Ela subiu o documento para o servidor.", "Ela carregou o documento para o servidor.")]
    [InlineData("Vou subir o ficheiro.", "Vou carregar o ficheiro.")]
    [InlineData("Ela estava subindo o arquivo.", "Ela estava a carregar o ficheiro.")]
    [InlineData("Vou baixar o documento.", "Vou descarregar o documento.")]
    [InlineData("Ela salvou o ficheiro.", "Ela guardou o ficheiro.")]
    [InlineData("<i>Ela subiu\num arquivo.</i>", "<i>Ela carregou\num ficheiro.</i>")]
    public void VocabularyVerbWorkbookAddsReviewedMissingSenses(string source, string expected)
    {
        AssertConverts(source, expected);
        AssertConverts(expected, expected);
    }

    [Theory]
    [InlineData("Ele engraxou os sapatos.")]
    [InlineData("Ela afrouxou a vigilância.")]
    [InlineData("Ele subiu a escada.")]
    [InlineData("Ele subiu os documentos para o sótão.")]
    [InlineData("Ela subiu o documento.")]
    [InlineData("Ela sobe ao palco.")]
    [InlineData("Os preços sobem.")]
    [InlineData("Ele salvou a criança.")]
    [InlineData("Ela baixou a cabeça.")]
    [InlineData("O grampo de cabelo.")]
    [InlineData("Uma prova de amor.")]
    [InlineData("O boleto.")]
    [InlineData("A vedação do terreno.")]
    [InlineData("Ela confere autoridade.")]
    [InlineData("Ele travou a batalha.")]
    [InlineData("A fila está longa.")]
    [InlineData("A matéria.")]
    [InlineData("A mira telescópica.")]
    [InlineData("A matriz de mira.")]
    [InlineData("A torreta de ajuste.")]
    [InlineData("O torquímetro.")]
    [InlineData("Ela digita cartas.")]
    [InlineData("A pressão de calibragem dos pneus.")]
    public void VocabularyVerbWorkbookPreservesOtherSensesAndSharedPortuguese(string text) => AssertConverts(text, text);

    [Theory]
    [InlineData("Uma sacola nova.")]
    [InlineData("Ela destravou a porta.")]
    [InlineData("Ela engraxou o mecanismo.")]
    [InlineData("Eu digitei o código.")]
    [InlineData("Ela subiu um ficheiro.")]
    public void VocabularyVerbWorkbookRespectsVocabularySwitch(string source) =>
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));

    [Fact]
    public void UploadVerbRespectsConstructionStageAndReportsTrace()
    {
        const string source = "Ela subiu o ficheiro.";
        var trace = new List<RuleChange>();
        Assert.Equal("Ela carregou o ficheiro.", Converter.Convert(source, trace: trace));
        Assert.Contains(trace, r => r.Id == "construction-vocabulary");
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "construction-vocabulary")).Convert(source));
    }

    [Theory]
    [InlineData("porque você sempre sabe\nquando eu tô mentindo.", "porque sabes sempre\nquando estou a mentir.")]
    [InlineData("<i>porque você sempre sabe\nquando eu tô mentindo.</i>", "<i>porque sabes sempre\nquando estou a mentir.</i>")]
    [InlineData("Tu sempre sabes quando eu estou mentindo.", "Tu sabes sempre quando estou a mentir.")]
    [InlineData("Ela sempre chega cedo.", "Ela chega sempre cedo.")]
    [InlineData("Eles sempre trabalham aqui.", "Eles trabalham sempre aqui.")]
    [InlineData("Sempre encontro uma solução.", "Encontro sempre uma solução.")]
    [InlineData("Porque sempre soubeste.", "Porque soubeste sempre.")]
    [InlineData("SEMPRE CHEGO CEDO.", "CHEGO SEMPRE CEDO.")]
    [InlineData("porque SEMPRE SABES.", "porque SABES SEMPRE.")]
    [InlineData("Porque sempre chegavam cedo.", "Porque chegavam sempre cedo.")]
    [InlineData("Eles sempre trabalharão aqui.", "Eles trabalharão sempre aqui.")]
    [InlineData("Quando eu estou a trabalhar.", "Quando estou a trabalhar.")]
    [InlineData("Se eu estou a mentir.", "Se estou a mentir.")]
    [InlineData("Ele sabe quando eu estou a estudar.", "Ele sabe quando estou a estudar.")]
    public void NaturalHabitualWordOrderAndRecoverableProgressiveSubjects(string source, string expected)
    {
        AssertConverts(source, expected);
        AssertConverts(expected, expected);
    }

    [Theory]
    [InlineData("Nem sempre sabes.")]
    [InlineData("Quase sempre trabalho.")]
    [InlineData("Para sempre trabalho.")]
    [InlineData("Como sempre digo.")]
    [InlineData("Sempre que chego.")]
    [InlineData("Sempre posso ajudar.")]
    [InlineData("Sempre estou a trabalhar.")]
    [InlineData("Sempre me dizem isso.")]
    [InlineData("Sempre fazer o melhor.")]
    [InlineData("Quando eu estava a trabalhar.")]
    [InlineData("Quando eu também estou a trabalhar.")]
    [InlineData("Quando só eu estou a trabalhar.")]
    [InlineData("Quando eu estou a trabalhar e tu estás a dormir.")]
    public void NaturalWordOrderPreservesScopeCliticsAndSubjectContrast(string text) => AssertConverts(text, text);

    [Fact]
    public void NaturalWordOrderRespectsColloquialAndStageSwitches()
    {
        const string text = "porque sempre sabes quando eu estou a mentir.";
        Assert.Equal(text, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(text));
        Assert.Equal(text, new PortugueseConverter(new ConversionOptions(DisabledRules: "natural-word-order")).Convert(text));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ScreenshotTellThingConvertsWithBothOrthographies(bool postAgreement)
    {
        const string source = "Você não lembra de mim,\nmas vou te contar uma parada";
        const string english = "You don't remember me, but I have something to tell you";
        var converter = new PortugueseConverter(new ConversionOptions(PostAgreement: postAgreement, ValidateSpelling: true));
        var rules = new List<RuleChange>();
        var result = converter.Convert(source, null, ReferenceHint.FromText(english), rules);
        Assert.Equal("Não te lembras de mim,\nmas vou contar-te uma coisa", result);
        Assert.Contains(rules, r => r.Id == "reference-idioms");
        Assert.Contains(rules, r => r.Id == "pronouns");
    }

    [Theory]
    [InlineData("Vou te contar uma parada.", "Vou contar-te uma coisa.")]
    [InlineData("Vou contar-te uma parada.", "Vou contar-te uma coisa.")]
    [InlineData("Vou-te contar uma parada.", "Vou-te contar uma coisa.")]
    [InlineData("Mas vou contar uma parada.", "Mas vou contar uma coisa.")]
    [InlineData("Não vou te contar uma parada.", "Não vou contar-te uma coisa.")]
    [InlineData("Contar-lhe uma parada.", "Contar-lhe uma coisa.")]
    public void TellThingComposesWithCliticPlacement(string source, string expected) => AssertConverts(source, expected);

    [Theory]
    [InlineData("O holerite chegou.", "O recibo de vencimento chegou.")]
    [InlineData("Os holerites chegaram.", "Os recibos de vencimento chegaram.")]
    [InlineData("Um trem-bala novo.", "Um comboio de alta velocidade novo.")]
    [InlineData("Os trens-bala.", "Os comboios de alta velocidade.")]
    [InlineData("Os trens-balas.", "Os comboios de alta velocidade.")]
    [InlineData("Um suco de caixinha.", "Um sumo de pacote.")]
    [InlineData("Os sucos de caixinha.", "Os sumos de pacote.")]
    [InlineData("O computador de mesa.", "O computador de secretária.")]
    [InlineData("Os computadores de mesa.", "Os computadores de secretária.")]
    [InlineData("Uma mensagem inbox.", "Uma mensagem privada.")]
    [InlineData("As mensagens inbox.", "As mensagens privadas.")]
    [InlineData("Uma captura de tela.", "Uma captura de ecrã.")]
    [InlineData("As capturas de tela.", "As capturas de ecrã.")]
    [InlineData("Um pão francês.", "Um papo-seco.")]
    [InlineData("Os pães franceses.", "Os papos-secos.")]
    [InlineData("Um escanteio.", "Um canto.")]
    [InlineData("Os escanteios.", "Os cantos.")]
    [InlineData("O zagueiro chegou.", "O defesa-central chegou.")]
    [InlineData("Os zagueiros chegaram.", "Os defesas-centrais chegaram.")]
    [InlineData("A zagueira chegou.", "A defesa-central chegou.")]
    [InlineData("As zagueiras chegaram.", "As defesas-centrais chegaram.")]
    [InlineData("Uma mexerica madura.", "Uma tangerina madura.")]
    [InlineData("As mexericas maduras.", "As tangerinas maduras.")]
    [InlineData("Um suco de mamão.", "Um sumo de papaia.")]
    public void AdditionalWorkbookVocabulary(string source, string expected)
    {
        AssertConverts(source, expected);
        Assert.Equal(expected, Converter.Convert(expected));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
    }

    [Theory]
    [InlineData("Me conta uma parada.", "Conta-me uma coisa.")]
    [InlineData("Ela contou uma parada.", "Ela contou uma coisa.")]
    [InlineData("Contaram umas paradas.", "Contaram umas coisas.")]
    [InlineData("Ela puxou assunto.", "Ela meteu conversa.")]
    [InlineData("Eles puxavam assunto.", "Eles metiam conversa.")]
    [InlineData("Vou puxar assunto.", "Vou meter conversa.")]
    [InlineData("Ela estava puxando assunto.", "Ela estava a meter conversa.")]
    [InlineData("Eles jogaram conversa fora.", "Eles falaram.")]
    [InlineData("Vou jogar conversa fora.", "Vou falar.")]
    [InlineData("Ele está enchendo linguiça.", "Ele está a encher chouriços.")]
    [InlineData("Ela encheu linguiça.", "Ela encheu chouriços.")]
    [InlineData("Dá uma força!", "Dá uma ajuda!")]
    [InlineData("Ele deu uma força.", "Ele deu uma ajuda.")]
    [InlineData("Dei uma escapada.", "Dei uma escapadela.")]
    [InlineData("Vou dar rolê.", "Vou dar uma volta.")]
    [InlineData("Vou dar uns rolês.", "Vou dar umas voltas.")]
    [InlineData("Estou de saco cheio.", "Estou farto.")]
    [InlineData("Ela ficou de saco cheio.", "Ela ficou farta.")]
    [InlineData("Elas ficaram de saco cheio.", "Elas ficaram fartas.")]
    [InlineData("Nós estamos de saco cheio.", "Nós estamos fartos.")]
    [InlineData("Ela não ficou de saco cheio.", "Ela não ficou farta.")]
    [InlineData("Ela já está de saco cheio.", "Ela já está farta.")]
    [InlineData("Ele falou pra caramba.", "Ele falou imenso.")]
    [InlineData("Falar para caramba.", "Falar imenso.")]
    [InlineData("PUXEI ASSUNTO.", "METI CONVERSA.")]
    [InlineData("Ela puxou\nassunto.", "Ela meteu\nconversa.")]
    [InlineData("<i>Ela puxou assunto.</i>", "<i>Ela meteu conversa.</i>")]
    public void AdditionalWorkbookExpressions(string source, string expected)
    {
        AssertConverts(source, expected);
        Assert.Equal(expected, Converter.Convert(expected));
    }

    [Theory]
    [InlineData("Baixei um arquivo.", "Descarreguei um ficheiro.", "Baixei um arquivo.")]
    [InlineData("Ela salvou os arquivos.", "Ela guardou os ficheiros.", "Ela salvou os arquivos.")]
    [InlineData("Vou salvar o arquivo.", "Vou guardar o ficheiro.", "Vou salvar o arquivo.")]
    [InlineData("Ela está baixando o arquivo.", "Ela está a descarregar o ficheiro.", "Ela está a baixar o arquivo.")]
    [InlineData("Não salve este arquivo.", "Não guardes este ficheiro.", "Não salves este arquivo.")]
    [InlineData("BAIXEI O ARQUIVO.", "DESCARREGUEI O FICHEIRO.", "BAIXEI O ARQUIVO.")]
    [InlineData("Baixei um\narquivo.", "Descarreguei um\nficheiro.", "Baixei um\narquivo.")]
    public void AdditionalWorkbookDigitalFiles(string source, string expected, string withoutVocabulary)
    {
        AssertConverts(source, expected);
        Assert.Equal(expected, Converter.Convert(expected));
        Assert.Equal(withoutVocabulary, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
    }

    [Theory]
    [InlineData("Ele quebrou um galho.", "He did me a favor.", "Ele desenrascou.")]
    [InlineData("Ele quebrou o galho.", "He broke the branch.", "Ele quebrou o galho.")]
    [InlineData("Ele quebrou um galho.", "He helped cut a branch.", "Ele partiu um galho.")]
    public void AdditionalWorkbookLiteralBranchesNeedReference(string source, string english, string expected)
    {
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, Converter.Convert(source, null, hint));
        Assert.Equal(expected, Converter.Convert(expected, null, hint));
    }

    [Theory]
    [InlineData("Ele salvou a criança.")]
    [InlineData("Ela baixou a cabeça.")]
    [InlineData("O arquivo histórico.")]
    [InlineData("A segurança rodoviária.")]
    [InlineData("A balada é uma canção.")]
    [InlineData("A matéria está na mesa.")]
    [InlineData("Uma prova do crime.")]
    [InlineData("Boleto e atestado médico.")]
    [InlineData("SUS e SNS.")]
    [InlineData("Ficar a ver navios.")]
    [InlineData("Dar o braço a torcer.")]
    [InlineData("Ela mexerica sobre todos.")]
    [InlineData("Puxou o assunto da reunião.")]
    [InlineData("Eu escanteio o jogador.")]
    [InlineData("O motor deu uma força de 30 newtons.")]
    [InlineData("Ele é bonito para caramba.")]
    public void AdditionalWorkbooksPreserveAmbiguousAndSharedTerms(string source) =>
        AssertConverts(source, source);

    [Fact]
    public void AdditionalWorkbookMedicalParadaKeepsExistingTranslation()
    {
        AssertConverts("Contou uma parada cardíaca.", "Contou uma paragem cardíaca.");
        AssertConverts("Contou umas paradas cardíacas.", "Contou umas paradas cardíacas.");
    }

    [Fact]
    public void AdditionalWorkbookLiteralSausageIsNotPluralized()
    {
        AssertConverts("Encheu linguiça com carne.", "Encheu chouriço com carne.");
        var hint = ReferenceHint.FromText("He stuffed a sausage.");
        Assert.Equal("Ele encheu chouriço.", Converter.Convert("Ele encheu linguiça.", null, hint));
    }

    [Fact]
    public void AdditionalWorkbookDigitalFilesRespectStageSuppression()
    {
        Assert.Equal("Baixei um arquivo.", new PortugueseConverter(
            new ConversionOptions(DisabledRules: "construction-vocabulary")).Convert("Baixei um arquivo."));
    }

    [Theory]
    [InlineData("Ela puxou assunto.")]
    [InlineData("Dei uma força.")]
    [InlineData("Estou de saco cheio.")]
    public void AdditionalWorkbookExpressionsRespectToggles(string source)
    {
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "reference-idioms")).Convert(source));
    }

    [Theory]
    [InlineData("Uma breja gelada.", "Uma cerveja gelada.")]
    [InlineData("Duas brejas.", "Duas cervejas.")]
    [InlineData("A bufunfa acabou.", "A massa acabou.")]
    [InlineData("Vou pegar um busão.", "Vou apanhar um autocarro.")]
    [InlineData("Os busões chegaram.", "Os autocarros chegaram.")]
    [InlineData("Ela pegou um busão.", "Ela apanhou um autocarro.")]
    [InlineData("Uma fita adesiva nova.", "Uma fita-cola nova.")]
    [InlineData("As fitas adesivas.", "As fitas-cola.")]
    [InlineData("O flanelinha chegou.", "O arrumador de carros chegou.")]
    [InlineData("Os flanelinhas chegaram.", "Os arrumadores de carros chegaram.")]
    [InlineData("Uma sunga nova.", "Um slip de banho novo.")]
    [InlineData("As sungas novas.", "Os slips de banho novos.")]
    [InlineData("Ele paquerou a Ana.", "Ele engatou a Ana.")]
    [InlineData("Elas paqueravam.", "Elas engatavam.")]
    [InlineData("Não paqueres.", "Não engates.")]
    [InlineData("Uma meia-calça nova.", "Uns collants novos.")]
    [InlineData("A meia-calça está rasgada.", "Os collants estão rasgados.")]
    [InlineData("As meias-calças novas.", "Os collants novos.")]
    [InlineData("Minha meia-calça está azul.", "Os meus collants estão azuis.")]
    [InlineData("Aquela meia-calça velha.", "Aqueles collants velhos.")]
    [InlineData("A meia-calça vermelha.", "Os collants vermelhos.")]
    [InlineData("UMA FITA ADESIVA.", "UMA FITA-COLA.")]
    public void VerifiedSpreadsheetVocabulary(string source, string expected)
    {
        AssertConverts(source, expected);
        Assert.Equal(expected, Converter.Convert(expected));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
    }

    [Theory]
    [InlineData("A mensalidade da faculdade.", "A propina.")]
    [InlineData("As mensalidades da faculdade.", "As propinas.")]
    public void UniversityFeesAreNotConvertedToBribesInTheSameRun(string source, string expected) =>
        AssertConverts(source, expected);

    [Theory]
    [InlineData("Vou dar um rolê.", "Vou dar uma volta.")]
    [InlineData("Dei um rolezinho.", "Dei uma voltinha.")]
    [InlineData("Demos uns rolês.", "Demos umas voltas.")]
    [InlineData("DÁ UM ROLÊ!", "DÁ UMA VOLTA!")]
    [InlineData("Vou dar um\nrolê.", "Vou dar uma\nvolta.")]
    [InlineData("Ela trocou uma ideia com a Ana.", "Ela falou com a Ana.")]
    [InlineData("Eles trocaram uma ideia.", "Eles falaram.")]
    [InlineData("Vou trocar uma ideia.", "Vou falar.")]
    [InlineData("Ela bateu um papo com a Ana.", "Ela falou com a Ana.")]
    [InlineData("Nós batíamos papo.", "Nós conversávamos.")]
    [InlineData("Paguei mico.", "Passei vergonha.")]
    [InlineData("Ela pagou um mico.", "Ela passou vergonha.")]
    [InlineData("Elas pagaram mico.", "Elas passaram vergonha.")]
    [InlineData("Vou pagar mico.", "Vou passar vergonha.")]
    [InlineData("PAGUEI MICO!", "PASSEI VERGONHA!")]
    [InlineData("<i>Ela pagou mico.</i>", "<i>Ela passou vergonha.</i>")]
    [InlineData("Ela estava pagando mico.", "Ela estava a passar vergonha.")]
    [InlineData("Ela ficou de bobeira.", "Ela ficou sem fazer nada.")]
    [InlineData("Elas ficaram de bobeira.", "Elas ficaram sem fazer nada.")]
    [InlineData("Ele fala abobrinha.", "Ele fala disparates.")]
    [InlineData("Eles falavam abobrinhas.", "Eles falavam disparates.")]
    [InlineData("Tomei uma breja.", "Bebi uma cerveja.")]
    [InlineData("Tomamos brejas.", "Bebemos cervejas.")]
    [InlineData("Ele estava tomando uma breja.", "Ele estava a beber uma cerveja.")]
    [InlineData("Ela tinha tomado uma breja.", "Ela tinha bebido uma cerveja.")]
    [InlineData("Ela tinha pagado mico.", "Ela tinha passado vergonha.")]
    [InlineData("Ele tinha dado um rolê.", "Ele tinha dado uma volta.")]
    [InlineData("Ela estava trocando uma ideia.", "Ela estava a falar.")]
    public void VerifiedSpreadsheetExpressions(string source, string expected)
    {
        AssertConverts(source, expected);
        Assert.Equal(expected, Converter.Convert(expected));
    }

    [Theory]
    [InlineData("Troquei uma ideia por outra.")]
    [InlineData("Paguei o bilhete.")]
    [InlineData("O mico subiu à árvore.")]
    [InlineData("O bife rolê está pronto.")]
    [InlineData("Errei de bobeira.")]
    [InlineData("Falar de abobrinha.")]
    [InlineData("Gatinho, mala, descarga, vitamina, grampo.")]
    [InlineData("Uma flanelinha macia.")]
    [InlineData("O pão-duro está na cozinha.")]
    [InlineData("Estou com pressa.")]
    [InlineData("Fazer uma vaquinha.")]
    [InlineData("A água com gás.")]
    [InlineData("A injeção e o preservativo.")]
    public void SpreadsheetDoesNotIntroduceUnsafeLiteralReplacements(string source) =>
        AssertConverts(source, source);

    [Theory]
    [InlineData("Vou dar um rolê.")]
    [InlineData("Ela trocou uma ideia.")]
    [InlineData("Paguei mico.")]
    public void SpreadsheetExpressionsRespectDisabledGroups(string source)
    {
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
    }

    [Theory]
    [InlineData("Ele é pão-duro.", "He's stingy.", "Ele é forreta.")]
    [InlineData("São pães-duros.", "They're cheapskates.", "São forretas.")]
    [InlineData("Um pão-duro.", "A kitchen spatula.", "Um pão-duro.")]
    [InlineData("O pão está duro.", "The bread is hard.", "O pão está duro.")]
    public void SpreadsheetAmbiguousSlangRequiresReference(string source, string english, string expected)
    {
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, Converter.Convert(source, null, hint));
        Assert.Equal(expected, Converter.Convert(expected, null, hint));
        Assert.Equal(source, Converter.Convert(source));
    }

    [Fact]
    public void SpreadsheetExpressionsRespectStageSuppressionAndTrace()
    {
        var trace = new List<RuleChange>();
        Assert.Equal("Passei vergonha.", Converter.Convert("Paguei mico.", trace: trace));
        Assert.Contains(trace, r => r.Id == "reference-idioms");
        Assert.Equal("Paguei mico.", new PortugueseConverter(
            new ConversionOptions(DisabledRules: "reference-idioms")).Convert("Paguei mico."));
    }

    [Theory]
    [InlineData("Ned, quem você acha que vê isso?", "Ned, who do you think is actually watching this?", "Ned, quem achas que vê isto?")]
    [InlineData("Quem você pensa que trabalha aqui?", "Who do you think works here?", "Quem pensas que trabalha aqui?")]
    [InlineData("Quem você achava que ganhou?", "Who did you think won?", "Quem achavas que ganhou?")]
    [InlineData("O que você acha que mudou?", "What do you think changed?", "O que achas que mudou?")]
    [InlineData("Qual equipa você acha que ganha?", "Which team do you think wins?", "Qual equipa achas que ganha?")]
    [InlineData("Quem tu imaginas que sabe?", "Who do you imagine knows?", "Quem tu imaginas que sabe?")]
    [InlineData("Quem achas\nque vê isso?", "Who do you think sees this?", "Quem achas\nque vê isto?")]
    [InlineData("QUEM VOCÊ ACHA QUE VÊ ISSO?", "Who do you think sees this?", "QUEM ACHAS QUE VÊ ISTO?")]
    [InlineData("<i>Quem você acha que chegou?</i>", "Who do you think arrived?", "<i>Quem achas que chegou?</i>")]
    [InlineData("Quem você acha que pode ajudar?", "Who do you think can help?", "Quem achas que pode ajudar?")]
    [InlineData("Quem você acha que vai ganhar?", "Who do you think will win?", "Quem achas que vai ganhar?")]
    [InlineData("Quem você acha que vê? Você sabe que chegou.", "Who do you think sees? You know you arrived.", "Quem achas que vê? Sabes que chegaste.")]
    [InlineData("Quem você acredita que chegou?", "Who do you believe arrived?", "Quem acreditas que chegou?")]
    [InlineData("Quem você disse que viu isso?", "Who did you say saw this?", "Quem disseste que viu isto?")]
    [InlineData("Quem você acha que realmente vê isso?", "Who do you think actually sees this?", "Quem achas que realmente vê isto?")]
    public void ExtractedQuestionDoesNotMakeEmbeddedSubjectSecondPerson(string source, string english, string expected)
    {
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, Converter.Convert(source, null, hint));
        Assert.Equal(expected, Converter.Convert(expected, null, hint));
    }

    [Theory]
    [InlineData("Quem você acha que viu?", "Who do you think you saw?", "Quem achas que viste?")]
    [InlineData("O que você pensa que fez?", "What do you think you did?", "O que pensas que fizeste?")]
    [InlineData("Quem você acha que pode ajudar?", "Who do you think you can help?", "Quem achas que podes ajudar?")]
    [InlineData("Quem você acha que você vê?", "Who do you think you see?", "Quem achas que vês?")]
    [InlineData("Quem você disse que viu?", "Who did you say you saw?", "Quem disseste que viste?")]
    [InlineData("Onde você acha que trabalhou?", "Where do you think you worked?", "Onde achas que trabalhaste?")]
    [InlineData("Por que você acha que fez isso?", "Why do you think you did this?", "Porque achas que fizeste isto?")]
    public void ExtractedObjectsKeepConfirmedSecondPersonSubject(string source, string english, string expected)
    {
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, Converter.Convert(source, null, hint));
        Assert.Equal(expected, Converter.Convert(expected, null, hint));
    }

    [Theory]
    [InlineData("Quem você acha que pode ajudar?", "Quem achas que pode ajudar?")]
    [InlineData("Quem você pensa que consegue ganhar?", "Quem pensas que consegue ganhar?")]
    public void AmbiguousExtractedQuestionsDoNotAssumeSubjectContinuity(string source, string expected) =>
        AssertConverts(source, expected);

    [Fact]
    public void ExtractedQuestionGuardRespectsAddressOptionsWithoutDependingOnColloquial()
    {
        const string source = "Ned, quem você acha que vê isso?";
        var hint = ReferenceHint.FromText("Ned, who do you think is actually watching this?");
        Assert.Equal("Ned, quem achas que vê isto?",
            new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(source, null, hint));
        Assert.Equal("Ned, quem acha que vê isto?",
            Converter.Convert(source, null, hint! with { FormalAddress = true }));
        Assert.Equal("Ned, quem você acha que vê isto?",
            new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source, null, hint));
    }

    [Theory]
    [InlineData("Sabe o que aconteceu ontem?", "You know what happened yesterday?", "Sabes o que aconteceu ontem?")]
    [InlineData("Sabe o que é aquilo?", "You know what that is?", "Sabes o que é aquilo?")]
    [InlineData("Sabe o que era aquilo?", "You know what that was?", "Sabes o que era aquilo?")]
    [InlineData("Sabe o que está acontecendo?", "You know what's happening?", "Sabes o que está a acontecer?")]
    [InlineData("O que está acontecendo?", "What are you doing?", "O que está a acontecer?")]
    [InlineData("O que é isso?", "What are you doing?", "O que é isso?")]
    [InlineData("Acha que sabe o que aconteceu?", "You think you know what happened?", "Achas que sabes o que aconteceu?")]
    [InlineData("Sabe o\nque aconteceu ontem?", "You know what happened yesterday?", "Sabes o\nque aconteceu ontem?")]
    [InlineData("SABE O QUE ACONTECEU ONTEM?", "You know what happened yesterday?", "SABES O QUE ACONTECEU ONTEM?")]
    public void ReferenceDoesNotTreatEmbeddedWhatAsYou(string source, string english, string expected)
    {
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, Converter.Convert(source, null, hint));
        Assert.Equal(expected, Converter.Convert(expected, null, hint));
        var noYou = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        if (source == "O que está acontecendo?")
            Assert.Equal(expected, noYou.Convert(source, null, hint));
        else
            Assert.Equal(noYou.Convert(source), noYou.Convert(source, null, hint));
    }

    [Theory]
    [InlineData("Se isso me preocupa, eu paro.")]
    [InlineData("Se isto te assusta, espera.")]
    [InlineData("Quando aquilo me preocupa, eu paro.")]
    [InlineData("Não sei aonde isso me leva.")]
    [InlineData("Se isso\nme preocupa, eu paro.")]
    [InlineData("Se isso me preocupa, eu paro. Isso preocupa-me.")]
    public void DemonstrativeSubjectsPreserveSubordinateProclisis(string text) =>
        Assert.Equal(text, Converter.Convert(text));

    [Theory]
    [InlineData("Isso me preocupa.", "Isso preocupa-me.")]
    [InlineData("Isto me assusta.", "Isto assusta-me.")]
    [InlineData("Eu sei que isso me preocupa.", "Eu sei que isso me preocupa.")]
    public void DemonstrativeSubjectsStillUseMainClauseEnclisis(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert(source));
    }

    [Theory]
    [InlineData("Nos últimos quatro dias.")]
    [InlineData("Nos próximos meses.")]
    [InlineData("Nos péssimos resultados.")]
    [InlineData("NOS ÚLTIMOS QUATRO DIAS.")]
    [InlineData("Nos\núltimos quatro dias.")]
    public void ContractedNosBeforeAdjectivesIsNotAClitic(string text)
    {
        Assert.Equal(text, Converter.Convert(text));
    }

    [Theory]
    [InlineData("Nos vemos amanhã.", "Vemo-nos amanhã.")]
    [InlineData("Nos encontramos amanhã.", "Encontramo-nos amanhã.")]
    [InlineData("Nos diga a verdade.", "Diz-nos a verdade.")]
    public void LeadingNosStillAttachesToRecognizedVerbs(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
    }

    [Theory]
    [InlineData("Que doideira!", "Que loucura!")]
    [InlineData("São doideiras.", "São loucuras.")]
    [InlineData("Vambora!", "Vamos embora!")]
    [InlineData("VAMBORA!", "VAMOS EMBORA!")]
    [InlineData("Bora lá!", "Bora lá!")]
    public void AdditionalBrazilianSlangUsesEuropeanEquivalents(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
    }

    [Theory]
    [InlineData("Este é o detetive que falei para vocês.", "Este é o detetive de que vos falei.")]
    [InlineData("Esta é a professora que falei para ela.", "Esta é a professora de que lhe falei.")]
    [InlineData("Esse é o livro que falei para nós.", "Esse é o livro de que nos falei.")]
    [InlineData("Este é o detetive que\nfalei para vocês.", "Este é o detetive de que\nvos falei.")]
    public void RelativeFalarUsesPrepositionAndDative(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
    }

    [Theory]
    [InlineData("Eu alcanço vocês depois.", "I'll catch you guys later.", "Eu depois apanho-vos.")]
    [InlineData("Alcanço você mais tarde.", "I'll catch you later.", "Depois apanho-te.")]
    [InlineData("Eu alcanço vocês depois.", "I reach you later.", "Eu alcanço vocês depois.")]
    public void CatchUpLaterUsesReferenceIdiom(string source, string english, string expected) =>
        Assert.Equal(expected, Converter.Convert(source, null, ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Vamos pegar este cara.", "Vamos apanhar este tipo.")]
    [InlineData("Pegou o suspeito.", "Apanhou o suspeito.")]
    [InlineData("Peguem esses criminosos.", "Apanhem esses criminosos.")]
    [InlineData("Pegaremos aquela fugitiva.", "Apanharemos aquela fugitiva.")]
    [InlineData("Não peguei nenhum suspeito.", "Não apanhei nenhum suspeito.")]
    [InlineData("Vamos pegar este tipo de arma.", "Vamos pegar neste tipo de arma.")]
    [InlineData("Pega a arma.", "Pega na arma.")]
    [InlineData("Vou pegar uma bebida.", "Vou buscar uma bebida.")]
    [InlineData("Vamos pegar\neste cara.", "Vamos apanhar\neste tipo.")]
    public void CatchPeopleKeepsObjectAndVerbInflection(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
    }

    [Theory]
    [InlineData("A gente sempre escuta,", "You just hear about this stuff.", "Ouvimos sempre,")]
    [InlineData("A gente sempre escuta sobre estas coisas.", null, "Ouvimos sempre sobre estas coisas.")]
    [InlineData("Nós escutamos sempre sobre estas coisas.", null, "Nós ouvimos sempre sobre estas coisas.")]
    [InlineData("Ela escutou sobre o acidente.", null, "Ela ouviu sobre o acidente.")]
    [InlineData("Escutarei sobre isso.", null, "Ouvirei sobre isso.")]
    [InlineData("A gente sempre escutava.", "We always heard about it.", "Ouvíamos sempre.")]
    [InlineData("A gente sempre escuta música.", "We always listen to music.", "Escutamos sempre música.")]
    [InlineData("Escute com atenção.", "Listen carefully.", "Escuta com atenção.")]
    [InlineData("A gente nunca escuta.", "We never listen.", "Nunca escutamos.")]
    [InlineData("A gente sempre se lembra.", null, "Sempre nos lembramos.")]
    [InlineData("A gente sempre escuta,", null, "Escutamos sempre,")]
    public void HearingAboutDiffersFromDeliberateListening(string source, string? english, string expected)
    {
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, Converter.Convert(source, null, hint));
        Assert.Equal(expected, Converter.Convert(expected, null, hint));
    }

    [Theory]
    [InlineData("e o síndico lhe disse que", "e o encarregado do edifício disse-lhe que")]
    [InlineData("Nossa força-tarefa\nvai responder cada ligação,", "A nossa equipa especial\nvai responder cada chamada,")]
    [InlineData("Me coloque na\nespera, que espero.", "Põe-me em\nespera, que espero.")]
    [InlineData("Me coloque na espera.", "Põe-me em espera.")]
    [InlineData("Coloca-me na espera.", "Põe-me em espera.")]
    [InlineData("Ponha-o na espera.", "Põe-no em espera.")]
    [InlineData("Ficou na espera do autocarro.", "Ficou na espera do autocarro.")]
    [InlineData("Ele usa um terno.", "Ele usa um fato.")]
    [InlineData("Os maiôs.", "Os fatos de banho.")]
    [InlineData("Qual é o seu sobrenome?", "Qual é o teu apelido?")]
    [InlineData("Vamos ao shopping.", "Vamos ao centro comercial.")]
    [InlineData("Chega de papos.", "Chega de conversas.")]
    [InlineData("A síndica chegou.", "A encarregada do edifício chegou.")]
    public void SinglePassReachesFinalResult(string br, string pt) => AssertConverts(br, pt);

    // Re-running cannot distinguish PT-PT fato/apelido/propina from their different Brazilian senses.
    private static readonly Regex AmbiguousOnRerun = new(
        @"(?<![\p{L}-])(?:fatos?|apelidos?|propinas?|comercia(?:l|is)|conversas)(?![\p{L}-])", RegexOptions.IgnoreCase);

    [Theory]
    [InlineData("Estavam armando uma cilada.", "Estavam a armar uma cilada.")]
    [InlineData("Nos diga a verdade.", "Diz-nos a verdade.")]
    [InlineData("Nos casos graves, ligue.", "Nos casos graves, ligue.")]
    [InlineData("Deixa-me te perguntar uma coisa.", "Deixa-me perguntar-te uma coisa.")]
    [InlineData("Ele está tentando me proteger.", "Ele está a tentar proteger-me.")]
    [InlineData("Isso é gente ruim.", "Isso é gente má.")]
    [InlineData("Foram seis caras.", "Foram seis tipos.")]
    [InlineData("Que tipo de cara faz isso?", "Que tipo de homem faz isso?")]
    [InlineData("Que?", "Quê?")]
    [InlineData("Isso é para mim fazer.", "Isso é para eu fazer.")]
    [InlineData("Ao invés de ficar, saiu.", "Em vez de ficar, saiu.")]
    [InlineData("Moramos a duas quadras daqui.", "Vivemos a dois quarteirões daqui.")]
    [InlineData("Comprei uma passagem de avião.", "Comprei um bilhete de avião.")]
    [InlineData("O motorista chegou.", "O condutor chegou.")]
    [InlineData("O motorista de ônibus chegou.", "O motorista de autocarro chegou.")]
    [InlineData("Ele faz isso toda hora.", "Ele faz isso a toda a hora.")]
    [InlineData("Uma gota d'água.", "Uma gota de água.")]
    [InlineData("Esse ano vai ser bom.", "Este ano vai ser bom.")]
    [InlineData("Ele me chamou de idiota.", "Ele chamou-me idiota.")]
    [InlineData("Chamou de volta.", "Chamou de volta.")]
    [InlineData("A ligação entre eles.", "A ligação entre eles.")]
    [InlineData("Laboratório voltou\ncom o resultado do kit estupro.", "O Laboratório devolveu\no resultado do kit de violação.")]
    [InlineData("Preciso do kit de estupro.", "Preciso do kit de violação.")]
    [InlineData("Trouxe um kit primeiros socorros.", "Trouxe um kit de primeiros socorros.")]
    [InlineData("Estava pensando de ir de topless ao nosso casamento", "Estava a pensar em ir em topless ao nosso casamento")]
    [InlineData("Penso de ir embora.", "Penso em ir embora.")]
    [InlineData("O que você pensa de ficar aqui?", "O que pensas de ficar aqui?")]
    [InlineData("Ela andava de topless na praia.", "Ela andava em topless na praia.")]
    [InlineData("Tem uma ligação para você.", "Há uma chamada para ti.")]
    [InlineData("Tem um tipo lá fora.", "Há um tipo lá fora.")]
    [InlineData("Tem uma ligação forte entre eles.", "Há uma ligação forte entre eles.")]
    [InlineData("A vítima tem ficha?", "A vítima tem registo criminal?")]
    [InlineData("Ele tem ficha na polícia.", "Ele tem registo criminal na polícia.")]
    [InlineData("Ele esconde a ficha na polícia.", "Ele esconde o registo criminal.")]
    [InlineData("O paciente tem ficha?", "O paciente tem historial clínico?")]
    [InlineData("Ele tem uma ficha limpa.", "Ele tem um registo criminal limpo.")]
    [InlineData("Preencha a ficha de inscrição.", "Preenche a ficha de inscrição.")]
    [InlineData("Ele tem ficha técnica completa.", "Ele tem ficha técnica completa.")]
    [InlineData("Liga para o 190.", "Liga para o 112.")]
    [InlineData("-Chamou 190? -Sim, fui eu.", "-Chamou 112? -Sim, fui eu.")]
    [InlineData("Ligue para o 911.", "Liga para o 112.")]
    [InlineData("O número 190 está na ficha.", "O número 190 está na ficha.")]
    [InlineData("Ele gostou que eu dirigi.", "Ele gostou de ser eu a conduzir.")]
    [InlineData("Ele gostou que eu paguei a conta.", "Ele gostou de ser eu a pagar a conta.")]
    [InlineData("Ele gostou que eu preparei o jantar.", "Ele gostou de ser eu a preparar o jantar.")]
    [InlineData("Ele gostava que eu dirigia.", "Ele gostava de ser eu a conduzir.")]
    [InlineData("Ele gostou que eu conduzisse.", "Ele gostou que eu conduzisse.")]
    [InlineData("e uma reunião de inteligência logo após.", "e uma reunião de inteligência logo depois.")]
    [InlineData("Logo após a reunião.", "Logo depois a reunião.")]
    [InlineData("Após a reunião, saímos.", "Após a reunião, saímos.")]
    [InlineData("para investigar cada tiroteio\na envolver policial.", "para investigar cada tiroteio\na envolver um agente.")]
    [InlineData("Ele foi baleado por policial.", "Ele foi baleado por um agente.")]
    [InlineData("Casos envolvendo policiais.", "Casos envolvendo agentes.")]
    [InlineData("Ação policial.", "Ação policial.")]
    [InlineData("Esperamos uma ligação furiosa", "Esperamos uma chamada furiosa")]
    [InlineData("Ele tinha uma ligação naquela hora.", "Ele tinha uma chamada naquela hora.")]
    [InlineData("Um bar de topless.", "Um bar de topless.")]
    [InlineData("O policial Olivet está aqui para vê-lo.", "O agente Olivet está aqui para vê-lo.")]
    [InlineData("Sabia sobre isso?", "Sabia disto?")]
    [InlineData("Mande entrar.", "Mando-o entrar.")]
    [InlineData("Nossa foto de casamento?", "A nossa foto de casamento?")]
    [InlineData("E ocupando nossa cadeira muito brevemente.", "E ocupando a nossa cadeira muito brevemente.")]
    [InlineData("Nossa, que lindo!", "Caramba, que lindo!")]
    [InlineData("Quando seu pai chegou, saí.", "Quando o teu pai chegou, saí.")]
    [InlineData("A policial Janko não teve nada com isso.", "A agente Janko não teve nada com isso.")]
    [InlineData("Gostaria que você fosse policial por uma semana.", "Gostaria que fosses polícia por uma semana.")]
    [InlineData("Talvez seja melhor policial do que pensa.", "Talvez seja melhor polícia do que pensa.")]
    [InlineData("Ele estava disfarçado de policial.", "Ele estava disfarçado de polícia.")]
    [InlineData("Foi uma ação policial.", "Foi uma ação policial.")]
    [InlineData("Ta brincando?", "Estás a brincar?")]
    [InlineData("Esta faltando um pouco de cor.", "Está a faltar um pouco de cor.")]
    [InlineData("Esta casa é grande.", "Esta casa é grande.")]
    [InlineData("Direto, mas pegando leve com ela.", "Direto, mas a ir devagar com ela.")]
    [InlineData("Pega leve comigo.", "Vai com calma comigo.")]
    [InlineData("Vou pegar leve.", "Vou ter calma.")]
    [InlineData("Um bando de caras esperam trabalho.", "Um bando de tipos esperam trabalho.")]
    [InlineData("Caras, jogaram uma garrafa.", "Rapazes, jogaram uma garrafa.")]
    [InlineData("As coisas caras que comprei.", "As coisas caras que comprei.")]
    [InlineData("Ao invés da minha cara feia.", "Em vez da minha cara feia.")]
    [InlineData("Agora me arrependo disso.", "Agora arrependo-me disso.")]
    [InlineData("Hoje me sinto bem.", "Hoje sinto-me bem.")]
    [InlineData("Depois me liga.", "Depois liga-me.")]
    [InlineData("Já me lembro.", "Já me lembro.")]
    [InlineData("Mas ele nunca furaria comigo.", "Mas ele nunca falharia comigo.")]
    [InlineData("Ela furou com a gente ontem.", "Ela falhou connosco ontem.")]
    [InlineData("Não fure comigo.", "Não falhes comigo.")]
    [InlineData("Ele furou a fila.", "Ele furou a fila.")]
    [InlineData("Policial Kevin Olivet atirou.", "Agente Kevin Olivet disparou.")]
    [InlineData("Obrigada, policial.", "Obrigada, agente.")]
    [InlineData("Não matei policial algum.", "Não matei nenhum polícia.")]
    [InlineData("Foi uma ação policial.", "Foi uma ação policial.")]
    [InlineData("Continuaremos procurando.", "Continuaremos a procurar.")]
    [InlineData("Quatro custarão $ 200.", "Quatro custarão 200 dólares.")]
    [InlineData("Ele pagou $ 10 mil.", "Ele pagou 10 mil dólares.")]
    [InlineData("Ganhou $ 1 milhão.", "Ganhou 1 milhão de dólares.")]
    [InlineData("Time 2, suspeito em movimento.", "Equipa 2, suspeito em movimento.")]
    [InlineData("Ele dava ré no estacionamento.", "Ele dava marcha-atrás no estacionamento.")]
    [InlineData("Eles jogaram uma garrafa na rua.", "Eles atiraram uma garrafa na rua.")]
    [InlineData("Vão jogar a toalha?", "Vão atirar a toalha ao chão?")]
    [InlineData("Eles vão jogar futebol.", "Eles vão jogar futebol.")]
    [InlineData("Cara.", "Pá.")]
    [InlineData("Falei com o cara da FedEx.", "Falei com o tipo da FedEx.")]
    [InlineData("O que é isso aqui?", "O que é isto?")]
    [InlineData("Esse aqui é bom.", "Este é bom.")]
    [InlineData("Essas aqui são minhas.", "Estas são minhas.")]
    [InlineData("Isso aí é seu.", "Isso aí é teu.")]
    [InlineData("Laboratório voltou com o resultado do kit de estupro.", "O Laboratório devolveu o resultado do kit de violação.")]
    [InlineData("Tem a amostra mas não bate no sistema.", "A amostra existe, mas não tem correspondência no sistema.")]
    [InlineData("A digital não bate com o banco de dados.", "A digital não tem correspondência no banco de dados.")]
    [InlineData("Ele voltou com a namorada.", "Ele voltou com a namorada.")]
    [InlineData("Eu vou na padaria.", "Eu vou à padaria.")]
    [InlineData("Você foi no supermercado?", "Foste ao supermercado?")]
    [InlineData("Ela foi na casa da Ana.", "Ela foi à casa da Ana.")]
    [InlineData("Ele foi no carro dela.", "Ele foi no carro dela.")]
    [InlineData("Ela vai no sábado.", "Ela vai no sábado.")]
    [InlineData("Vou mostrar a casa para ele.", "Vou mostrar-lhe a casa.")]
    [InlineData("Ele deu o livro para ela.", "Ele deu-lhe o livro.")]
    [InlineData("Vou levar o carro para ele.", "Vou levar o carro para ele.")]
    [InlineData("Vou comprar um presente para ela.", "Vou comprar um presente para ela.")]
    [InlineData("Não leve pro pessoal, Syd.", "Não leve para o lado pessoal, Syd.")]
    [InlineData("Não leve isso pro pessoal.", "Não leve isso para o lado pessoal.")]
    [InlineData("Ele levou para o pessoal da empresa.", "Ele levou para o pessoal da empresa.")]
    [InlineData("Um kit completo.", "Um kit completo.")]
    [InlineData("E Sebastian dá com os ombros,", "E Sebastian abana os ombros,")]
    [InlineData("Ela deu com os ombros.", "Ela abanou os ombros.")]
    [InlineData("Ele tem ficha criminal.", "Ele tem registo criminal.")]
    [InlineData("Tenho meu carro aqui.", "Tenho o meu carro aqui.")]
    [InlineData("Tenho meu próprio carro.", "Tenho o meu próprio carro.")]
    public void CorpusReviewRules(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("e o síndico lhe disse que")]
    [InlineData("Nossa força-tarefa\nvai responder cada ligação,")]
    [InlineData("Me coloque na\nespera, que espero.")]
    public void ReportedCuesAreStableWhenConvertedAgain(string br)
    {
        var once = Converter.Convert(br);
        Assert.Equal(once, Converter.Convert(once));
    }

    [Fact]
    public void ConvertingAgainChangesNothingForAllRegressionExamples()
    {
        var unstable = new List<string>();
        foreach (var method in typeof(ConverterTests).GetMethods())
        {
            foreach (var data in method.GetCustomAttributes(typeof(InlineDataAttribute), false).Cast<InlineDataAttribute>())
            {
                var args = data.GetData(method).Single();
                if (args.Length < 1 || args[0] is not string input)
                {
                    continue;
                }

                var once = Converter.Convert(input);
                if (AmbiguousOnRerun.IsMatch(once))
                {
                    continue;
                }

                var twice = Converter.Convert(once);
                if (once != twice)
                {
                    unstable.Add($"{method.Name}: \"{input}\" → \"{once}\" → \"{twice}\"");
                }
            }
        }

        Assert.True(unstable.Count == 0, string.Join(Environment.NewLine, unstable));
    }

    [Theory]
    [InlineData("- Como posso ajudá-lo?\n- Estamos entrando. Se afaste.",
        "- Como posso ajudá-lo?\n- Estamos a entrar. Afasta-te.")]
    [InlineData("Se afaste.", "Afasta-te.")]
    [InlineData("Se afastem da porta!", "Afastem-se da porta!")]
    [InlineData("Se aproxime.", "Aproxima-te.")]
    [InlineData("Se sente.", "Sente-se.")]
    [InlineData("Se levante!", "Levanta-te!")]
    [InlineData("Se prepare para sair.", "Prepara-te para sair.")]
    [InlineData("Se proteja.", "Protege-te.")]
    [InlineData("Se esconda.", "Esconde-te.")]
    [InlineData("Se acalme.", "Acalma-te.")]
    [InlineData("SE AFASTE!", "AFASTA-TE!")]
    [InlineData("<i>Se afaste.</i>", "<i>Afasta-te.</i>")]
    [InlineData("Afaste-se.", "Afasta-te.")]
    [InlineData("Não se afaste.", "Não te afastes.")]
    [InlineData("Espero que se afaste.", "Espero que se afaste.")]
    [InlineData("Talvez se afaste.", "Talvez se afaste.")]
    [InlineData("Se afaste o perigo, ficamos aqui.", "Se afaste o perigo, ficamos aqui.")]
    [InlineData("Se afaste ele, esperamos.", "Se afaste ele, esperamos.")]
    [InlineData("Se sente dor, fale.", "Se sente dor, fale.")]
    [InlineData("Se sente dor.", "Se sente dor.")]
    public void ReflexiveImperativePlacement(string br, string pt) => AssertConverts(br, pt);

    [Fact]
    public void ReflexiveImperativesRespectOptionsAndIgnoredPhrases()
    {
        Assert.Equal("Se afaste.", new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert("Se afaste."));
        var local = new PortugueseLocalWordList();
        local.Ignored.Add("Se afaste");
        Assert.Equal("Se afaste.", new PortugueseConverter(localWordList: local).Convert("Se afaste."));
    }

    [Theory]
    [InlineData("Precisamos falar com\nseu marido. Ele está em casa?", "Precisamos de falar com\no seu marido. Ele está em casa?")]
    [InlineData("Sua mulher. Ela está em casa?", "A sua mulher. Ela está em casa?")]
    [InlineData("Já lhe mandei suas coisas.", "Já lhe mandei as suas coisas.")]
    [InlineData("Já lhes entreguei seus livros.", "Já lhes entreguei os seus livros.")]
    [InlineData("Preciso do seu contato.", "Preciso do teu contacto.")]
    [InlineData("Você esqueceu suas coisas.", "Esqueceste as tuas coisas.")]
    public void CueLevelPossessiveContext(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("- Já até mandei suas coisas.\n- Para onde?", "- Até, já lhe mandei as suas coisas.\n- Para onde?")]
    [InlineData("Já até enviei suas coisas.", "Até, já lhe enviei as suas coisas.")]
    [InlineData("Já devolvi seus livros.", "Já devolvi os seus livros.")]
    [InlineData("Já mandei suas roupas.", "Já mandei as suas roupas.")]
    public void PreviousCuePossessiveContext(string br, string pt)
    {
        Assert.Equal(pt, Converter.Convert(br, "E me divorciando do cretino."));
    }

    [Fact]
    public void PreviousContextDoesNotMutateConverterState()
    {
        const string text = "Já mandei suas roupas.";
        Assert.Equal("Já mandei as suas roupas.", Converter.Convert(text, "E me divorciando do cretino."));
        Assert.Equal("Já mandei as tuas roupas.", Converter.Convert(text));
        Assert.Equal("Esqueceste as tuas coisas.", Converter.Convert("Você esqueceu suas coisas.", "E me divorciando do cretino."));
    }

    [Theory]
    [InlineData("E me divorciando do cretino.", "E a divorciar-me do cretino.")]
    [InlineData("E me preparando.", "E a preparar-me.")]
    [InlineData("Ou me escondendo.", "Ou a esconder-me.")]
    [InlineData("E trabalhando em casa.", "E a trabalhar em casa.")]
    [InlineData("E pensando nisso.", "E pensando nisso.")]
    [InlineData("Por que está me olhando?\nEntra no carro, vai.",
        "Porque estás a olhar para mim?\nEntra no carro, vá.")]
    [InlineData("Por que está trabalhando?", "Porque estás a trabalhar?")]
    [InlineData("Como consegue fazer isso?", "Como consegues fazer isso?")]
    [InlineData("Por que ela está me olhando?", "Porque ela está a olhar para mim?")]
    [InlineData("Estou te olhando.", "Estou a olhar para ti.")]
    [InlineData("Estão nos olhando.", "Estão a olhar para nós.")]
    [InlineData("Estou me olhando.", "Estou a olhar para mim.")]
    [InlineData("Entra no carro, vai.", "Entra no carro, vá.")]
    [InlineData("Anda, vai!", "Anda, vá!")]
    [InlineData("Ele vai.", "Ele vai.")]
    [InlineData("Entra no carro, vai buscar ajuda.", "Entra no carro, vai buscar ajuda.")]
    [InlineData("Ele entra no carro, vai.", "Ele entra no carro, vai.")]
    public void CoordinatedGerundsAndLookAtQuestions(string br, string pt) => AssertConverts(br, pt);

    [Fact]
    public void NewProgressivePatternsRespectOptions()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Progressive: false));
        Assert.Equal("E me divorciando do cretino.", converter.Convert("E me divorciando do cretino."));
        converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        Assert.Equal("Porque está a olhar para mim?", converter.Convert("Por que está me olhando?"));
        converter = new PortugueseConverter(new ConversionOptions(Colloquial: false));
        Assert.Equal("Entra no carro, vai.", converter.Convert("Entra no carro, vai."));
    }

    [Theory]
    [InlineData("Agora, seu irmão Tiago,\npor outro lado...", "Agora, o teu irmão Tiago,\npor outro lado...")]
    [InlineData("Agora, sua irmã Maria,", "Agora, a tua irmã Maria,")]
    [InlineData("Entretanto, seus irmãos,", "Entretanto, os teus irmãos,")]
    [InlineData("No entanto, suas irmãs,", "No entanto, as tuas irmãs,")]
    [InlineData("Por outro lado, meu carro...", "Por outro lado, o meu carro...")]
    [InlineData("Agora, nossa casa,", "Agora, a nossa casa,")]
    [InlineData("Agora, nossos carros,", "Agora, os nossos carros,")]
    [InlineData("- Agora, seu irmão Tiago,", "- Agora, o teu irmão Tiago,")]
    [InlineData("<i>Agora, seu irmão Tiago,</i>", "<i>Agora, o teu irmão Tiago,</i>")]
    [InlineData("AGORA, SEU IRMÃO TIAGO,", "AGORA, O TEU IRMÃO TIAGO,")]
    [InlineData("Agora, o teu irmão Tiago,", "Agora, o teu irmão Tiago,")]
    [InlineData("Agora, meu amigo, ouve.", "Agora, meu amigo, ouve.")]
    [InlineData("Agora, meu Deus!", "Agora, meu Deus!")]
    [InlineData("Doutor, agora, seu irmão Tiago,", "Doutor, agora, seu irmão Tiago,")]
    public void IntroductoryPossessiveArticles(string br, string pt) => AssertConverts(br, pt);

    [Fact]
    public void IntroductoryPossessiveArticlesRespectDisabledPronouns()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Pronouns: false));
        Assert.Equal("Agora, teu irmão Tiago,", converter.Convert("Agora, seu irmão Tiago,"));
    }

    [Theory]
    [InlineData("Lembra quando me perguntou", "Lembras-te quando me perguntaste")]
    [InlineData("Lembra quando me perguntou?", "Lembras-te quando me perguntaste?")]
    [InlineData("Lembra quando me disse isso?", "Lembras-te quando me disseste isso?")]
    [InlineData("Lembra quando me ajudou?", "Lembras-te quando me ajudaste?")]
    [InlineData("Lembra quando me viu?", "Lembras-te quando me viste?")]
    [InlineData("Lembra quando nos trouxe comida?", "Lembras-te quando nos trouxeste comida?")]
    [InlineData("Lembra quando me telefonou?", "Lembras-te quando me telefonaste?")]
    [InlineData("Lembras-te quando me perguntou?", "Lembras-te quando me perguntaste?")]
    [InlineData("- Lembra quando me perguntou\nse eu estava vendo alguém?",
        "- Lembras-te quando me perguntaste\nse eu estava a ver alguém?")]
    [InlineData("<i>Lembra quando me perguntou?</i>", "<i>Lembras-te quando me perguntaste?</i>")]
    [InlineData("LEMBRA QUANDO ME PERGUNTOU?", "LEMBRAS-TE QUANDO ME PERGUNTASTE?")]
    [InlineData("Ele lembra quando me perguntou.", "Ele lembra quando me perguntou.")]
    [InlineData("Lembra quando ela me perguntou?", "Lembras-te quando ela me perguntou?")]
    [InlineData("Lembra quando me perguntou ela?", "Lembras-te quando me perguntou ela?")]
    [InlineData("Doutor, lembra quando me perguntou?", "Doutor, lembra quando me perguntou?")]
    [InlineData("Lembras-te quando me perguntaste?", "Lembras-te quando me perguntaste?")]
    public void RememberWhenAgreement(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("É a gente.", "Somos nós.")]
    [InlineData("Não é a gente.", "Não somos nós.")]
    [InlineData("Foi a gente!", "Fomos nós!")]
    [InlineData("Era a gente.", "Éramos nós.")]
    [InlineData("Será a gente?", "Seremos nós?")]
    [InlineData("Seria a gente.", "Seríamos nós.")]
    [InlineData("Espero que seja a gente.", "Espero que sejamos nós.")]
    [InlineData("Se fosse a gente.", "Se fôssemos nós.")]
    [InlineData("Se for a gente.", "Se formos nós.")]
    [InlineData("- É a gente.", "- Somos nós.")]
    [InlineData("<i>É a gente.</i>", "<i>Somos nós.</i>")]
    [InlineData("É A GENTE!", "SOMOS NÓS!")]
    [InlineData("É\na gente.", "Somos\nnós.")]
    [InlineData("É a gente da aldeia.", "É a gente da aldeia.")]
    [InlineData("É a gente que vive aqui.", "É a gente que vive aqui.")]
    [InlineData("É toda a gente.", "É toda a gente.")]
    [InlineData("Somos nós.", "Somos nós.")]
    public void WeIdentityAgreement(string br, string pt) => AssertConverts(br, pt);

    [Fact]
    public void NewAgreementRulesRespectOptionsAndIgnoredPhrases()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Colloquial: false, YouToTu: false));
        Assert.Equal("É a gente.", converter.Convert("É a gente."));
        Assert.Equal("Lembra quando me perguntou?", converter.Convert("Lembra quando me perguntou?"));
        var local = new PortugueseLocalWordList();
        local.Ignored.Add("É a gente");
        local.Ignored.Add("Lembra quando me perguntou");
        converter = new PortugueseConverter(localWordList: local);
        Assert.Equal("É a gente.", converter.Convert("É a gente."));
        Assert.Equal("Lembra quando me perguntou?", converter.Convert("Lembra quando me perguntou?"));
    }

    [Theory]
    [InlineData("Parabéns a suas famílias,", "Parabéns às vossas famílias,")]
    [InlineData("Parabéns às suas famílias!", "Parabéns às vossas famílias!")]
    [InlineData("PARABÉNS A SUAS FAMÍLIAS!", "PARABÉNS ÀS VOSSAS FAMÍLIAS!")]
    [InlineData("Parabéns a sua família.", "Parabéns a tua família.")]
    [InlineData("As suas famílias estão bem.", "As tuas famílias estão bem.")]
    [InlineData("- Sabe como é a ponte a essa hora.\n- Sei, estou indo.",
        "- Sabes como é a ponte a esta hora.\n- Sei, estou a ir.")]
    [InlineData("Sabe como é a ponte a esta hora.", "Sabes como é a ponte a esta hora.")]
    [InlineData("<i>Sabe como é.</i>", "<i>Sabes como é.</i>")]
    [InlineData("SABE COMO É.", "SABES COMO É.")]
    [InlineData("Ele sabe como é a ponte.", "Ele sabe como é a ponte.")]
    [InlineData("A mãe sabe como é a ponte.", "A mãe sabe como é a ponte.")]
    [InlineData("Sabe ela como é a ponte?", "Sabe ela como é a ponte?")]
    [InlineData("Doutor, sabe como é a ponte.", "Doutor, sabe como é a ponte.")]
    public void FamilyCongratulationsAndKnowHow(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("Tu sabe como é.", "Tu sabes como é.")]
    [InlineData("Tu tem razão.", "Tu tens razão.")]
    [InlineData("Tu fez isso.", "Tu fizeste isso.")]
    [InlineData("Tu não se lembra.", "Tu não te lembras.")]
    [InlineData("Tu fala português.", "Tu falas português.")]
    [InlineData("Tu já comeu.", "Tu já comeste.")]
    [InlineData("Tu sabe e ele sabe.", "Tu sabes e ele sabe.")]
    [InlineData("Tu programou isso.", "Tu programaste isso.")]
    [InlineData("Tu programaste isso.", "Tu programaste isso.")]
    [InlineData("Tu sabes como é.", "Tu sabes como é.")]
    [InlineData("Tu és doente.", "Tu és doente.")]
    [InlineData("TU SABE.", "TU SABES.")]
    [InlineData("Você mantém a calma.", "Manténs a calma.")]
    [InlineData("Você deteve o homem.", "Detiveste o homem.")]
    [InlineData("Você obtiver ajuda.", "Obtiveres ajuda.")]
    [InlineData("Você prevê problemas.", "Prevês problemas.")]
    [InlineData("Você previu problemas.", "Previste problemas.")]
    [InlineData("Você refaz o trabalho.", "Refazes o trabalho.")]
    [InlineData("Você desfez isso.", "Desfizeste isso.")]
    [InlineData("Você satisfaz o cliente.", "Satisfazes o cliente.")]
    public void BroaderSecondPersonAgreement(string br, string pt) => AssertConverts(br, pt);

    [Fact]
    public void AddressCorrectionsRespectDisabledOption()
    {
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        Assert.Equal("Parabéns a suas famílias.", converter.Convert("Parabéns a suas famílias."));
        Assert.Equal("Sabe como é.", converter.Convert("Sabe como é."));
        Assert.Equal("Tu sabe como é.", converter.Convert("Tu sabe como é."));
    }

    [Theory]
    [InlineData("programou", "programaste")]
    [InlineData("programava", "programavas")]
    [InlineData("programasse", "programasses")]
    [InlineData("programará", "programarás")]
    [InlineData("programaria", "programarias")]
    [InlineData("programar", "programares")]
    [InlineData("traduziu", "traduziste")]
    public void DictionaryValidatedInflections(string third, string second)
    {
        Assert.True(PortugueseVerbs.TryGuessThirdToSecond(third, out var result));
        Assert.Equal(second, result);
    }

    [Fact]
    public void UnknownInflectionsAreNotInvented()
    {
        Assert.False(PortugueseVerbs.TryGuessThirdToSecond("zzzzou", out _));
        Assert.Equal("Você zzzzou.", Converter.Convert("Você zzzzou."));
    }

    [Theory]
    [InlineData("Enfiou minha cabeça na privada!", "Enfiou a minha cabeça na sanita!")]
    [InlineData("A privada está suja.", "A sanita está suja.")]
    [InlineData("As privadas estão entupidas.", "As sanitas estão entupidas.")]
    [InlineData("NA PRIVADA!", "NA SANITA!")]
    [InlineData("A vida privada.", "A vida privada.")]
    [InlineData("Uma empresa privada.", "Uma empresa privada.")]
    [InlineData("Foi privada de liberdade.", "Foi privada de liberdade.")]
    [InlineData("A privada empresa fechou.", "A privada empresa fechou.")]
    [InlineData("O vaso sanitário.", "A sanita.")]
    [InlineData("Os vasos sanitários.", "As sanitas.")]
    [InlineData("Água sanitária.", "Lixívia.")]
    [InlineData("O botijão de gás.", "A botija de gás.")]
    [InlineData("Os botijões de gás.", "As botijas de gás.")]
    [InlineData("A pia da cozinha.", "O lava-loiça da cozinha.")]
    [InlineData("A pia do banheiro.", "O lavatório da casa de banho.")]
    [InlineData("Dar descarga.", "Puxar o autoclismo.")]
    [InlineData("Uma máquina de lavar louças.", "Uma máquina de lavar loiça.")]
    [InlineData("A cômoda.", "A cómoda.")]
    [InlineData("Os grampos de papel.", "Os agrafos.")]
    [InlineData("Os grampos de cabelo.", "Os ganchos de cabelo.")]
    [InlineData("Os maiôs.", "Os fatos de banho.")]
    [InlineData("Cadarços e zíperes.", "Atacadores e fechos-éclair.")]
    [InlineData("Esmalte de unhas.", "Verniz das unhas.")]
    [InlineData("Revistas em quadrinhos.", "Revistas de banda desenhada.")]
    [InlineData("Dubladores e dubladoras.", "Atores de dobragem e atrizes de dobragem.")]
    [InlineData("Academia de ginástica.", "Ginásio.")]
    [InlineData("A academia de ciências.", "A academia de ciências.")]
    [InlineData("O acostamento.", "A berma.")]
    [InlineData("Os acostamentos.", "As bermas.")]
    [InlineData("Rotatórias e conversíveis.", "Rotundas e descapotáveis.")]
    [InlineData("Um misto quente.", "Uma tosta mista.")]
    [InlineData("Os mistos quentes.", "As tostas mistas.")]
    [InlineData("O boteco.", "A tasca.")]
    [InlineData("Os caquis e as berinjelas.", "Os dióspiros e as beringelas.")]
    [InlineData("Calças caqui.", "Calças caqui.")]
    [InlineData("Abobrinha grelhada.", "Curgete grelhada.")]
    [InlineData("Falar abobrinha.", "Falar disparates.")]
    [InlineData("A farinha de rosca.", "O pão ralado.")]
    [InlineData("Leite integral.", "Leite gordo.")]
    [InlineData("As balas de menta.", "Os rebuçados de menta.")]
    [InlineData("Uma bala de goma.", "Uma goma.")]
    [InlineData("A bala atingiu a parede.", "A bala atingiu a parede.")]
    [InlineData("O canudo de plástico.", "A palhinha de plástico.")]
    [InlineData("O canudo do diploma.", "O canudo do diploma.")]
    [InlineData("Um resfriado.", "Uma constipação.")]
    [InlineData("O motor resfriado.", "O motor resfriado.")]
    [InlineData("Os ônibus escolares.", "Os autocarros escolares.")]
    [InlineData("Absorventes higiênicos.", "Pensos higiénicos.")]
    [InlineData("Esparadrapos e bandeides.", "Adesivos e pensos rápidos.")]
    public void ExpandedVocabulary(string br, string pt) => AssertConverts(br, pt);

    [Fact]
    public void ExpandedVocabulary_RespectsDisabledVocabulary()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Vocabulary: false));
        Assert.Equal("Na privada. Água sanitária. Cadarços.", converter.Convert("Na privada. Água sanitária. Cadarços."));
    }

    [Theory]
    [InlineData("É um fato.", "É um facto.")]
    [InlineData("Os fatos são claros.", "Os factos são claros.")]
    [InlineData("Ele é muito econômico.", "Ele é muito económico.")]
    [InlineData("Qual é o gênero do filme?", "Qual é o género do filme?")]
    [InlineData("O fenômeno é anônimo.", "O fenómeno é anónimo.")]
    [InlineData("Uma atitude acadêmica.", "Uma atitude académica.")]
    [InlineData("O bebê está dormindo.", "O bebé está a dormir.")]
    [InlineData("Venha conosco.", "Vem connosco.")]
    [InlineData("Ele tem dezesseis anos.", "Ele tem dezasseis anos.")]
    [InlineData("Preciso do seu contato.", "Preciso do teu contacto.")]
    [InlineData("Faça o registro.", "Faz o registo.")]
    [InlineData("Estamos planejando a viagem.", "Estamos a planear a viagem.")]
    [InlineData("Ele tem uma conseqüência.", "Ele tem uma consequência.")]
    [InlineData("Uma idéia heróica.", "Uma ideia heroica.")]
    [InlineData("ECONÔMICO", "ECONÓMICO")]
    [InlineData("Ele está no estômago.", "Ele está no estômago.")]
    [InlineData("Eles têm ciência disso.", "Eles têm ciência disso.")]
    [InlineData("O cônjuge dele.", "O cônjuge dele.")]
    public void Spelling(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("Vamos pegar o ônibus.", "Vamos apanhar o autocarro.")]
    [InlineData("Os ônibus estão atrasados.", "Os autocarros estão atrasados.")]
    [InlineData("O trem chegou.", "O comboio chegou.")]
    [InlineData("Me liga no celular.", "Liga-me no telemóvel.")]
    [InlineData("Onde fica o banheiro?", "Onde fica a casa de banho?")]
    [InlineData("A geladeira está vazia.", "O frigorífico está vazio.")]
    [InlineData("Uma geladeira nova.", "Um frigorífico novo.")]
    [InlineData("O nosso time ganhou.", "A nossa equipa ganhou.")]
    [InlineData("Não tenho tempo.", "Não tenho tempo.")]
    [InlineData("Olha a tela.", "Olha o ecrã.")]
    [InlineData("Ele pegou o trem.", "Ele apanhou o comboio.")]
    [InlineData("Pegue o próximo ônibus.", "Apanha o próximo autocarro.")]
    [InlineData("Pega a caneta.", "Pega na caneta.")]
    [InlineData("Ônibus", "Autocarro")]
    [InlineData("Comi um cachorro-quente.", "Comi um cachorro-quente.")]
    public void Vocabulary(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("E essa saia? Sério?", "E essa saia? A sério?")]
    [InlineData("Sério mesmo?", "A sério?")]
    [InlineData("Sério, eu juro.", "A sério, eu juro.")]
    [InlineData("Isso é sério.", "Isso é sério.")]
    [InlineData("Falando sério, vamos embora.", "A sério, vamos embora.")]
    [InlineData("Tchau, mãe.", "Adeus, mãe.")]
    [InlineData("Que saco!", "Que seca!")]
    [InlineData("Fica frio.", "Tem calma.")]
    [InlineData("De jeito nenhum!", "De maneira nenhuma!")]
    [InlineData("- Detetive, posso falar com você?", "- Detetive, posso falar consigo?")]
    [InlineData("Senhor, isto é para você.", "Senhor, isto é para si.")]
    [InlineData("Doutor, você está bem?", "Doutor, está bem?")]
    [InlineData("Senhora, você se lembra?", "Senhora, lembra-se?")]
    [InlineData("Posso falar com você?", "Posso falar contigo?")]
    [InlineData("e o síndico disse-lhe que", "e o encarregado do edifício disse-lhe que")]
    [InlineData("viu alguns miúdos pegarem", "viu alguns miúdos a pegar")]
    [InlineData("Vi o homem correndo.", "Vi o homem a correr.")]
    [InlineData("Eles querem que vocês vejam.", "Eles querem que vocês vejam.")]
    [InlineData("Ele saiu correndo.", "Ele saiu a correr.")]
    [InlineData("Passei o dia trabalhando.", "Passei o dia a trabalhar.")]
    [InlineData("Ele acabou ficando.", "Ele acabou por ficar.")]
    [InlineData("Pega o meu casaco.", "Pega no meu casaco.")]
    [InlineData("A polícia pegou-o.", "A polícia apanhou-o.")]
    [InlineData("Ele foi pego.", "Ele foi apanhado.")]
    [InlineData("Ela pegou uma gripe.", "Ela apanhou uma gripe.")]
    [InlineData("A casa vai pegar fogo.", "A casa vai pegar fogo.")]
    [InlineData("Chama o encanador.", "Chama o canalizador.")]
    [InlineData("Que bagunça!", "Que confusão!")]
    [InlineData("Ele usa um terno.", "Ele usa um fato.")]
    [InlineData("Isso é fofoca.", "Isso é mexerico.")]
    [InlineData("Ele pega a estrada todo dia.", "Ele apanha a estrada todo dia.")]
    [InlineData("Passou correndo por mim.", "Passou a correr por mim.")]
    [InlineData("E aí, beleza?", "Então, tudo bem?")]
    [InlineData("A beleza dela?", "A beleza dela?")]
    [InlineData("Ouvi eles cantando.", "Ouvi-os a cantar.")]
    [InlineData("O senhor quer café? Você quer?", "O senhor quer café? Quer?")]
    [InlineData("- Meu bebé!\n- Tudo bem!", "- O meu bebé!\n- Está tudo bem!")]
    [InlineData("Minha casa!", "A minha casa!")]
    [InlineData("Meu Deus!", "Meu Deus!")]
    [InlineData("Meu amor!", "Meu amor!")]
    [InlineData("Tudo bem?", "Tudo bem?")]
    [InlineData("Tudo bem, tudo bem.", "Está tudo bem, está tudo bem.")]
    [InlineData("É você quem precisa de um advogado, Tiago.", "És tu que precisas de um advogado, Tiago.")]
    [InlineData("Foi você que fez isto?", "Foste tu que fizeste isto?")]
    [InlineData("É você quem não sabe.", "És tu que não sabes.")]
    [InlineData("O quê, está me controlando?", "O quê, estás a controlar-me?")]
    [InlineData("a mamãe e o papai estarão lá\nesperando você.", "a mãe e o pai estarão lá\nà tua espera.")]
    [InlineData("Estou esperando você.", "Estou à tua espera.")]
    [InlineData("Esse policial aqui\ntentou me afogar.", "Este polícia aqui\ntentou afogar-me.")]
    [InlineData("Ele tentou me ajudar.", "Ele tentou ajudar-me.")]
    [InlineData("Ele tentou me ver.", "Ele tentou ver-me.")]
    [InlineData("Ele tentou me ouvir.", "Ele tentou ouvir-me.")]
    [InlineData("Ela tentava nos convencer.", "Ela tentava convencer-nos.")]
    [InlineData("- Cadê a mamãe?\n- Veremos ela já, já.", "- Onde está a mãe?\n- Vamos vê-la já, já.")]
    [InlineData("Veremos ele amanhã.", "Vamos vê-lo amanhã.")]
    [InlineData("Veremos elas amanhã.", "Vamos vê-las amanhã.")]
    [InlineData("Não veremos ela amanhã.", "Não a vamos ver amanhã.")]
    [InlineData("Veremos ela quando chegar.", "Vamos vê-la quando chegar.")]
    [InlineData("ou está procurando novidades, pai?", "ou está à procura de novidades, pai?")]
    [InlineData("Estou procurando o meu livro.", "Estou à procura do meu livro.")]
    [InlineData("Estamos procurando por ele.", "Estamos à procura dele.")]
    [InlineData("Ele está procurando um emprego.", "Ele está à procura de um emprego.")]
    [InlineData("Estava procurando a chave.", "Estava à procura da chave.")]
    [InlineData("Estou procurando entender.", "Estou a procurar perceber.")]
    [InlineData("O que está procurando?", "O que está a procurar?")]
    [InlineData("Estou procurando-o.", "Estou a procurá-lo.")]
    public void Improvements(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("A gente se conheceu na terapia.", "Conhecemo-nos na terapia.")]
    [InlineData("A gente não se fala há anos.", "Não nos falamos há anos.")]
    [InlineData("A gente é amigo.", "Somos amigos.")]
    [InlineData("Todo mundo se amava.", "Toda a gente se amava.")]
    [InlineData("-A gente consegue.", "-Conseguimos.")]
    [InlineData("Ele chamou a gente.", "Ele chamou-nos.")]
    [InlineData("Vem com a gente.", "Vem connosco.")]
    [InlineData("E se a gente deixar isso só entre nós?", "E se deixarmos isso só entre nós?")]
    [InlineData("Acha que ela vem?", "Achas que ela vem?")]
    [InlineData("Lembra do meu pai?", "Lembras-te do meu pai?")]
    [InlineData("Ele perdeu seu emprego.", "Ele perdeu o seu emprego.")]
    [InlineData("Seu idiota!", "Seu idiota!")]
    [InlineData("Isso é ruim.", "Isso é mau.")]
    [InlineData("Uma notícia ruim.", "Uma notícia má.")]
    [InlineData("É bom demais!", "É muito bom!")]
    [InlineData("Vou assistir o filme.", "Vou ver o filme.")]
    [InlineData("Escuta, cara.", "Ouve, meu.")]
    [InlineData("Isso te dá medo.", "Isso dá-te medo.")]
    [InlineData("Me desculpa.", "Desculpa.")]
    [InlineData("Valeu por vir.", "Obrigado por vir.")]
    [InlineData("Valeu a pena.", "Valeu a pena.")]
    [InlineData("Vim pegar minhas coisas.", "Vim buscar as minhas coisas.")]
    [InlineData("É o jeito certo de fazer.", "É a forma certa de fazer.")]
    [InlineData("Pesquisei seu namorado.", "Pesquisei o teu namorado.")]
    [InlineData("Queria que sua primeira vez fosse boa.", "Queria que a tua primeira vez fosse boa.")]
    [InlineData("Te vejo lá.", "Vemo-nos lá.")]
    [InlineData("Não fala.", "Não fales.")]
    [InlineData("Não se preocupa.", "Não te preocupes.")]
    [InlineData("Não faz mal.", "Não faz mal.")]
    [InlineData("Não vai acontecer de novo.", "Não vai acontecer de novo.")]
    [InlineData("Ele não fala português.", "Ele não fala português.")]
    [InlineData("Precisa de foco total.", "Precisas de foco total.")]
    [InlineData("Alguém precisa de dar um toque.", "Alguém precisa de dar um toque.")]
    [InlineData("Vivemos junto.", "Vivemos juntos.")]
    [InlineData("Não consigo achar o negócio.", "Não consigo encontrar o negócio.")]
    [InlineData("Vou achar que estás louca.", "Vou achar que estás louca.")]
    [InlineData("Falando nisso, liguei para a tua mãe.", "Por falar nisso, liguei para a tua mãe.")]
    [InlineData("- Obrigada, amiga. - Imagina.", "- Obrigada, amiga. - De nada.")]
    public void CorpusImprovements(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("A barca sai em dez minutos, pai.", "O barco sai em dez minutos, pai.")]
    [InlineData("A barca sai em\ndez minutos, pai.", "O barco sai em\ndez minutos, pai.")]
    [InlineData("A barca parte amanhã.", "O barco parte amanhã.")]
    [InlineData("As barcas partem amanhã.", "Os barcos partem amanhã.")]
    [InlineData("A barca é pequena.", "A barca é pequena.")]
    [InlineData("A barca chegou ontem.", "A barca chegou ontem.")]
    public void BarcaChangesToBarcoInDepartureContext(string br, string pt) => AssertConverts(br, pt);

    [Fact]
    public void BarcaDepartureConversionRespectsVocabularyOption()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Vocabulary: false));
        Assert.Equal("A barca sai em dez minutos, pai.", converter.Convert("A barca sai em dez minutos, pai."));
    }

    [Theory]
    [InlineData("O que está rolando aqui?", "O que está a acontecer aqui?")]
    [InlineData("Todos vão acabar me odiando.", "Todos vão acabar por me odiar.")]
    [InlineData("Passou anos me irritando.", "Passou anos a irritar-me.")]
    [InlineData("Estou me abrindo aqui e tentando ser otimista.", "Estou a abrir-me aqui e a tentar ser otimista.")]
    [InlineData("Estava ocupada criando o site.", "Estava ocupada a criar o site.")]
    [InlineData("Tem gente vivendo aqui.", "Há gente a viver aqui.")]
    [InlineData("Acho que tem alguém me seguindo.", "Acho que há alguém a seguir-me.")]
    [InlineData("Mandei mensagem dizendo que ia.", "Mandei mensagem a dizer que ia.")]
    [InlineData("- Recarregando!", "- A recarregar!")]
    [InlineData("Só conferindo.", "Só a verificar.")]
    [InlineData("Entendo.", "Percebo.")]
    [InlineData("Tendo dito isso, vamos.", "Tendo dito isso, vamos.")]
    [InlineData("Imaginei você fazendo sexo.", "Imaginei-te a fazer sexo.")]
    [InlineData("- Como? - Enterrando elas.", "- Como? - A enterrá-las.")]
    [InlineData("É bom ter você aqui.", "É bom ter-te aqui.")]
    [InlineData("Não tem ninguém aqui.", "Não há ninguém aqui.")]
    [InlineData("Ele tem alguém em casa.", "Ele tem alguém em casa.")]
    [InlineData("Não dá.", "Não dá.")]
    [InlineData("Ele atirou em mim!", "Ele disparou contra mim!")]
    [InlineData("Atirem!", "Disparem!")]
    [InlineData("Ele atirou a bola.", "Ele atirou a bola.")]
    [InlineData("Ok. E onde ele está?", "Está bem. E onde está ele?")]
    [InlineData("E o que ele quer?", "E o que ele quer?")]
    [InlineData("Podes vir comigo, hein?", "Podes vir comigo, não?")]
    [InlineData("Que puta humilhação.", "Que grande humilhação.")]
    [InlineData("Puta que o pariu.", "Merda.")]
    [InlineData("Filho da puta!", "Filho da puta!")]
    [InlineData("Vou checar o endereço no laptop.", "Vou verificar a morada no portátil.")]
    [InlineData("O prédio sumiu.", "O edifício desapareceu.")]
    [InlineData("Estou com saudade.", "Tenho saudades.")]
    [InlineData("Está bem, levante-se. Pegue.", "Está bem, levanta-te. Toma.")]
    [InlineData("Para de pegar no pé do Noah.", "Para de implicar com o Noah.")]
    [InlineData("- Vou pegar chá. - Está bem.", "- Vou buscar chá. - Está bem.")]
    [InlineData("Se você pedir comida, tudo bem.", "Se pedires comida, tudo bem.")]
    [InlineData("Ele quer enfraquecer você.", "Ele quer enfraquecer-te.")]
    [InlineData("É melhor você e o Rui irem primeiro.", "É melhor tu e o Rui irem primeiro.")]
    [InlineData("Achei que ele podia ser você.", "Achei que ele podia ser tu.")]
    [InlineData("Não é assim que obriga eles.", "Não é assim que os obriga.")]
    [InlineData("Vai se foder.", "Vai-te foder.")]
    [InlineData("Se não quer, beleza.", "Se não quer, tudo bem.")]
    [InlineData("Mano, valeu!", "Mano, obrigado!")]
    [InlineData("Tá, cai fora!", "Está bem, sai daqui!")]
    [InlineData("Ele ia te dopar e depois te afogar.", "Ele ia dopar-te e depois afogar-te.")]
    [InlineData("Tá.", "Está bem.")]
    [InlineData("Uns 42 minutos, seu tarado.", "Uns 42 minutos, seu tarado.")]
    [InlineData("Ontem eu a deixei puta da vida.", "Ontem eu a deixei furiosa.")]
    [InlineData("É, estou vendo dobrado, Pereira.", "É, estou a ver a dobrar, Pereira.")]
    [InlineData("Estou vendo tudo dobrado.", "Estou a ver tudo a dobrar.")]
    [InlineData("Ele via dobrado.", "Ele via a dobrar.")]
    [InlineData("Coloque-me na espera, que eu espero.", "Põe-me em espera, que eu espero.")]
    [InlineData("Tens dúvidas sobre ser policial?", "Tens dúvidas sobre ser polícia?")]
    [InlineData("Pegam muitos peixes aqui.", "Apanham muitos peixes aqui.")]
    [InlineData("Pegam muitas coisas aqui.", "Apanham muitas coisas aqui.")]
    [InlineData("Peguei várias moedas ali.", "Apanhei várias moedas ali.")]
    [InlineData("Pegam muitos objetos aqui.", "Apanham muitos objetos aqui.")]
    [InlineData("Pegue as flores.", "Apanha as flores.")]
    [InlineData("Pegaremos cogumelos amanhã.", "Apanharemos cogumelos amanhã.")]
    [InlineData("Pegam muitas\ncoisas aqui.", "Apanham muitas\ncoisas aqui.")]
    [InlineData("PEGUEM MUITAS COISAS AQUI.", "APANHEM MUITAS COISAS AQUI.")]
    [InlineData("Pega a arma.", "Pega na arma.")]
    [InlineData("Pega em muitas coisas aqui.", "Pega em muitas coisas aqui.")]
    [InlineData("Mas não hoje.", "Mas hoje não.")]
    [InlineData("Ser os restos da miúda num contêiner.", "Ser os restos da miúda num contentor.")]
    [InlineData("O teu irmão não foi morto numa batida que deu errado.", "O teu irmão não foi morto numa pancada que correu mal.")]
    [InlineData("E não é esse pestinha aqui.", "E não é este pestinha aqui.")]
    [InlineData("O assado vai esfriar.", "O assado vai arrefecer.")]
    [InlineData("Miúda católica, mesma idade da Teresa, desaparecida.", "Miúda católica, da mesma idade da Teresa, desaparecida.")]
    [InlineData("Porque não acordas um dia aos 50 e vira um pervertido.", "Porque não acordas um dia aos 50 e tornas-te um pervertido.")]
    [InlineData("Pare de drama, só me diga.", "Deixa-te de dramas, diz-me só.")]
    [InlineData("Dê uma olhada nos jornais.", "Dá uma vista de olhos aos jornais.")]
    [InlineData("Não dá para dar uma olhada?", "Não dá para dar uma vista de olhos?")]
    [InlineData("Vou dar uma olhada na Capstone.", "Vou dar uma vista de olhos à Capstone.")]
    [InlineData("Está tudo sob controle.", "Está tudo sob controlo.")]
    [InlineData("Atenção, Controle.", "Atenção, Controlo.")]
    public void SecondCorpusImprovements(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("Não acho meu iPod.", "Não encontro o meu iPod.")]
    [InlineData("Nunca acharam o corpo.", "Nunca encontraram o corpo.")]
    [InlineData("Achei o corpo.", "Encontrei o corpo.")]
    [InlineData("A boneca foi achada.", "A boneca foi encontrada.")]
    [InlineData("Ele foi achado culpado.", "Ele foi achado culpado.")]
    [InlineData("Acharam a carrinha.", "Encontraram a carrinha.")]
    [InlineData("Acho que ele vem.", "Acho que ele vem.")]
    [InlineData("Acho o carro bonito.", "Acho o carro bonito.")]
    [InlineData("Acho a faca estranha.", "Acho a faca estranha.")]
    [InlineData("Achei o carro desconfortável.", "Achei o carro desconfortável.")]
    [InlineData("Acho o carro muito velho.", "Acho o carro muito velho.")]
    [InlineData("Dei sorte de chegar.", "Tive sorte de chegar.")]
    [InlineData("Ele deu sorte.", "Ele teve sorte.")]
    [InlineData("Isso dá sorte.", "Isso dá sorte.")]
    [InlineData("Esqueci de carregá-lo.", "Esqueci-me de carregá-lo.")]
    [InlineData("Esqueci que temos uma reunião.", "Esqueci-me de que temos uma reunião.")]
    [InlineData("ESQUECI QUE TEMOS UMA REUNIÃO.", "ESQUECI-ME DE QUE TEMOS UMA REUNIÃO.")]
    [InlineData("Esqueci disso.", "Esqueci-me disso.")]
    [InlineData("Esqueci-me disso.", "Esqueci-me disso.")]
    [InlineData("Nunca me esqueci de nenhum.", "Nunca me esqueci de nenhum.")]
    [InlineData("Nunca esqueci disso.", "Nunca me esqueci disso.")]
    [InlineData("Serem policiais antigos.", "Serem polícias antigos.")]
    [InlineData("Sem digitais.", "Sem impressões digitais.")]
    [InlineData("Temos digitais e uma foto.", "Temos impressões digitais e uma foto.")]
    [InlineData("Análise de digitais e sangue.", "Análise de impressões digitais e sangue.")]
    [InlineData("Temos impressões digitais.", "Temos impressões digitais.")]
    [InlineData("Fotografias digitais.", "Fotografias digitais.")]
    [InlineData("Cheque os vídeos.", "Verifica os vídeos.")]
    [InlineData("O cheque está na mesa.", "O cheque está na mesa.")]
    [InlineData("Seres policial.", "Seres polícia.")]
    [InlineData("Precisamos de policiais.", "Precisamos de polícias.")]
    [InlineData("Brutalidade policial.", "Brutalidade policial.")]
    [InlineData("Uma investigação policial.", "Uma investigação policial.")]
    [InlineData("Van branca.", "Carrinha branca.")]
    [InlineData("Van Morrison.", "Van Morrison.")]
    [InlineData("Vi a entrevista coletiva.", "Vi a conferência de imprensa.")]
    [InlineData("Conheces a mídia.", "Conheces a comunicação social.")]
    [InlineData("As enquetes.", "As sondagens.")]
    [InlineData("A louça dela.", "A loiça dela.")]
    [InlineData("Senso de humor.", "Sentido de humor.")]
    [InlineData("Senso comum.", "Senso comum.")]
    [InlineData("Vamos fazer tudo que pudermos.", "Vamos fazer tudo o que pudermos.")]
    [InlineData("Francis, esta é a Oficial Romano.", "Francis, esta é a Oficial Romano.")]
    public void SubtitleReviewImprovements(string br, string pt) => AssertConverts(br, pt);


    [Theory]
    [InlineData("Estou fazendo o jantar.", "Estou a fazer o jantar.")]
    [InlineData("Ele está dizendo a verdade.", "Ele está a dizer a verdade.")]
    [InlineData("Estávamos saindo.", "Estávamos a sair.")]
    [InlineData("O que você está pondo aí?", "O que estás a pôr aí?")]
    [InlineData("Estou te dizendo.", "Estou a dizer-te.")]
    [InlineData("Ela está se vestindo.", "Ela está a vestir-se.")]
    [InlineData("Continua chovendo.", "Continua a chover.")]
    [InlineData("Está lindo.", "Está lindo.")]
    [InlineData("Estou só olhando.", "Estou só a olhar.")]
    [InlineData("Ele está no fundo.", "Ele está no fundo.")]
    public void Progressive(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("Me diga a verdade.", "Diz-me a verdade.")]
    [InlineData("Te amo.", "Amo-te.")]
    [InlineData("Eu te amo.", "Eu amo-te.")]
    [InlineData("Ele se chama João.", "Ele chama-se João.")]
    [InlineData("Vou te matar!", "Vou matar-te!")]
    [InlineData("Posso te ajudar?", "Posso ajudar-te?")]
    [InlineData("Eu vi ele ontem.", "Eu vi-o ontem.")]
    [InlineData("Não vi ele.", "Não o vi.")]
    [InlineData("Nos vemos amanhã.", "Vemo-nos amanhã.")]
    [InlineData("Eu não te conheço.", "Eu não te conheço.")]
    [InlineData("Ele disse ele mesmo.", "Ele disse ele mesmo.")]
    [InlineData("Ela o viu.", "Ela viu-o.")]
    [InlineData("Um cara me seguiu.", "Um tipo seguiu-me.")]
    [InlineData("O homem que me seguiu.", "O homem que me seguiu.")]
    [InlineData("Meu pai não gosta disso.", "O meu pai não gosta disso.")]
    [InlineData("Meu Deus, o que foi isso?", "Meu Deus, o que foi isso?")]
    public void Pronouns(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("Tá bom.", "Está bem.")]
    [InlineData("Tô cansado.", "Estou cansado.")]
    [InlineData("Vou pra casa.", "Vou para casa.")]
    [InlineData("Cadê o Pedro?", "Onde está o Pedro?")]
    [InlineData("Cadê os meninos?", "Onde estão os meninos?")]
    [InlineData("Tenho que ir.", "Tenho de ir.")]
    [InlineData("Isso não tem que ver com isso.", "Isso não tem a ver com isso.")]
    [InlineData("Oi, tudo bem?", "Olá, tudo bem?")]
    [InlineData("Deixa eu ver.", "Deixa-me ver.")]
    [InlineData("Ele ficou bravo.", "Ele ficou zangado.")]
    [InlineData("Legal!", "Fixe!")]
    [InlineData("Fica calmo, tá?", "Fica calmo, está bem?")]
    [InlineData("Onde tá o meu celular?", "Onde está o meu telemóvel?")]
    [InlineData("Tenho certeza disso.", "Tenho a certeza disso.")]
    [InlineData("Preciso ir ao banheiro.", "Preciso de ir à casa de banho.")]
    [InlineData("Preciso te ver.", "Preciso de te ver.")]
    [InlineData("Me desculpa, eu não queria te machucar.", "Desculpa, eu não queria magoar-te.")]
    [InlineData("Ele se machucou.", "Ele magoou-se.")]
    [InlineData("Ele dirige muito mal.", "Ele conduz muito mal.")]
    [InlineData("Ela vai dirigir o filme.", "Ela vai dirigir o filme.")]
    public void Colloquial(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("Você tem razão.", "Tens razão.")]
    [InlineData("Você está bem?", "Estás bem?")]
    [InlineData("Você quer ir?", "Queres ir?")]
    [InlineData("Você falou com ele?", "Falaste com ele?")]
    [InlineData("Vou com você.", "Vou contigo.")]
    [InlineData("Isto é para você.", "Isto é para ti.")]
    [InlineData("Eu amo você.", "Eu amo-te.")]
    [InlineData("Obrigado, e você?", "Obrigado, e tu?")]
    [InlineData("Você se lembra?", "Lembras-te?")]
    [InlineData("Você não se lembra?", "Não te lembras?")]
    [InlineData("Vocês estão bem?", "Vocês estão bem?")]
    [InlineData("Se você quiser, eu vou.", "Se quiseres, eu vou.")]
    [InlineData("Foi você?", "Foste tu?")]
    [InlineData("O que você fez?", "O que fizeste?")]
    [InlineData("Eu sei que você me ama.", "Eu sei que me amas.")]
    [InlineData("VOCÊ TEM RAZÃO", "TENS RAZÃO")]
    [InlineData("Você zarpou?", "Zarpaste?")]
    [InlineData("Você cansada?", "Você cansada?")]
    [InlineData("Eu sei que você se importa.", "Eu sei que te importas.")]
    public void YouToTu(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("Então soube que chegou cedo.", "Then I found out you arrived early.", "Então soube que chegaste cedo.")]
    [InlineData("Sabe que fui um oficial.", "You know I was an officer.", "Sabes que fui um oficial.")]
    [InlineData("Pensa que trabalhou muito.", "You think you worked hard.", "Pensas que trabalhaste muito.")]
    [InlineData("Sabe o que fez ontem?", "You know what you did yesterday?", "Sabes o que fizeste ontem?")]
    [InlineData("Sabe o que era?", "You know what you were?", "Sabes o que eras?")]
    [InlineData("O que está fazendo aqui?", "What are you doing here?", "O que estás a fazer aqui?")]
    [InlineData("O que pode fazer?", "What can you do?", "O que podes fazer?")]
    [InlineData("Sei o que quer dizer.", "I know what you mean.", "Sei o que queres dizer.")]
    [InlineData("Ele sabe que foi para casa.", "He knows he went home.", "Ele sabe que foi para casa.")]
    [InlineData("Então soube que chegou cedo.", "Then I found out he arrived early.", "Então soube que chegou cedo.")]
    [InlineData("O que aconteceu?", "What happened to you?", "O que aconteceu?")]
    public void ReferenceGuidesDroppedSecondPersonSubjects(string br, string english, string pt) =>
        Assert.Equal(pt, Converter.Convert(br, null, ReferenceHint.FromText(english)));

    [Fact]
    public void DroppedSecondPersonWithoutReferenceIsNotGuessed()
    {
        Assert.Equal("Sabe que fui um oficial.", Converter.Convert("Sabe que fui um oficial."));
    }

    [Theory]
    [InlineData("Por isso que nunca namorou nenhum tira?", "Is that why you never dated any cops?", "Por isso é que nunca namoraste um polícia?")]
    [InlineData("Por isso que nunca visitou nenhuma ilha?", "Is that why you never visited any islands?", "Por isso é que nunca visitaste uma ilha?")]
    [InlineData("Por isso que já chegou?", "Is that why you already arrived?", "Por isso é que já chegaste?")]
    [InlineData("Por isso que não quer sair?", "Is that why you don't want to leave?", "Por isso é que não queres sair?")]
    [InlineData("É por isso que nunca trabalhou aqui?", "Is that why you never worked here?", "É por isso que nunca trabalhaste aqui?")]
    [InlineData("Foi por isso que nunca veio?", "Was that why you never came?", "Foi por isso que nunca vieste?")]
    [InlineData("Por isso que nunca\nnamorou nenhum tira?", "Is that why you never dated any cops?", "Por isso é que nunca\nnamoraste um polícia?")]
    [InlineData("POR ISSO QUE NUNCA FALOU?", "Is that why you never spoke?", "POR ISSO É QUE NUNCA FALASTE?")]
    [InlineData("<i>Por isso que nunca falou?</i>", "Is that why you never spoke?", "<i>Por isso é que nunca falaste?</i>")]
    [InlineData("Por isto que não veio?", "Is this why you didn't come?", "Por isto é que não vieste?")]
    public void CausalClausesUseReferenceSubjectAcrossVerbs(string source, string english, string expected)
    {
        var hint = ReferenceHint.FromText(english);
        Assert.True(hint!.SecondPersonSubject);
        Assert.Equal(expected, Converter.Convert(source, null, hint));
        Assert.Equal(expected, Converter.Convert(expected, null, hint));
    }

    [Theory]
    [InlineData("Por isso que nunca namorou?", "Is that why he never dated?", "Por isso é que nunca namorou?")]
    [InlineData("Por isso que ele nunca namorou?", "Is that why he never dated?", "Por isso é que ele nunca namorou?")]
    [InlineData("Por isso que eu nunca namorei?", "Is that why I never dated?", "Por isso é que eu nunca namorei?")]
    [InlineData("Por isso que a Ana nunca namorou?", "Is that why you think Ana never dated?", "Por isso é que a Ana nunca namorou?")]
    [InlineData("Por isso que nunca falou de você?", "Is that why he never spoke about you?", "Por isso é que nunca falou de ti?")]
    [InlineData("Por isso que nunca namorou?", null, "Por isso é que nunca namorou?")]
    [InlineData("Por isso vim.", "That's why I came.", "Por isso vim.")]
    [InlineData("Eu lutei por isso que mencionou.", null, "Eu lutei por isso que mencionou.")]
    [InlineData("Por isso é que nunca namoraste?", "Is that why you never dated?", "Por isso é que nunca namoraste?")]
    public void CausalClausesDoNotGuessSubjectsOrDuplicateFocus(string source, string? english, string expected) =>
        Assert.Equal(expected, Converter.Convert(source, null, ReferenceHint.FromText(english)));

    [Fact]
    public void CausalClausePreservesFormalReferenceContext()
    {
        var hint = ReferenceHint.FromText("Is that why you never dated any officers?")! with { FormalAddress = true };
        Assert.Equal("Por isso é que nunca namorou um agente?",
            Converter.Convert("Por isso que nunca namorou nenhum polícia?", null, hint));
        var off = new PortugueseConverter(ConversionOptions.Default with { Colloquial = false });
        Assert.Equal("Por isso que nunca namoraste nenhum polícia?",
            off.Convert("Por isso que nunca namorou nenhum polícia?", null, ReferenceHint.FromText("Is that why you never dated any officers?")));
    }

    [Theory]
    [InlineData("Nunca vi nenhum castelo.", "I never saw any castles.", "Nunca vi um castelo.")]
    [InlineData("Nunca visitou nenhuma ilha.", "She never visited any islands.", "Nunca visitou uma ilha.")]
    [InlineData("Nunca vi nenhum castelo.", "I never saw a castle.", "Nunca vi nenhum castelo.")]
    [InlineData("Não vi nenhum castelo.", "I didn't see any castles.", "Não vi nenhum castelo.")]
    [InlineData("Nunca nenhum polícia entrou.", "No cop ever entered.", "Nunca nenhum polícia entrou.")]
    [InlineData("Nunca foi nenhum problema.", "It was never any trouble.", "Nunca foi nenhum problema.")]
    public void NeverAnyObjectUsesReferenceAndKeepsNegation(string source, string english, string expected) =>
        Assert.Equal(expected, Converter.Convert(source, null, ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Não pense que porque seu velho é o chefe.", "Não penses que porque o teu velho é o chefe.")]
    [InlineData("que vou facilitar pra você.", "que te vou facilitar.")]
    [InlineData("que eu vou facilitar para você.", "que eu te vou facilitar.")]
    public void AddressedCommandsAndDativeInfinitives(string br, string pt) => AssertConverts(br, pt);

    [Theory]
    [InlineData("<i>Estou fazendo isso.</i>", "<i>Estou a fazer isso.</i>")]
    [InlineData("{\\an8}O trem chegou.", "{\\an8}O comboio chegou.")]
    [InlineData("<font color=\"#ff0000\">tela</font>", "<font color=\"#ff0000\">ecrã</font>")]
    [InlineData("- Oi!\n- Tá bom.", "- Olá!\n- Está bem.")]
    public void KeepsTagsAndLines(string br, string pt) => AssertConverts(br, pt);

    [Fact]
    public void Options_DisableGroups()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Spelling: false, Vocabulary: false, Progressive: false, Pronouns: false, Colloquial: false, YouToTu: false));
        const string text = "Você tá fazendo um fato no trem, me diga.";
        Assert.False(converter.TryConvert(text, out _));
    }

    [Fact]
    public void Options_OnlySpelling()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Vocabulary: false, Progressive: false, Pronouns: false, Colloquial: false, YouToTu: false));
        Assert.Equal("Estou fazendo um facto no trem.", converter.Convert("Estou fazendo um fato no trem."));
    }

    [Fact]
    public void LocalWordList_AddsAndIgnores()
    {
        var list = new PortugueseLocalWordList();
        list.Words.Add(("parada", "coisa"));
        list.Ignored.Add("trem");
        var converter = new PortugueseConverter(null, list);
        Assert.Equal("Que coisa é essa no trem?", converter.Convert("Que parada é essa no trem?"));
    }

    [Fact]
    public void BuiltInList_HasNoDuplicateOrIdentityEntries()
    {
        var words = PortugueseConverter.BuiltInWords;
        var duplicates = words.GroupBy(w => w.Br, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.Empty(duplicates);
        Assert.DoesNotContain(words, w => string.Equals(w.Br, w.Pt, StringComparison.Ordinal));
    }
}

public sealed class VerbVocabularyTests
{
    private static readonly PortugueseConverter Converter = new(ConversionOptions.Default);

    [Theory]
    [InlineData("Botei o meu carro na garagem.", "Meti o meu carro na garagem.")]
    [InlineData("Bota isso aí.", "Mete isso aí.")]
    [InlineData("Me bota no carro.", "Mete-me no carro.")]
    [InlineData("Vou botá-lo na cela.", "Vou metê-lo na cela.")]
    [InlineData("Estou botando as coisas no lugar.", "Estou a meter as coisas no lugar.")]
    [InlineData("Ele a botou no carro.", "Ele meteu-a no carro.")]
    [InlineData("Você bota tudo.", "Metes tudo.")]
    [InlineData("Se você botar o carro aí...", "Se meteres o carro aí...")]
    [InlineData("Eles xingaram muito.", "Eles insultaram muito.")]
    [InlineData("Checaremos o resto.", "Verificaremos o resto.")]
    [InlineData("Esquente a sopa.", "Aquece a sopa.")]
    [InlineData("Ele pulou da janela.", "Ele saltou da janela.")]
    [InlineData("Jogue o lixo fora.", "Deita o lixo fora.")]
    [InlineData("Jogou tudo fora?", "Deitou tudo fora?")]
    [InlineData("Ele arrumou um emprego.", "Ele arranjou um emprego.")]
    [InlineData("Não quero arrumar problemas.", "Não quero arranjar problemas.")]
    [InlineData("Estão estressados.", "Estão stressados.")]
    [InlineData("Fomos flagrados.", "Fomos apanhados.")]
    [InlineData("O avião aterrissou.", "O avião aterrou.")]
    [InlineData("Eles moravam juntos.", "Eles viviam juntos.")]
    [InlineData("A gente deletou tudo.", "Apagámos tudo.")]
    [InlineData("Ela posta fotos.", "Ela publica fotos.")]
    [InlineData("Quem bagunçou o quarto?", "Quem desarrumou o quarto?")]
    [InlineData("Achei que ela entenderia.", "Achei que ela perceberia.")]
    public void BrazilianVerbsBecomeEuropean(string text, string expected) => Assert.Equal(expected, Converter.Convert(text));

    [Theory]
    [InlineData("Calcei as botas.")]
    [InlineData("Comprei botas novas.")]
    [InlineData("botas de trabalho e um cinto.")]
    [InlineData("A bota dela.")]
    [InlineData("Há 330 mil postes na cidade.")]
    [InlineData("Dá um pulo lá.")]
    [InlineData("Ela ficou de tromba.")]
    [InlineData("O cheque voltou.")]
    [InlineData("Ele jogou futebol.")]
    [InlineData("Arruma o quarto.")]
    [InlineData("Entendido.")]
    [InlineData("Ele botou fogo na casa.")]
    [InlineData("Ele esquentou a cabeça.")]
    public void NounsAndOtherMeaningsAreKept(string text) => Assert.Equal(text, Converter.Convert(text));

    [Fact]
    public void OwnWordListEntryWinsOverGeneratedForms()
    {
        var local = new PortugueseLocalWordList();
        local.Words.Add(("botar", "pôr"));
        var converter = new PortugueseConverter(ConversionOptions.Default, local);
        Assert.Equal("Vou pôr aqui. Botei ali.", converter.Convert("Vou botar aqui. Botei ali."));
    }

    [Fact]
    public void ConjugatorKeepsTheConsonantSound()
    {
        Assert.True(VerbConjugator.TryConjugate("xingar", out var xingar));
        Assert.Contains("xinguei", xingar);
        Assert.Contains("xingue", xingar);
        Assert.True(VerbConjugator.TryConjugate("aquecer", out var aquecer));
        Assert.Contains("aqueço", aquecer);
        Assert.Contains("aquece", aquecer);
        Assert.Contains("aqueça", aquecer);
        Assert.Equal("metê", VerbConjugator.CliticInfinitive("meter"));
    }
}

public sealed class PlanearTests
{
    private static readonly PortugueseConverter Converter = new(ConversionOptions.Default);

    [Theory]
    [InlineData("Eu planejo tudo.", "Eu planeio tudo.")]
    [InlineData("Você planeja demais.", "Planeias demais.")]
    [InlineData("Ele planejou o assalto.", "Ele planeou o assalto.")]
    [InlineData("Eles planejaram tudo.", "Eles planearam tudo.")]
    [InlineData("Estou planejando uma festa.", "Estou a planear uma festa.")]
    [InlineData("Foi planejado.", "Foi planeado.")]
    [InlineData("Não planeje nada.", "Não planeie nada.")]
    [InlineData("Não planejes nada.", "Não planeies nada.")]
    [InlineData("Planejem bem.", "Planeiem bem.")]
    [InlineData("Vamos planejá-lo.", "Vamos planeá-lo.")]
    [InlineData("O planejamento falhou.", "O planeamento falhou.")]
    [InlineData("Ele planejava fugir.", "Ele planeava fugir.")]
    [InlineData("PLANEJEI TUDO.", "PLANEEI TUDO.")]
    public void PlanejarBecomesPlanear(string text, string expected) => Assert.Equal(expected, Converter.Convert(text));

    [Theory]
    [InlineData("Você fala demais.", "Falas demais.")]
    [InlineData("Ele reclama demais.", "Ele reclama demais.")]
    [InlineData("Isso é caro demais.", "Isso é muito caro.")]
    [InlineData("Caro demais!", "Muito caro!")]
    public void DemaisAfterAVerbStays(string text, string expected) => Assert.Equal(expected, Converter.Convert(text));
}

public sealed class SlangTests
{
    private static readonly PortugueseConverter Converter = new(ConversionOptions.Default);

    [Theory]
    [InlineData("Você é o tira que trabalhou\nno caso da Katie Snow,", "És o polícia que trabalhou\nno caso da Katie Snow,")]
    [InlineData("Ela foi estuprada.", "Ela foi violada.")]
    [InlineData("Ele estuprou duas mulheres.", "Ele violou duas mulheres.")]
    [InlineData("Foi um estupro.", "Foi uma violação.")]
    [InlineData("O estuprador fugiu.", "O violador fugiu.")]
    [InlineData("Jaqueta gasta, meio velha.", "Casaco gasto, meio velho.")]
    [InlineData("Usava uma jaqueta preta e rasgada.", "Usava um casaco preto e rasgado.")]
    [InlineData("A geladeira está quebrada.", "O frigorífico está quebrado.")]
    [InlineData("Que encrenca danada, cara.", "Que sarilho danado, meu.")]
    [InlineData("Nossa encrenca, cara.", "Nosso sarilho, meu.")]
    [InlineData("Manterei você informado.", "Manter-te-ei informado.")]
    [InlineData("Não manterei você aqui.", "Não te manterei aqui.")]
    [InlineData("Te ligarei amanhã.", "Ligar-te-ei amanhã.")]
    [InlineData("Eu te direi amanhã.", "Eu dir-te-ei amanhã.")]
    [InlineData("Eu o farei.", "Eu fá-lo-ei.")]
    [InlineData("Aproveite seu indulto.\nNão vai durar muito.", "Aproveita o teu indulto.\nNão vai durar muito.")]
    [InlineData("- Foi bom te ver e sua namorada.\n- Parceira.", "- Foi bom ver-te e a tua namorada.\n- Parceira.")]
    [InlineData("Que bom te ver!", "Que bom ver-te!")]
    [InlineData("Foi ótimo te conhecer.", "Foi ótimo conhecer-te.")]
    [InlineData("Obrigado por me receber.", "Obrigado por me receber.")]
    [InlineData("Pegue suas coisas e saia.", "Pega nas tuas coisas e sai.")]
    [InlineData("Me dê sua arma.", "Dá-me a tua arma.")]
    [InlineData("Diga a sua mãe que eu liguei.", "Diz à tua mãe que eu liguei.")]
    [InlineData("Diga a seu pai que eu liguei.", "Diz ao teu pai que eu liguei.")]
    [InlineData("Mostre a sua arma.", "Mostra a tua arma.")]
    [InlineData("Desculpe, tua mãe ligou.", "Desculpa, a tua mãe ligou.")]
    [InlineData("Este é o teu carro.", "Este é o teu carro.")]
    [InlineData("Aproveite.", "Aproveita.")]
    [InlineData("Isto é para ti.", "Isto é para ti.")]
    [InlineData("Dê pé! Vamos!\nLevanta!", "De pé! Vamos!\nLevanta-te!")]
    [InlineData("- Não!\n- Levante!", "- Não!\n- Levanta-te!")]
    [InlineData("Senta já!", "Senta-te já!")]
    [InlineData("Acalmem!", "Acalmem-se!")]
    [InlineData("Vamos, mexe!", "Vamos, mexe-te!")]
    [InlineData("Abaixa!", "Baixa-te!")]
    [InlineData("Levanta daí.", "Levanta-te daí.")]
    [InlineData("Levanta a mão.", "Levanta a mão.")]
    [InlineData("Ele levanta.", "Ele levanta.")]
    [InlineData("Levante-se!", "Levanta-te!")]
    [InlineData("Não dê pé para isso.", "Não dês pé para isso.")]
    [InlineData("- Essas duas.\n- King, lembra de mim?", "- Essas duas.\n- King, lembras-te de mim?")]
    [InlineData("Lembra de mim?", "Lembras-te de mim?")]
    [InlineData("Você lembra de mim?", "Lembras-te de mim?")]
    [InlineData("Se lembra de onde\nestava há duas noites?", "Lembras-te de onde\nestava há duas noites?")]
    [InlineData("Estávamos no hospital, lembra?", "Estávamos no hospital, lembras-te?")]
    [InlineData("Não lembra de nada?", "Não te lembras de nada?")]
    [InlineData("Lembra de trancar a porta.", "Lembra-te de trancar a porta.")]
    [InlineData("Lembre de trancar a porta.", "Lembra-te de trancar a porta.")]
    [InlineData("Ele lembra de mim.", "Ele lembra-se de mim.")]
    [InlineData("Eu não lembro.", "Eu não me lembro.")]
    [InlineData("Eu lembro de você.", "Eu lembro-me de ti.")]
    [InlineData("Eu também lembro disso.", "Eu também me lembro disso.")]
    [InlineData("Todos lembramos do\nhorrível mal entendido", "Todos nos lembramos do\nhorrível mal entendido")]
    [InlineData("Nós lembramos de tudo.", "Nós lembramo-nos de tudo.")]
    [InlineData("Ela não lembrava do rosto.", "Ela não se lembrava do rosto.")]
    [InlineData("Ele esqueceu da reunião.", "Ele esqueceu-se da reunião.")]
    [InlineData("Você esqueceu de mim?", "Esqueceste-te de mim?")]
    [InlineData("Não esquece de mim.", "Não te esqueças de mim.")]
    [InlineData("Não esqueça de ligar.", "Não te esqueças de ligar.")]
    [InlineData("Eu sei. Sinto muito.\nEsqueci.", "Eu sei. Sinto muito.\nEsqueci-me.")]
    [InlineData("Doutor, lembra de mim?", "Doutor, lembra-se de mim?")]
    [InlineData("Comissário, lembra de mim?", "Comissário, lembra-se de mim?")]
    [InlineData("Tiago, sabe onde está o carro?", "Tiago, sabes onde está o carro?")]
    [InlineData("Na verdade, nem me\nlembro dessa conversa.", "Na verdade, nem me\nlembro dessa conversa.")]
    [InlineData("Não me lembro de nada.", "Não me lembro de nada.")]
    [InlineData("Isso lembra o meu pai.", "Isso lembra o meu pai.")]
    [InlineData("Esqueci o nome dele.", "Esqueci o nome dele.")]
    [InlineData("Esquece isso.", "Esquece isso.")]
    [InlineData("Esquece!", "Esquece!")]
    [InlineData("Tudo o que lembro.", "Tudo o que me lembro.")]
    [InlineData("Botei o meu na reta para conseguir prender esse cara.", "Arrisquei o pescoço para conseguir prender esse tipo.")]
    [InlineData("Ele colocou o dele na reta por você.", "Ele arriscou o pescoço por ti.")]
    [InlineData("Não vou botar a minha na reta.", "Não vou arriscar o pescoço.")]
    [InlineData("Ela chega em casa e já reclama de tudo.", "Ela chegou a casa e já está a queixar-se de tudo.")]
    [InlineData("Quando cheguei em casa, ela tinha saído.", "Quando cheguei a casa, ela tinha saído.")]
    [InlineData("Ele chegou no hospital às duas.", "Ele chegou ao hospital às duas.")]
    [InlineData("Eles chegaram em um acordo.", "Eles chegaram a um acordo.")]
    [InlineData("Chegaste em tempo.", "Chegaste a tempo.")]
    [InlineData("Chego em dez minutos.", "Chego em dez minutos.")]
    [InlineData("Ele chegou na hora.", "Ele chegou na hora.")]
    [InlineData("Foi pessoal para nós,\nseu babaca.", "Foi pessoal para nós,\nseu parvo.")]
    [InlineData("Ele sai da cadeia e já viola\nsua ordem de proteção.", "Ele saiu da prisão e já está a violar\na sua ordem de proteção.")]
    [InlineData("Eles saem da escola e já fumam.", "Eles saíram da escola e já estão a fumar.")]
    [InlineData("Ele chega e já faz confusão.", "Ele chegou e já está a fazer confusão.")]
    [InlineData("Ele sai e já volta.", "Ele sai e já volta.")]
    [InlineData("E já sabe o que fazer.", "E já sabe o que fazer.")]
    [InlineData("A cadeia de comando é clara.", "A cadeia de comando é clara.")]
    [InlineData("Vais apodrecer\nna cadeia.", "Vais apodrecer\nna prisão.")]
    [InlineData("Além de aumentar o\nseu colesterol.", "Além de aumentar o\nteu colesterol.")]
    [InlineData("Por favor, apenas me mate.", "Por favor, mate-me apenas.")]
    [InlineData("Apenas me diga\ncomo o matou.", "Diz-me apenas\ncomo o matou.")]
    [InlineData("Apenas me deixe em paz!", "Deixa-me em paz!")]
    [InlineData("Simplesmente se acalme.", "Simplesmente acalme-se.")]
    [InlineData("Apenas me deixou muito... enojada.", "Apenas me deixou muito... enojada.")]
    [InlineData("Eu apenas me lembro disso.", "Eu apenas me lembro disso.")]
    [InlineData("será analisado pela Corregedoria,", "será analisado pelos Assuntos Internos,")]
    [InlineData("A Corregedoria vai investigar.", "Os Assuntos Internos vão investigar.")]
    [InlineData("Ele é da Corregedoria.", "Ele é dos Assuntos Internos.")]
    [InlineData("investigador da Assuntos Internos?", "investigador dos Assuntos Internos?")]
    [InlineData("O corregedor chegou.", "O inspetor chegou.")]
    [InlineData("A Corregedoria-Geral da Justiça abriu um processo.", "A Inspeção-Geral da Justiça abriu um processo.")]
    [InlineData("- Cale sua boca.", "- Cala-te.")]
    [InlineData("Cala a boca, Tiago.", "Cala-te, Tiago.")]
    [InlineData("Calem a boca!", "Calem-se!")]
    [InlineData("Bom. Cale a boca\ne continue a trabalhar.", "Bom. Cala-te\ne continua a trabalhar.")]
    [InlineData("Quero que cale a boca.", "Quero que cale a boca.")]
    [InlineData("Estou surpreso.", "Estou surpreendido.")]
    [InlineData("Fiquei surpresa com a notícia.", "Fiquei surpreendida com a notícia.")]
    [InlineData("Foi uma surpresa.", "Foi uma surpresa.")]
    [InlineData("Sim. Agradeça seu chefe por mim.", "Sim. Agradece ao teu chefe por mim.")]
    [InlineData("Temos de agradecer os bombeiros.", "Temos de agradecer aos bombeiros.")]
    [InlineData("Agradeço a Deus.", "Agradeço a Deus.")]
    [InlineData("Quero agradecer o favor.", "Quero agradecer o favor.")]
    [InlineData("Senta no teu trono.", "Senta no teu trono.")]
    [InlineData("Não quero que você\nperca o seu emprego.", "Não quero que percas\no teu emprego.")]
    [InlineData("Olhe, tua mãe está a caminho.", "Olha, a tua mãe está a caminho.")]
    [InlineData("Espere aqui.", "Espera aqui.")]
    [InlineData("Eles me queriam também.", "Eles me queriam também.")]
    [InlineData("Ele me sequestraria.", "Ele raptar-me-ia.")]
    [InlineData("Um estupro foi cometido.", "Uma violação foi cometida.")]
    [InlineData("Estupro é hediondo.", "Violação é hedionda.")]
    [InlineData("que vira um tira de patrulha.", "que se torna um polícia de patrulha.")]
    [InlineData("Só é preciso de dois tiras e\numas caixas velhas.", "Só é preciso de dois polícias e\numas caixas velhas.")]
    [InlineData("Joe morreu sendo um bom tira.", "Joe morreu sendo um bom polícia.")]
    [InlineData("Sou tira.", "Sou polícia.")]
    [InlineData("Matou um tira!", "Matou um polícia!")]
    [InlineData("Tiras de rua têm a pior visão.", "Polícias de rua têm a pior visão.")]
    [InlineData("Esse é o nosso tira corrupto.", "Esse é o nosso polícia corrupto.")]
    [InlineData("O tira tira o casaco.", "O polícia tira o casaco.")]
    [InlineData("Nossa, obrigada.", "Caramba, obrigada.")]
    [InlineData("-Minha nossa, é roubado?", "-Caramba, é roubado?")]
    [InlineData("Droga.", "Bolas.")]
    [InlineData("Droga, Ann.", "Bolas, Ann.")]
    [InlineData("disse que não o faz um tira ruim.", "disse que não o faz um polícia mau.")]
    [InlineData("Está de sacanagem.", "Está a gozar.")]
    [InlineData("Que droga!", "Que porcaria!")]
    [InlineData("É uma droga.", "É uma porcaria.")]
    [InlineData("Não sou dedo-duro.", "Não sou bufo.")]
    [InlineData("no caso de estar grampeado.", "no caso de estar sob escuta.")]
    [InlineData("Eles estão encrencados.", "Eles estão metidos em sarilhos.")]
    [InlineData("Isso é uma grana preta.", "Isso é um balúrdio.")]
    [InlineData("Aquele pilantra fugiu.", "Aquele vigarista fugiu.")]
    [InlineData("É só papo furado.", "É só conversa fiada.")]
    [InlineData("A ganhadora é a Sofia.", "A vencedora é a Sofia.")]
    [InlineData("Os ganhadores recebem o prémio.", "Os vencedores recebem o prémio.")]
    [InlineData("O promotor quer um acordo.", "O procurador quer um acordo.")]
    [InlineData("Não sabia que tiras e promotores\nestavam em lados opostos.", "Não sabia que polícias e procuradores\nestavam em lados opostos.")]
    [InlineData("A pesquisadora chegou.", "A investigadora chegou.")]
    [InlineData("Brincavam de tiras e ladrões.", "Brincavam de polícias e ladrões.")]
    [InlineData("Os bandidos e tiras sabiam.", "Os bandidos e polícias sabiam.")]
    [InlineData("A faxineira encontrou o corpo.", "A empregada de limpeza encontrou o corpo.")]
    [InlineData("Ele é um batedor de carteiras.", "Ele é um carteirista.")]
    [InlineData("Que cara de pau!", "Que descaramento!")]
    [InlineData("Ela é uma patricinha.", "Ela é uma betinha.")]
    [InlineData("O calouro chegou atrasado.", "O caloiro chegou atrasado.")]
    [InlineData("Há um motivo para a\nmaioridade legal ser 18 anos.", "Há um motivo para a\nmaioridade legal ser 18 anos.")]
    [InlineData("Ele estava mantendo\na distância mínima legal?", "Ele estava a manter\na distância mínima legal?")]
    [InlineData("Digo, a justiça ir atrás desse\ncara, por causa do porte.", "Digo, a justiça ir atrás desse\ntipo, por causa do porte.")]
    [InlineData("- Oi.\nCara, que susto.", "- Olá.\nMeu, que susto.")]
    [InlineData("Ele é um cara legal.", "Ele é um tipo fixe.")]
    public void BrazilianSlangBecomesEuropean(string text, string expected) => Assert.Equal(expected, Converter.Convert(text));

    [Theory]
    [InlineData("Tira a mão de mim!")]
    [InlineData("Vê se tira esse site do ar.")]
    [InlineData("A escola diz que ele\nsempre tira nota \"A\".")]
    [InlineData("Quem tira as fotos?")]
    [InlineData("Nossa vítima.")]
    [InlineData("Um novo tipo de droga.")]
    public void VerbTirarAndOtherMeaningsAreKept(string text) => Assert.Equal(text, Converter.Convert(text));
}

public sealed class PluralTemTests
{
    private static readonly PortugueseConverter Converter = new();

    [Theory]
    [InlineData("Tiras de rua tem a pior visão de tudo.", "Polícias de rua têm a pior visão de tudo.")]
    [InlineData("Os polícias tem de trabalhar.", "Os polícias têm de trabalhar.")]
    [InlineData("Todos os polícias tem armas.", "Todos os polícias têm armas.")]
    [InlineData("Os miúdos da escola já tem férias.", "Os miúdos da escola já têm férias.")]
    [InlineData("Dois homens tem a chave.", "Dois homens têm a chave.")]
    [InlineData("- Os agentes tem\nde sair.", "- Os agentes têm\nde sair.")]
    [InlineData("Os meus amigos tem-me ajudado.", "Os meus amigos têm-me ajudado.")]
    [InlineData("Estas caixas contém provas.", "Estas caixas contêm provas.")]
    [InlineData("Eles tem razão.", "Eles têm razão.")]
    [InlineData("Elas não tem culpa.", "Elas não têm culpa.")]
    [InlineData("Vocês tem dinheiro?", "Vocês têm dinheiro?")]
    [InlineData("Eles vem amanhã.", "Eles vêm amanhã.")]
    [InlineData("Disse que eles tem\nmedo.", "Disse que eles têm\nmedo.")]
    public void PluralSubjectGetsTem(string br, string pt) => Assert.Equal(pt, Converter.Convert(br));

    [Theory]
    [InlineData("Muitas vezes tem de esperar.")]
    [InlineData("Todos os dias tem algo novo.")]
    [InlineData("Às vezes tem razão.")]
    [InlineData("Depois tem de ir.")]
    [InlineData("Lucas tem razão.")]
    [InlineData("Carlos da Silva tem razão.")]
    [InlineData("Para eles tem de ser assim.")]
    [InlineData("Ele tem razão.")]
    [InlineData("O pai dos miúdos tem razão.")]
    public void SingularOrAdverbialKeepsTem(string text) => Assert.Equal(text, Converter.Convert(text));

    [Theory]
    [InlineData("Então seja bem-vinda ao clube.", "Então sê bem-vinda ao clube.")]
    [InlineData("Cadê você?", "Onde estás?")]
    [InlineData("Vá se foder!", "Vai-te foder!")]
    [InlineData("Vão se foder.", "Vão-se foder.")]
    [InlineData("Então me escute, seu viciado:", "Então escuta-me, seu viciado:")]
    [InlineData("Por quê?", "Porquê?")]
    [InlineData("Sabe por que ele saiu?", "Sabes porque ele saiu?")]
    [InlineData("O motivo por que vim.", "O motivo por que vim.")]
    [InlineData("Coloque no leite.", "Põe no leite.")]
    [InlineData("Pegue suas coisas e saia.", "Pega nas tuas coisas e sai.")]
    [InlineData("Pare de negar!", "Para de negar!")]
    [InlineData("Não fale com ela.", "Não fales com ela.")]
    [InlineData("Não se preocupe.", "Não te preocupes.")]
    [InlineData("Senhor, espere aqui.", "Senhor, espere aqui.")]
    [InlineData("Seja como for, vou.", "Seja como for, vou.")]
    [InlineData("-Estou tentando…", "-Estou a tentar…")]
    [InlineData("Tem uma vigia me mandando sair.", "Há uma vigia a mandar-me sair.")]
    [InlineData("Tem um elevador saindo do chão.", "Há um elevador a sair do chão.")]
    [InlineData("Comece descobrindo quem sabia.", "Começa por descobrir quem sabia.")]
    [InlineData("A hipotermia começa quando o corpo arrefece.", "A hipotermia começa quando o corpo arrefece.")]
    [InlineData("Nem fodendo.", "Nem pensar.")]
    [InlineData("Não tem como apagar.", "Não há forma de apagar.")]
    [InlineData("Sempre tem outra jogada.", "Há sempre outra jogada.")]
    [InlineData("Tem um minuto?", "Tem um minuto?")]
    [InlineData("Ele tem uma filha.", "Ele tem uma filha.")]
    [InlineData("Não tem nada a ver.", "Não tem nada a ver.")]
    [InlineData("Ela devia ter me deixado.", "Ela devia ter-me deixado.")]
    [InlineData("Ele não tinha me dito.", "Ele não me tinha dito.")]
    [InlineData("Eu teria me dado bem em Medicina.", "Eu ter-me-ia dado bem em Medicina.")]
    [InlineData("Achei que tivesses me deixado entrar.", "Achei que me tivesses deixado entrar.")]
    [InlineData("Talvez tenha se esquecido.", "Talvez se tenha esquecido.")]
    [InlineData("Obrigado por ter me colocado nessa situação.", "Obrigado por ter-me posto nessa situação.")]
    [InlineData("Eu me arrependo de uma coisa que eu disse.", "Eu arrependo-me de uma coisa que eu disse.")]
    [InlineData("Está disposto a se reconciliar.", "Está disposto a reconciliar-se.")]
    [InlineData("Noah, o que faz aqui?", "Noah, o que fazes aqui?")]
    [InlineData("O que quer dizer isso?", "O que quer dizer isso?")]
    [InlineData("Pelo bem de tudo que criamos aqui.", "Pelo bem de tudo o que criamos aqui.")]
    [InlineData("Agradeço, mas dessa vez é diferente.", "Agradeço, mas desta vez é diferente.")]
    [InlineData("Eu falei que ele vinha.", "Eu disse que ele vinha.")]
    [InlineData("Se falar que eu trabalho aqui, morre.", "Se falar que eu trabalho aqui, morre.")]
    [InlineData("Foi mal.", "Desculpa.")]
    [InlineData("O teste foi mal.", "O teste foi mal.")]
    [InlineData("É sério?", "A sério?")]
    [InlineData("Uma arma de verdade.", "Uma arma a sério.")]
    [InlineData("O que houve?", "O que se passou?")]
    [InlineData("Bateram na porta.", "Bateram à porta.")]
    [InlineData("Nós dois sabemos.", "Nós os dois sabemos.")]
    [InlineData("Cheiro de queimado.", "Cheiro a queimado.")]
    [InlineData("Não vamos a lugar nenhum.", "Não vamos a lado nenhum.")]
    [InlineData("Peço desculpas.", "Peço desculpa.")]
    [InlineData("Enquanto isso, ele saiu.", "Entretanto, ele saiu.")]
    [InlineData("Faz dois anos que saí.", "Há dois anos que saí.")]
    [InlineData("Ele faz anos amanhã.", "Ele faz anos amanhã.")]
    [InlineData("Na época, eu era novo.", "Na altura, eu era novo.")]
    [InlineData("Opa! Desculpa.", "Epá! Desculpa.")]
    [InlineData("Puxa! Que susto.", "Bolas! Que susto.")]
    [InlineData("Puxe o manifesto.", "Puxa o manifesto.")]
    [InlineData("Ô, Noah!", "Ó, Noah!")]
    [InlineData("Cansei de esperar.", "Fartei-me de esperar.")]
    [InlineData("É da hora.", "É fixe.")]
    [InlineData("Já era.", "Acabou.")]
    [InlineData("Já era tarde.", "Já era tarde.")]
    [InlineData("-Cai fora daqui.", "-Sai daqui.")]
    [InlineData("Vai chegar rapidinho no green.", "Vai chegar num instante no green.")]
    [InlineData("Ela vazou o vídeo.", "Ela divulgou o vídeo.")]
    [InlineData("Amei o presente.", "Adorei o presente.")]
    [InlineData("Quebrei o braço.", "Parti o braço.")]
    [InlineData("Eles brigaram.", "Eles discutiram.")]
    [InlineData("Estamos brigados.", "Estamos brigados.")]
    [InlineData("Para que esse surto?", "Para que esse surto?")]
    [InlineData("Maior fracasso!", "Maior fracasso!")]
    [InlineData("Ele é um fracassado.", "Ele é um falhado.")]
    [InlineData("Tomou os remédios?", "Tomou os medicamentos?")]
    [InlineData("Deixa na pia.", "Deixa no lava-loiça.")]
    [InlineData("Vou colocar aqui.", "Vou pôr aqui.")]
    [InlineData("Eu podia ter colocado uma tocaia.", "Eu podia ter posto uma tocaia.")]
    [InlineData("Ele gerencia a loja.", "Ele gere a loja.")]
    [InlineData("Anotei a placa do carro.", "Anotei a matrícula do carro.")]
    [InlineData("Srta. Neagley, a Judith a espera.", "Mna. Neagley, a Judith a espera.")]
    [InlineData("Tina. Ebraheim.", "Tina. Ebraheim.")]
    [InlineData("Gabe!", "Gabe!")]
    [InlineData("Livre, leve e solta.", "Livre, leve e solta.")]
    [InlineData("ATAQUE ZUMBI", "ATAQUE ZUMBI")]
    [InlineData("Golfe é para os jovens, Carl.", "Golfe é para os jovens, Carl.")]
    [InlineData("Sem gracinhas, Sra. Chore.", "Sem gracinhas, Sra. Chore.")]
    [InlineData("Sr. Cole, desculpe entrar dessa forma.", "Sr. Cole, desculpe entrar dessa forma.")]
    [InlineData("Vista para o mar e quarto conjugado.", "Vista para o mar e quarto conjugado.")]
    [InlineData("Vista-se.", "Veste-te.")]
    [InlineData("Fosse o que fosse, acabou.", "Fosse o que fosse, acabou.")]
    [InlineData("Trouxa! O Walmart não precisa.", "Trouxa! O Walmart não precisa.")]
    [InlineData("Pergunto-me por\nque ela fez isso.", "Pergunto-me\nporque ela fez isso.")]
    [InlineData("Sinto o cheiro\nde algo delicioso.", "Sinto o cheiro\na algo delicioso.")]
    [InlineData("Então pensei, vocês\ndois têm algum plano?", "Então pensei, vocês\nos dois têm algum plano?")]
    [InlineData("Você tem se metido no meu trabalho.", "Tens-te metido no meu trabalho.")]
    [InlineData("- Pistola, calibre 45.", "- Pistola, calibre 45.")]
    [InlineData("Não que pareças o tipo de boa vizinha.", "Não que pareças o tipo de boa vizinha.")]
    [InlineData("Relaxa, estou de boa.", "Relaxa, estou na boa.")]
    [InlineData("Coloque-o no carro.", "Põe-no no carro.")]
    [InlineData("Traga-o aqui.", "Trá-lo aqui.")]
    [InlineData("Chame o tipo e faça-o fazer tudo.", "Chama o tipo e fá-lo fazer tudo.")]
    [InlineData("Eles deram-a ao juiz.", "Eles deram-na ao juiz.")]
    [InlineData("Alce americano.", "Alce americano.")]
    [InlineData("Mas firme como rocha.", "Mas firme como rocha.")]
    [InlineData("- Me ouça.\n- Me ouça você!", "- Ouve-me.\n- Ouve-me tu!")]
    [InlineData("Ouça você!", "Ouve tu!")]
    [InlineData("Me escute você!", "Escuta-me tu!")]
    [InlineData("Olha você!", "Olha para ti!")]
    [InlineData("Me diga você.", "Diz-me tu.")]
    [InlineData("Me ama você?", "Amas-me?")]
    [InlineData("Me ouve você?", "Ouves-me?")]
    [InlineData("Odeio você!", "Odeio-te!")]
    [InlineData("Vejo você amanhã.", "Vejo-te amanhã.")]
    [InlineData("Se não, vamos ouví-lo.", "Se não, vamos ouvi-lo.")]
    [InlineData("Quero possuí-lo.", "Quero possuí-lo.")]
    [InlineData("Tiveram o merecimento de\nde serem os mais treinados,", "Tiveram o mérito\nde serem os mais treinados,")]
    [InlineData("mas mostra que está na\ntrilha certa.", "mas mostra que está no\ntrilho certo.")]
    [InlineData("Vamos fazer uma trilha noturna.", "Vamos fazer um trilho noturno.")]
    [InlineData("Podemos fazer trilha.", "Podemos fazer caminhadas.")]
    [InlineData("Adoro a trilha sonora do filme.", "Adoro a banda sonora do filme.")]
    [InlineData("É um bom show, e ainda não começou.", "É um bom espetáculo, e ainda não começou.")]
    [InlineData("para o reality show da Milli.", "para o reality show da Milli.")]
    [InlineData("Trouxe uma torta para ele.", "Trouxe uma tarte para ele.")]
    [InlineData("A linha está torta.", "A linha está torta.")]
    [InlineData("Foi moleza.", "Foi canja.")]
    [InlineData("Lógico, mas vão encontrá-lo antes.", "Claro, mas vão encontrá-lo antes.")]
    [InlineData("É um raciocínio lógico.", "É um raciocínio lógico.")]
    [InlineData("Sai, cansei das tuas mentiras.", "Sai, fartei-me das tuas mentiras.")]
    [InlineData("És um covarde!", "És um cobarde!")]
    [InlineData("Ela estava pelada no quarto.", "Ela estava nua no quarto.")]
    [InlineData("Vamos jogar uma pelada no domingo.", "Vamos jogar uma futebolada no domingo.")]
    [InlineData("Compartilhamos tudo.", "Partilhamos tudo.")]
    [InlineData("O gato malhado fugiu.", "O gato malhado fugiu.")]
    [InlineData("Ele aceitou uma propina.", "Ele aceitou um suborno.")]
    [InlineData("Levei uma bronca do meu pai.", "Levei um raspanete do meu pai.")]
    [InlineData("És fã de que time deles?", "És fã de que equipa deles?")]
    [InlineData("Ela é hilária.", "Ela é hilariante.")]
    [InlineData("Vim trazer um travesseiro antialérgico.", "Vim trazer uma almofada antialérgica.")]
    [InlineData("Ganhei na loteria!", "Ganhei na lotaria!")]
    [InlineData("Há muitos sem-teto aqui.", "Há muitos sem-abrigo aqui.")]
    [InlineData("Mais um drinque?", "Mais uma bebida?")]
    [InlineData("Vou de de carro.", "Vou de carro.")]
    [InlineData("E, quando a gente para para pensar,", "E, quando a gente para para pensar,")]
    [InlineData("de A a Z", "de A a Z")]
    [InlineData("Não menciona eles.", "Não os menciona.")]
    [InlineData("Comeste ela?", "Comeste-a?")]
    [InlineData("Mandei ele sair.", "Mandei-o sair.")]
    [InlineData("Não vou usar ela contra ti.", "Não vou usá-la contra ti.")]
    [InlineData("Acho ele muito bonito.", "Acho-o muito bonito.")]
    [InlineData("Pode ser ele.", "Pode ser ele.")]
    [InlineData("Está a mandar eles fazerem coisas.", "Está a mandá-los fazer coisas.")]
    [InlineData("Então manda você.", "Então manda tu.")]
    [InlineData("Ele manda você embora.", "Ele manda-te embora.")]
    [InlineData("Ele levou o Mischa até o estoque?", "Ele levou o Mischa até ao stock?")]
    [InlineData("Ele lutou até o fim.", "Ele lutou até ao fim.")]
    [InlineData("Fico aqui até as 19h.", "Fico aqui até às 19h.")]
    [InlineData("Até o pai sabe.", "Até o pai sabe.")]
    [InlineData("Vamos esperar até o Farooq aparecer.", "Vamos esperar até o Farooq aparecer.")]
    [InlineData("O álibi dele confere.", "O álibi dele bate certo.")]
    [InlineData("Isso confere com o depoimento.", "Isso bate certo com o depoimento.")]
    [InlineData("Vou conferir.", "Vou verificar.")]
    [InlineData("É meu primeiro dia.", "É o meu primeiro dia.")]
    [InlineData("Qual é teu nome?", "Qual é o teu nome?")]
    [InlineData("Ele é meu filho.", "Ele é meu filho.")]
    [InlineData("Não é tua culpa.", "Não é tua culpa.")]
    [InlineData("É meu também.", "É meu também.")]
    [InlineData("O duto de ventilação está entupido.", "A conduta de ventilação está entupida.")]
    [InlineData("Estava tudo dando certo.", "Estava tudo a dar certo.")]
    [InlineData("Há algo muito estranho acontecendo.", "Há algo muito estranho a acontecer.")]
    [InlineData("Estamos há quase dois anos trabalhando nisso.", "Estamos há quase dois anos a trabalhar nisso.")]
    [InlineData("Virei a noite puxando.", "Virei a noite a puxar.")]
    [InlineData("Foi apanhada bebendo em serviço?", "Foi apanhada a beber em serviço?")]
    [InlineData("Não parecia uma pessoa tentando sobreviver.", "Não parecia uma pessoa a tentar sobreviver.")]
    [InlineData("Invadindo a casa dele?", "A invadir a casa dele?")]
    [InlineData("O que você quiser.", "O que quiseres.")]
    [InlineData("Tendo em conta isso, sim.", "Tendo em conta isso, sim.")]
    [InlineData("Olha só você fazendo uma entrada.", "Olha só para ti a fazer uma entrada.")]
    [InlineData("Olha você fazendo isso!", "Olha para ti a fazer isso!")]
    [InlineData("Olha você me enganando.", "Olha para ti a enganar-me.")]
    [InlineData("Olha, você fazendo isso é estranho.", "Olha, você fazendo isso é estranho.")]
    [InlineData("Sei que minha mãe traiu ele.", "Sei que a minha mãe o traiu.")]
    [InlineData("Disse que o Jack matou ela.", "Disse que o Jack a matou.")]
    [InlineData("Sei que minha mãe traiu você.", "Sei que a minha mãe te traiu.")]
    [InlineData("Sei que minha mãe me traiu.", "Sei que a minha mãe me traiu.")]
    [InlineData("Quando a polícia achar ele, liga.", "Quando a polícia o achar, liga.")]
    [InlineData("Acho que a Maria vai ver ela.", "Acho que a Maria vai vê-la.")]
    [InlineData("O tipo que conheci me ligou.", "O tipo que conheci ligou-me.")]
    [InlineData("Manda você.", "Manda tu.")]
    [InlineData("- Manda você!", "- Manda tu!")]
    [InlineData("Incomoda você?", "Incomoda-te?")]
    [InlineData("Liga você para ela.", "Liga-lhe.")]
    [InlineData("Liga você para ela?", "Ligas tu para ela?")]
    [InlineData("Ligue para eles!", "Liga-lhes!")]
    [InlineData("Vou ligar para ela.", "Vou ligar-lhe.")]
    [InlineData("Não, ainda não liguei para eles.", "Não, ainda não lhes liguei.")]
    [InlineData("Ligarei para ela amanhã.", "Ligar-lhe-ei amanhã.")]
    [InlineData("Ligue para mim.", "Liga-me.")]
    [InlineData("Diz para ele que eu volto.", "Diz-lhe que eu volto.")]
    [InlineData("Quando eu pedi para ele sair,", "Quando eu lhe pedi para sair,")]
    [InlineData("Não menti para você.", "Não te menti.")]
    [InlineData("Pergunta para ele.", "Pergunta-lhe.")]
    [InlineData("Paguei para ele ficar calado.", "Paguei-lhe para ficar calado.")]
    [InlineData("Está mentindo para mim.", "Está a mentir-me.")]
    [InlineData("Sei que minha mãe ligou para ele.", "Sei que a minha mãe lhe ligou.")]
    [InlineData("Só peça para ele tirar isso.", "Só lhe pede para tirar isso.")]
    [InlineData("Só me ligue.", "Só me liga.")]
    [InlineData("Apenas tente.", "Apenas tenta.")]
    [InlineData("Não, engraçado é você achar isso.", "Não, engraçado és tu achares isso.")]
    [InlineData("Era você vestindo roupas de mulher?", "Eras tu a vestir roupas de mulher?")]
    [InlineData("Disse que o João ligou\npra ele do hotel,", "Disse que o João\nlhe ligou do hotel,")]
    [InlineData("Então disse pra ela\nme encontrar em sua casa.", "Então disse-lhe\npara me encontrar na sua casa.")]
    [InlineData("Nunca contou pra nós\nsobre isso.", "Nunca nos contou\nsobre isso.")]
    [InlineData("Disse pra ele trazer seu carro.", "Disse-lhe para trazer o seu carro.")]
    [InlineData("Eu o vi na TV,\nfalando sobre o caso.", "Eu vi-o na TV,\na falar sobre o caso.")]
    [InlineData("Falando em dinheiro, ele ligou.", "Por falar em dinheiro, ele ligou.")]
    [InlineData("Ela não quer que eu\nsaia de casa.", "Ela não quer que eu\nsaia de casa.")]
    [InlineData("Isso não conta para ele.", "Isso não conta para ele.")]
    [InlineData("Tenho uma pergunta para ela.", "Tenho uma pergunta para ela.")]
    [InlineData("Se tiver perguntas para nós,", "Se tiver perguntas para nós,")]
    [InlineData("Liguei para ele saber.", "Liguei para ele saber.")]
    [InlineData("Olha para ele.", "Olha para ele.")]
    [InlineData("Liguei para a Maria.", "Liguei para a Maria.")]
    [InlineData("Não dá para ele vir.", "Não dá para ele vir.")]
    public void CorpusAuditRules(string br, string pt) => Assert.Equal(pt, Converter.Convert(br));

    [Theory]
    [InlineData("Eu nunca sair da linha, Sofia.", "I never stepped out of bounds, Sofia.", "Eu nunca saí da linha, Sofia.")]
    [InlineData("Eu nunca sair da linha.", "I don't ever step out of line.", "Eu nunca saio da linha.")]
    [InlineData("Ele falar com ela ontem.", "He talked to her yesterday.", "Ele falou com ela ontem.")]
    [InlineData("Nós chegar tarde.", "We arrived late.", "Nós chegámos tarde.")]
    [InlineData("Eu nunca sair da linha.", "I could never step out of line.", "Eu nunca sair da linha.")]
    [InlineData("Eu sair? Nunca!", "Me leave? Never!", "Eu sair? Nunca!")]
    [InlineData("É para eu sair.", "I left.", "É para eu sair.")]
    [InlineData("Ele quer que eu pegue vocês", "He wanted me to get you guys", "Ele quer que eu pegue vocês")]
    public void BareInfinitiveUsesEnglishTense(string br, string english, string pt) =>
        Assert.Equal(pt, Converter.Convert(br, null, ReferenceHint.FromText(english)));

    [Fact]
    public void BareInfinitiveWithoutReferenceIsOnlyFlagged()
    {
        Assert.Equal("Eu nunca sair da linha.", Converter.Convert("Eu nunca sair da linha."));
        Assert.Contains("saí", GrammarReview.Note("Eu nunca sair da linha.")!);
        Assert.Contains("ter sido", GrammarReview.Note("Devia ter ser reformado.")!);
        Assert.Null(GrammarReview.Note("É para eu sair."));
        Assert.Null(GrammarReview.Note("Eu sair? Nunca!"));
        Assert.Null(GrammarReview.Note("Vai ter lugar amanhã."));
        Assert.Null(GrammarReview.Note("Eu nem sequer sei."));
        Assert.Null(GrammarReview.Note("Ela quer acabar comigo."));
        Assert.Null(GrammarReview.Note("Vou dizer-lhe quando eu estiver em posição."));
        Assert.Null(GrammarReview.Note("Vou dizer-lhe quando eu sair."));
        Assert.Null(GrammarReview.Note("ele sair no momento em que nos visitaram"));
    }

    [Theory]
    [InlineData("Não vou fala mais com vocês.", "Não vou falar mais com vocês.")]
    [InlineData("Ele vai trabalha amanhã.", "Ele vai trabalhar amanhã.")]
    [InlineData("Vamos come depois.", "Vamos comer depois.")]
    [InlineData("Eles vão parti amanhã.", "Eles vão partir amanhã.")]
    [InlineData("Ela ia fala comigo.", "Ela ia falar comigo.")]
    [InlineData("Não vou mais fala com eles.", "Não vou mais falar com eles.")]
    [InlineData("Não vais fala comigo.", "Não vais falar comigo.")]
    [InlineData("Você vai fala comigo.", "Vais falar comigo.")]
    [InlineData("Vou\nfala com eles.", "Vou\nfalar com eles.")]
    [InlineData("<i>Não vou fala.</i>", "<i>Não vou falar.</i>")]
    [InlineData("NÃO VOU FALA.", "NÃO VOU FALAR.")]
    [InlineData("Vou questiona a decisão.", "Vou questionar a decisão.")]
    [InlineData("Vou me lembra disso.", "Vou lembrar-me disso.")]
    public void MissingInfinitiveEndingAfterIr(string br, string pt)
    {
        Assert.Equal(pt, Converter.Convert(br));
        Assert.Equal(pt, Converter.Convert(br, null, ReferenceHint.FromText("I'm not going to talk to them anymore.")));
        Assert.Equal(pt, Converter.Convert(pt));
        Assert.NotNull(GrammarReview.Note(br));
        Assert.Null(GrammarReview.Note(pt));
    }

    [Theory]
    [InlineData("Vou falar com eles.")]
    [InlineData("Ele fala comigo.")]
    [InlineData("Não fala comigo.")]
    [InlineData("Vai para casa.")]
    [InlineData("Vou embora.")]
    [InlineData("Não vai nada.")]
    [InlineData("Vai fora de horas.")]
    [InlineData("Vai que dá certo.")]
    [InlineData("Vai Sara.")]
    [InlineData("Vou a pé.")]
    [InlineData("Vai, fala com eles.")]
    [InlineData("Ela quer ajuda.")]
    [InlineData("Foi luta até ao fim.")]
    [InlineData("Vou xyzabcde.")]
    [InlineData("Vou fala-lo.")]
    [InlineData("Vai à luta.")]
    public void MissingInfinitiveEndingDoesNotGuessOtherConstructions(string text)
    {
        Assert.Equal(text, PortugueseConverter.MissingInfinitiveEnding.Apply(text));
        Assert.Empty(PortugueseConverter.MissingInfinitiveEnding.Find(text));
        Assert.Null(GrammarReview.Note(text));
    }

    [Fact]
    public void MissingInfinitiveEndingSharesRepairAndReview()
    {
        Assert.Contains("vou falar", GrammarReview.Note("Não vou fala mais com vocês.")!);
        Assert.Contains("vou falar", GrammarReview.Note("<i>Não vou fala</i> mais com vocês.")!);
        Assert.Null(GrammarReview.Note("Não vou falar mais com vocês."));
        var local = new PortugueseLocalWordList();
        local.Ignored.Add("fala");
        Assert.Equal("Não vou fala mais com vocês.", new PortugueseConverter(localWordList: local).Convert("Não vou fala mais com vocês."));
    }

    [Theory]
    [InlineData("I never stepped out of bounds.", true, false)]
    [InlineData("He went home.", true, false)]
    [InlineData("I never do that.", false, true)]
    [InlineData("I will never step out.", false, false)]
    [InlineData("I'm going to leave.", false, false)]
    [InlineData("I talked to her.", true, false)]
    public void ReferenceHintTense(string english, bool past, bool present)
    {
        var hint = ReferenceHint.FromText(english)!;
        Assert.Equal(past, hint.Past);
        Assert.Equal(present, hint.Present);
    }

    [Theory]
    [InlineData("Ele chegou ontem à noite com o carro.", "He came in last night with the car.", true)]
    [InlineData("Tu.", "You.", false)]
    [InlineData("O que foi?", "What? In 15 years of marriage, I never once had to come down here.", false)]
    [InlineData("Uma frase qualquer.", null, false)]
    public void TranslationReviewSkipsShortAndMisalignedLines(string pt, string? english, bool comparable) =>
        Assert.Equal(comparable, LocalChecks.IsComparable(pt, english));
}
