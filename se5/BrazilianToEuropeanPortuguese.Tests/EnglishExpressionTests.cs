using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class EnglishExpressionTests
{
    public static TheoryData<string, string, string> ReviewedExpressions => new()
    {
        { "He's fishing for information.", "trying to elicit information indirectly", "Ele está à pesca de informações." },
        { "Trying to get me to take chump bait.", "fall for a provocation", "A tentar que eu morda o isco." },
        { "A cup of joe to go.", "cup of coffee", "Uma chávena de café para levar." },
        { "Something's not adding up.", "does not make sense", "Algo não bate certo." },
        { "I busted my hump.", "work extremely hard", "Dei cabo das costas." },
        { "Stop busting my chops.", "harass", "Para de me chatear." },
        { "It's a piece of cake.", "very easy task", "É canja." },
        { "Break a leg!", "good luck", "Boa sorte!" },
        { "Cut it out!", "stop doing that", "Chega!" },
        { "Beats me.", "I do not know", "Não faço ideia." },
        { "Hang in there.", "keep going despite difficulty", "Aguenta firme." },
        { "I'm all ears.", "I am ready to listen", "Sou todo ouvidos." },
        { "It's on the house.", "free of charge", "É oferta da casa." },
        { "It costs an arm and a leg.", "be very expensive", "Custa uma fortuna." },
        { "I'm under the weather.", "slightly ill", "Estou adoentado." },
        { "Don't spill the beans.", "reveal a secret", "Não contes tudo." },
        { "He let the cat out of the bag.", "reveal a secret", "Ele revelou o segredo." },
        { "Are you pulling my leg?", "tease", "Estás a gozar?" },
        { "He kicked the bucket.", "die", "Ele bateu a bota." },
        { "Let's hit the sack.", "go to bed", "Vamos para a cama." },
        { "Let's call it a day.", "stop for today", "Ficamos por hoje." },
        { "It's not my cup of tea.", "not something I like", "Não é a minha praia." },
        { "Sleep on it.", "think about it before deciding", "Pensa melhor." },
        { "She got cold feet.", "lose courage at the last moment", "Ela ficou com medo." },
        { "In a nutshell, we won.", "in summary", "Em suma, ganhámos." },
        { "The ball is in your court.", "it is your turn to decide", "Agora é contigo." },
        { "My bad.", "it was my mistake", "Foi erro meu." },
        { "Hold your horses.", "wait and calm down", "Tem calma." },
        { "Get a grip.", "control your emotions", "Controla-te." },
        { "Don't jump the gun.", "act too early", "Não te precipites." },
        { "It came out of the blue.", "unexpectedly", "Aconteceu de repente." },
        { "It's a rule of thumb.", "general practical guideline", "É uma regra geral." },
        { "It was the last straw.", "final problem that exhausts patience", "Foi a gota de água." },
        { "He blew off some steam.", "release tension", "Ele desabafou." },
        { "You're on thin ice.", "in a risky situation", "Estás em perigo." },
        { "Bite the bullet.", "face an unpleasant situation bravely", "Enfrenta a situação." },
        { "They're a dime a dozen.", "very common", "Há aos pontapés." },
        { "Let's hit the road.", "leave on a journey", "Vamos embora." },
        { "He bent over backwards.", "make an exceptional effort", "Ele fez de tudo." },
        { "Do it by the book.", "according to the rules", "Faz isso segundo as regras." },
        { "It's off the record.", "confidentially", "É confidencial." },
        { "It's a long shot.", "an unlikely possibility", "É uma hipótese remota." },
        { "You bet.", "certainly", "Podes crer." },
        { "It's no big deal.", "not important", "Não tem importância." },
        { "Keep your chin up.", "stay optimistic", "Anima-te." },
        { "Let's break the ice.", "ease initial social tension", "Vamos quebrar o gelo." },
        { "I'm absolutely knackered.", "I am exhausted", "Estou de rastos." },
        { "I'm totally skint.", "I have no money", "Estou sem dinheiro." },
        { "I'm flat broke.", "I have no money", "Estou sem dinheiro." },
        { "I'm beat.", "I am exhausted", "Estou de rastos." },
        { "I'm chuffed to bits.", "very pleased", "Estou muito contente." },
        { "I was gobsmacked.", "astonished", "Fiquei de boca aberta." },
        { "Are you taking the mickey?", "make fun", "Estás a gozar?" },
        { "Are you taking the piss?", "make fun", "Estás a gozar?" },
        { "It's doing my head in.", "cause frustration", "Está a dar cabo da cabeça." },
        { "Let's touch base tomorrow.", "communicate briefly", "Vamos falar amanhã." },
        { "Let's get the ball rolling.", "start the activity", "Vamos começar." },
        { "We're on the same page.", "in agreement", "Estamos de acordo." },
        { "She learned the ropes.", "learn how things work", "Ela aprendeu o ofício." },
        { "Pull your weight.", "contribute fairly", "Faz a tua parte." },
        { "We're back to square one.", "starting again", "Voltámos à estaca zero." },
        { "It's back to the drawing board.", "starting again", "É voltar à estaca zero." },
        { "Keep me in the loop.", "informed about developments", "Mantém-me a par." },
        { "It's on the back burner.", "postponed for now", "Fica em segundo plano." },
        { "Give me a ballpark figure.", "rough estimate", "Dá-me uma estimativa." },
        { "We paid through the nose.", "pay an excessive price", "Pagámos os olhos da cara." },
        { "I need to spend a penny.", "use the toilet", "Preciso de ir à casa de banho." },
        { "And Bob's your uncle.", "it is done", "E pronto." },
        { "Can I take a rain check?", "postpone accepting the invitation", "Pode ficar para outra vez?" },
        { "She works to bring home the bacon.", "earn money to support the family", "Ela trabalha para sustentar a família." },
        { "The kids are full of beans.", "very energetic", "Os miúdos estão cheios de energia." },
        { "It was a blessing in disguise.", "an apparent misfortune that turns out well", "Há males que vêm por bem." },
        { "The plan went pear-shaped.", "go wrong", "O plano correu mal." },
        { "I'm in a pickle.", "in a difficult situation", "Estou numa alhada." },
        { "We struggle to make ends meet.", "afford basic living expenses", "É difícil pagar as despesas." },
        { "We arrived in the nick of time.", "just before it was too late", "Chegámos mesmo a tempo." },
        { "It happens once in a blue moon.", "very rarely", "Acontece raramente." },
        { "It's a storm in a teacup.", "an exaggerated reaction to a small problem", "É uma tempestade num copo de água." },
        { "You're barking up the wrong tree.", "look in the wrong place for an answer", "Estás enganado." },
        { "That's a load of old cobblers.", "nonsense", "São disparates." },
        { "That's a load of old pony.", "nonsense", "São tretas." },
        { "That's a load of rubbish.", "nonsense", "São disparates." },
        { "We burned the midnight oil.", "work late into the night", "Trabalhámos pela noite dentro." },
        { "I'll kill two birds with one stone.", "achieve two goals with one action", "Mato dois coelhos de uma cajadada." },
        { "He threw a spanner in the works.", "disrupt the plans", "Ele estragou os planos." },
        { "Get it off your chest.", "talk about a worry", "Desabafa." },
        { "He's all mouth and no trousers.", "all talk and no action", "É muito paleio e pouca ação." },
        { "It's a dog's breakfast.", "a badly organized mess", "É uma trapalhada." },
        { "They're like two peas in a pod.", "very similar to each other", "São como duas gotas de água." },
        { "We avoid the elephant in the room.", "an obvious problem nobody discusses", "Evitamos falar do problema." },
        { "Have a butcher's at this.", "take a look", "Dá uma vista de olhos." },
        { "He threw a wobbly.", "have an angry outburst", "Ele fez uma cena." },
        { "We had a whale of a time.", "have a wonderful time", "Divertimo-nos imenso." },
        { "She threw in the towel.", "give up", "Ela desistiu." },
        { "It's a tough nut to crack.", "a difficult problem to solve", "É um osso duro de roer." },
        { "It's a wild goose chase.", "a futile search", "É uma busca inútil." },
        { "He's on his tod.", "alone", "Está sozinho." },
    };

    [Theory]
    [MemberData(nameof(ReviewedExpressions))]
    public void ReviewedExpressionsHaveMeaningsAndHints(string english, string meaning, string portuguese)
    {
        Assert.Contains(meaning, IdiomHints.MeaningReference(english));
        Assert.NotNull(IdiomHints.Note("Tradução por verificar.", english));
        Assert.Null(IdiomHints.Note(portuguese, english));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("The reference is ordinary dialogue.")]
    [InlineData("I'm down.")]
    [InlineData("Cheers!")]
    [InlineData("You have a butcher's knife.")]
    [InlineData("I am a gobsmackedness specialist.")]
    [InlineData("I'm beaten.")]
    [InlineData("I'm beat up.")]
    [InlineData("You bet on the horse.")]
    [InlineData("You're fishing.")]
    [InlineData("We're fishing for trout.")]
    [InlineData("The fish took the bait.")]
    [InlineData("Joe's cup is empty.")]
    [InlineData("Add up these numbers.")]
    public void AmbiguousOrUnrecognisedEnglishHasNoInventedMeaning(string? english) =>
        Assert.Null(IdiomHints.MeaningReference(english));

    [Fact]
    public void MeaningsRetainSentenceContextAndHandleTagsLineBreaksAndApostrophes()
    {
        Assert.Equal("We arrived just before it was too late to help her.",
            IdiomHints.MeaningReference("<i>We arrived in the nick\nof time</i> to help her."));
        Assert.Equal("And it is done.", IdiomHints.MeaningReference("And Bob’s your uncle."));
        Assert.Equal("They are very energetic but he is alone.",
            IdiomHints.MeaningReference("They are full of beans but he is on his tod."));
    }

    [Theory]
    [InlineData("Estou quebrado.", "I'm broke.", "Estou sem dinheiro.")]
    [InlineData("Estou quebrada.", "I am skint.", "Estou sem dinheiro.")]
    [InlineData("Eu estou quebrado.", "I'm flat broke.", "Eu estou sem dinheiro.")]
    [InlineData("Estou quebrado.", "I'm beat.", "Estou de rastos.")]
    [InlineData("Estou quebrada.", "I’m absolutely knackered.", "Estou de rastos.")]
    [InlineData("ESTOU QUEBRADO!", "I'M TOTALLY SKINT!", "ESTOU SEM DINHEIRO!")]
    public void NarrowSlangCorrectionsNeedBothLanguages(string source, string english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, null, ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Estou quebrado.", "I'm broken.")]
    [InlineData("Estou quebrado.", "I'm broke. He is exhausted.")]
    [InlineData("Estou quebrado.", "He is skint.")]
    [InlineData("Estou quebrado.", "I'm not skint.")]
    [InlineData("O vidro está quebrado.", "I'm skint.")]
    [InlineData("Estou quebrado. Ela também.", "I'm skint.")]
    public void AmbiguousSlangDoesNotTriggerNewCorrections(string source, string english)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(converter.Convert(source), converter.Convert(source, null, ReferenceHint.FromText(english)));
    }

    [Theory]
    [InlineData("vocabulary")]
    [InlineData("colloquial")]
    [InlineData("reference-idioms")]
    public void CorrectionsRespectExistingSwitches(string disabled)
    {
        var options = disabled switch
        {
            "vocabulary" => new ConversionOptions(Vocabulary: false),
            "colloquial" => new ConversionOptions(Colloquial: false),
            _ => new ConversionOptions(DisabledRules: disabled),
        };
        var converter = new PortugueseConverter(options);
        Assert.Equal(converter.Convert("Estou quebrado."),
            converter.Convert("Estou quebrado.", null, ReferenceHint.FromText("I'm skint.")));
    }

    [Theory]
    [InlineData("Está jogando verde.", "You're fishing.", "Estás à pesca.")]
    [InlineData("Ele está jogando verde.", "He's fishing.", "Ele está à pesca.")]
    [InlineData("Ela estava jogando um verde.", "She was fishing.", "Ela estava à pesca.")]
    [InlineData("Nós estamos a jogar verde.", "We're fishing.", "Nós estamos à pesca.")]
    [InlineData("Ele jogou verde.", "He fished for information.", "Ele pescou.")]
    [InlineData("Ele joga um verde para colher maduro.", "He fishes for information.", "Ele pesca.")]
    [InlineData("ESTÁ JOGANDO VERDE!", "YOU'RE FISHING!", "ESTÁS À PESCA!")]
    [InlineData("<i>Está\njogando verde.</i>", "You're fishing.", "<i>Estás\nà pesca.</i>")]
    [InlineData("Espero que ele pegue a isca.", "I hope he takes the bait.", "Espero que ele morda o isco.")]
    [InlineData("Ele pegou na isca.", "He took the bait.", "Ele mordeu o isco.")]
    [InlineData("Não peguei a isca.", "I didn't take chump bait.", "Não mordi o isco.")]
    [InlineData("E não somos jogadores.\nTemos um vídeo seu", "And we ain't no fishermen. We got video of you",
        "E não somos pescadores.\nTemos um vídeo teu")]
    [InlineData("Uma xícara de Joe para viagem.", "A cup of joe to go.", "Uma chávena de café para levar.")]
    [InlineData("Uma chávena\r\nde Joe.", "A cup of joe.", "Uma chávena\r\nde café.")]
    [InlineData("Há algo que não soma.", "Something's not adding up.", "Há algo que não bate certo.")]
    [InlineData("A história não somava.", "The story didn't add up.", "A história não batia certo.")]
    public void ReferenceMetaphorsKeepTheirMeaning(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Ele está jogando verde.", null)]
    [InlineData("Ele está jogando verde.", "He is playing green.")]
    [InlineData("Ele está jogando verde.", "He is fishing for trout in the river.")]
    [InlineData("Ele joga verde claro.", "He's fishing.")]
    [InlineData("Ele pegou a isca.", "He picked up the bait.")]
    [InlineData("Ele pegou a isca.", "He took the bait off the hook.")]
    [InlineData("Não somos jogadores.", null)]
    [InlineData("Não somos jogadores.", "We aren't players.")]
    [InlineData("Somos jogadores de futebol.", "We are fishermen.")]
    [InlineData("Somos jogadores. Eles são jogadores.", "We are fishermen.")]
    [InlineData("Somos jogadores.", "We're players, not fishermen.")]
    [InlineData("A xícara de Joe está vazia.", "Joe's cup is empty.")]
    [InlineData("Não soma dois e três.", "Add up two and three.")]
    public void MetaphorsNeedBothLanguagesAndProtectLiteralComplements(string source, string? english)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(converter.Convert(source), converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Fact]
    public void FishingGlossRequiresPortugueseEvidenceAndKeepsContext()
    {
        Assert.Equal("You're trying to elicit information indirectly.",
            IdiomHints.MeaningReference("You're fishing.", "Está jogando verde."));
        Assert.Null(IdiomHints.MeaningReference("You're fishing.", "Está a pescar."));
        Assert.Null(IdiomHints.MeaningReference("You're fishing in the river.", "Está jogando verde."));
        Assert.Equal("Something does not make sense.", IdiomHints.MeaningReference("Something's not adding up."));
        Assert.NotNull(IdiomHints.Note("Está a jogar verde.", "You're fishing."));
        Assert.Null(IdiomHints.Note("Estás à pesca.", "You're fishing."));
    }

    [Fact]
    public void ReferenceMetaphorsRespectOptionsAndLocalVocabulary()
    {
        const string source = "Ele está jogando verde.";
        var reference = ReferenceHint.FromText("He's fishing.");
        foreach (var options in new[] { new ConversionOptions(Vocabulary: false),
            new ConversionOptions(Colloquial: false), new ConversionOptions(DisabledRules: "reference-idioms") })
        {
            var converter = new PortugueseConverter(options);
            Assert.Equal(converter.Convert(source), converter.Convert(source, reference: reference));
        }
        foreach (var word in new[] { "jogar", "verde" })
        {
            var local = new PortugueseLocalWordList();
            local.Ignored.Add(word);
            var converter = new PortugueseConverter(localWordList: local);
            Assert.Equal(converter.Convert(source), converter.Convert(source, reference: reference));
        }
        var custom = new PortugueseLocalWordList();
        custom.Words.Add(("jogar", "brincar"));
        var overridden = new PortugueseConverter(localWordList: custom);
        Assert.Equal(overridden.Convert(source), overridden.Convert(source, reference: reference));
        Assert.Equal("Está à pesca.", new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert("Está jogando verde.", reference: ReferenceHint.FromText("You're fishing.")));
        Assert.Equal("O senhor está à pesca.", new PortugueseConverter()
            .Convert("O senhor está jogando verde.", reference: ReferenceHint.FromText("You're fishing.")));
    }

    [Fact]
    public void FishingSequenceKeepsCueBoundariesTimesAndOriginals()
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 1134014, EndMs = 1135676, Text = "Está jogando verde." },
            new SrtBlock { StartMs = 1135678, EndMs = 1137105, Text = "Tentando que eu pegue\na isca," },
            new SrtBlock { StartMs = 1137106, EndMs = 1138268, Text = "mas não sou tolo." },
            new SrtBlock { StartMs = 1138270, EndMs = 1139815, Text = "E não somos jogadores.\nTemos um vídeo seu" },
        };
        var originals = blocks.Select(b => (b.Text, b.StartMs, b.EndMs)).ToArray();
        var english = new[]
        {
            new SrtBlock { StartMs = 1133633, EndMs = 1135335, Text = "You're fishing." },
            new SrtBlock { StartMs = 1135368, EndMs = 1136736, Text = "Trying to get me\nto take chump bait," },
            new SrtBlock { StartMs = 1136769, EndMs = 1138238, Text = "but I ain't no dummy." },
            new SrtBlock { StartMs = 1138271, EndMs = 1139772, Text = "And we ain't no fishermen.\nWe got video of you" },
        };
        var aligned = ReferenceAligner.AlignByTiming(blocks, english);
        var converted = SubtitleConversion.Convert(blocks, ConversionOptions.Default, english: aligned.Texts).ToArray();
        Assert.Equal(4, converted.Length);
        Assert.Equal("Estás à pesca.", converted[0].Text);
        Assert.Contains("morda\no isco,", converted[1].Text);
        Assert.Equal("E não somos pescadores.\nTemos um vídeo teu", converted[3].Text);
        Assert.Equal(originals, blocks.Select(b => (b.Text, b.StartMs, b.EndMs)).ToArray());
        Assert.Contains(converted[0].Rules, rule => rule.Id == "reference-idioms");
        Assert.Contains(converted[3].Rules, rule => rule.Id == "reference-idioms");
    }
}
