using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class BigCompareObjectTests
{
    private static void AssertConverts(string input, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(input));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Adorei ele!", "Adorei-o!")]
    [InlineData("Eu tirei ela de lá.", "Eu tirei-a de lá.")]
    [InlineData("Ela queria você.", "Ela queria-te.")]
    [InlineData("e isso inclui você.", "e isso inclui-te.")]
    [InlineData("Se alguém investigasse você,", "Se alguém te investigasse,")]
    [InlineData("Tudo porque deixou que usassem você.", "Tudo porque deixou que te usassem.")]
    [InlineData("Não estou evitando você.", "Não estou a evitar-te.")]
    [InlineData("Está estudando ela?", "Estás a estudá-la?")]
    [InlineData("Está procurando você.", "Está a procurar-te.")]
    [InlineData("Citando você, Graça:", "Citando-te, Graça:")]
    [InlineData("Eu não mereço você.", "Eu não te mereço.")]
    [InlineData("Ele ferrou você.", "Ele lixou-te.")]
    [InlineData("Constrangi você?", "Constrangi-te?")]
    [InlineData("Porque agora eu tenho você.", "Porque agora eu tenho-te.")]
    [InlineData("Joguem ele no carro!", "Joguem-no no carro!")]
    [InlineData("Tragam ele, façam ele falar.", "Tragam-no, façam-no falar.")]
    [InlineData("Ele confundiu você com outra.", "Ele confundiu-te com outra.")]
    [InlineData("Ela stressava ele muito.", "Ela stressava-o muito.")]
    public void ObjectPronounsAfterPastGerundAndSubjunctiveForms(string input, string expected)
        => AssertConverts(input, expected);

    [Theory]
    [InlineData("Mas por que alguém mataria ele?", "Mas porque alguém o mataria?")]
    [InlineData("Eu aceitaria ela.", "Eu aceitá-la-ia.")]
    [InlineData("Não aceitariam ele?", "Não o aceitariam?")]
    [InlineData("O Bob e a Carol não salvariam ela.", "O Bob e a Carol não a salvariam.")]
    public void ConditionalObjectsUseProclisisOrMesoclisis(string input, string expected)
        => AssertConverts(input, expected);

    [Theory]
    [InlineData("Espero que tenha visto ele.", "Espero que o tenha visto.")]
    [InlineData("Queria ter encontrado você antes.", "Queria ter-te encontrado antes.")]
    [InlineData("Lamento ter trazido você aqui.", "Lamento ter-te trazido aqui.")]
    [InlineData("Tinha visto ele ontem.", "Tinha-o visto ontem.")]
    [InlineData("Não tinha visto ela.", "Não a tinha visto.")]
    [InlineData("Eu teria matado ele.", "Eu tê-lo-ia matado.")]
    [InlineData("Ele tem ajudado você muito.", "Ele tem-te ajudado muito.")]
    [InlineData("Por ter salvado ela, ganhou.", "Por tê-la salvado, ganhou.")]
    [InlineData("Ninguém tinha avisado você.", "Ninguém te tinha avisado.")]
    public void CompoundTenseObjectsAttachToTheAuxiliary(string input, string expected)
        => AssertConverts(input, expected);

    [Theory]
    [InlineData("Escolhe você.", "Escolhe tu.")]
    [InlineData("Tente você.", "Tenta tu.")]
    [InlineData("Atire você.", "Atira tu.")]
    [InlineData("E acho que conheço ele também.", "E acho que conheço ele também.")]
    [InlineData("Tem ele aqui?", "Tem ele aqui?")]
    [InlineData("O que tinha feito ele?", "O que tinha feito ele?")]
    [InlineData("Eu tinha ido lá.", "Eu tinha ido lá.")]
    [InlineData("Eu esperei você me pedir.", "Eu esperei pedires-me.")]
    [InlineData("Mas adorei você estar aqui.", "Mas adorei estares aqui.")]
    [InlineData("Convenci você a ir.", "Convenci-te a ir.")]
    [InlineData("Eu fiz você chorar.", "Eu fiz-te chorar.")]
    [InlineData("Não ouço você fazer comentários.", "Não te ouço fazer comentários.")]
    [InlineData("só eu mandando você se foder.", "só eu a mandar-te se foder.")]
    [InlineData("só eu mandando ele embora.", "só eu a mandá-lo embora.")]
    [InlineData("- Ajudando você!", "- Ajudando-te!")]
    [InlineData("Foda-se.", "Foda-se.")]
    [InlineData("eu nem sequer tenho visto ela.", "eu nem sequer a tenho visto.")]
    [InlineData("deve ter feito eles pirarem.", "deve ter feito eles pirarem.")]
    public void SubjectsAndIntransitiveCompoundsAreKept(string input, string expected)
        => AssertConverts(input, expected);

    [Theory]
    [InlineData("Ela é intensa e esquentada.", "Ela é intensa e esquentada.")]
    [InlineData("Esquentei a sopa.", "Aqueci a sopa.")]
    [InlineData("Nem esquente a cabeça com isso.", "Não te preocupes com isso.")]
    [InlineData("Chispa daqui!", "Desaparece daqui!")]
    [InlineData("Chispem daqui!", "Desapareçam daqui!")]
    [InlineData("Cantava uma canção de ninar.", "Cantava uma canção de embalar.")]
    [InlineData("Vai ser na marra.", "Vai ser à força.")]
    [InlineData("Não tem água encanada.", "Não tem água canalizada.")]
    public void BigCompareVocabulary(string input, string expected)
        => AssertConverts(input, expected);

    [Theory]
    [InlineData("Você deixou que usassem você.", "Deixaste-te usar.")]
    [InlineData("Não deixe que usem você.", "Não te deixes usar.")]
    [InlineData("Deixa que ajudem você.", "Deixa-te ajudar.")]
    [InlineData("Você deixou que te vissem.", "Deixaste-te ver.")]
    [InlineData("Você deixou eles usarem você.", "Deixaste-os usar-te.")]
    [InlineData("Você fez eles pararem.", "Fizeste-os parar.")]
    [InlineData("Ele deixou que usassem você.", "Ele deixou que te usassem.")]
    [InlineData("Não deixe que façam isso.", "Não deixes que façam isso.")]
    public void LetYourselfUsesReflexiveDeixarWithInfinitive(string input, string expected)
        => AssertConverts(input, expected);

    [Fact]
    public void DroppedSubjectLetYourselfNeedsEnglishEvidence()
    {
        const string source = "Tudo porque deixou que usassem você.";
        Assert.Equal("Tudo porque te deixaste usar.", new PortugueseConverter()
            .Convert(source, reference: ReferenceHint.FromText("All because you let yourself get used.")));
        Assert.Equal("Tudo porque deixou que te usassem.", new PortugueseConverter()
            .Convert(source, reference: ReferenceHint.FromText("All because he let them use you.")));
    }

    [Theory]
    [InlineData("Para eles usarem você.", "Para eles te usarem.")]
    [InlineData("Para eles usarem ele.", "Para eles o usarem.")]
    [InlineData("Sem eles verem você.", "Sem eles te verem.")]
    [InlineData("Antes de eles encontrarem você.", "Antes de eles te encontrarem.")]
    [InlineData("Até eles acharem você.", "Até eles te acharem.")]
    [InlineData("Para o Jack ver você.", "Para o Jack te ver.")]
    [InlineData("Para os polícias encontrarem você.", "Para os polícias te encontrarem.")]
    [InlineData("Para a polícia prender ele.", "Para a polícia o prender.")]
    [InlineData("para eles usarem-te.", "para eles te usarem.")]
    [InlineData("É melhor eles ajudarem você.", "É melhor eles ajudarem-te.")]
    [InlineData("Para eles irem embora.", "Para eles irem embora.")]
    public void PrepositionWithInfinitiveSubjectAttractsTheClitic(string input, string expected)
        => AssertConverts(input, expected);

    [Theory]
    [InlineData("Para você usar ele.", "Para o usares.")]
    [InlineData("Sem você ver ele.", "Sem o veres.")]
    [InlineData("Até você encontrar ela.", "Até a encontrares.")]
    [InlineData("Para a gente ajudar você.", "Para te ajudarmos.")]
    [InlineData("Para a gente encontrar eles.", "Para os encontrarmos.")]
    [InlineData("Para você usá-lo.", "Para o usares.")]
    [InlineData("Para você fazê-lo.", "Para o fazeres.")]
    [InlineData("Antes de você pô-lo lá.", "Antes de o pores lá.")]
    [InlineData("Sem você ajudar-me.", "Sem me ajudares.")]
    [InlineData("Pagaram para matarem ele.", "Pagaram para o matarem.")]
    [InlineData("Para ganharem ele treinou muito.", "Para ganharem ele treinou muito.")]
    [InlineData("Para tu o usares.", "Para tu o usares.")]
    public void PersonalInfinitiveEndingAttractsTheCliticWithoutSubject(string input, string expected)
        => AssertConverts(input, expected);

    [Theory]
    [InlineData("Nunca vi você também.", "Nunca te vi também.")]
    [InlineData("Amo você também.", "Amo-te também.")]
    [InlineData("Você também.", "Tu também.")]
    [InlineData("E você também vai.", "E tu também vais.")]
    [InlineData("Isso vale para você também.", "Isso vale para ti também.")]
    [InlineData("Era você também?", "Eras tu também?")]
    public void YouAlsoAfterTransitiveVerbIsTheObject(string input, string expected)
        => AssertConverts(input, expected);

    [Theory]
    [InlineData("Ele só quer faturar um pouco de Molly.", "Ele só quer comprar um pouco de Molly.")]
    [InlineData("Ele faturou cocaína no bar.", "Ele comprou cocaína no bar.")]
    [InlineData("A empresa faturou milhões.", "A empresa faturou milhões.")]
    [InlineData("Vamos faturar o cliente.", "Vamos faturar o cliente.")]
    public void ScoringDrugsIsBuying(string input, string expected)
        => AssertConverts(input, expected);

    [Fact]
    public void ScoringNeedsDrugOrEnglishEvidence()
    {
        Assert.Equal("Fui comprar um pouco.", new PortugueseConverter()
            .Convert("Fui faturar um pouco.", reference: ReferenceHint.FromText("I went to score some.")));
        Assert.Equal("Fui faturar um pouco.", new PortugueseConverter().Convert("Fui faturar um pouco."));
    }

    [Theory]
    [InlineData("Calma.\nQuanto quer faturar?", "Chill out. How much you looking to score?", "Calma.\nQuanto quer comprar?")]
    [InlineData("Ele foi faturar no bairro.", "He went to score in the neighborhood.", "Ele foi comprar no bairro.")]
    [InlineData("É onde eu costumava faturar.", "That's where I used to score.", "É onde eu costumava comprar.")]
    [InlineData("Ele tentou faturar dele.", "He tried to score from him.", "Ele tentou comprar dele.")]
    [InlineData("Faturou ontem à noite?", "Did you score last night?", "Compraste ontem à noite?")]
    [InlineData("Ele faturou!", "He scored a goal!", "Ele marcou!")]
    [InlineData("O time precisa faturar.", "The team needs to score.", "A equipa precisa de marcar.")]
    [InlineData("Ele faturou no segundo tempo.", "He scored in the second half.", "Ele marcou na segunda parte.")]
    [InlineData("Ele quer faturar alto.", "He wants to score big.", "Ele quer faturar alto.")]
    [InlineData("Vamos faturar.", "Let's settle the score.", "Vamos faturar.")]
    [InlineData("Ele vai faturar milhões.", "He's going to score millions.", "Ele vai faturar milhões.")]
    [InlineData("Ele faturou dois gols.", "", "Ele marcou dois golos.")]
    [InlineData("Faturei o primeiro gol.", "", "Marquei o primeiro golo.")]
    [InlineData("Eles faturaram pontos com o chefe.", "", "Eles marcaram pontos com o chefe.")]
    [InlineData("Nós faturamos três cestas.", "", "Nós marcamos três cestas.")]
    [InlineData("A loja faturou milhões.", "The store made millions.", "A loja faturou milhões.")]
    public void IntransitiveDrugScoringIsBuying(string input, string english, string expected)
    {
        Assert.Equal(expected, new PortugueseConverter().Convert(input, reference: ReferenceHint.FromText(english)));
    }

    [Theory]
    [InlineData("Presta atenção nisto.", "Presta atenção a isto.")]
    [InlineData("Presta atenção no que eu digo.", "Presta atenção ao que eu digo.")]
    [InlineData("Ninguém prestou atenção nela.", "Ninguém prestou atenção a ela.")]
    [InlineData("Não dês atenção naquilo.", "Não dês atenção àquilo.")]
    [InlineData("Prestem mais atenção nos detalhes.", "Prestem mais atenção aos detalhes.")]
    [InlineData("Presta atenção em mim.", "Presta atenção a mim.")]
    [InlineData("A atenção no caso é grande.", "A atenção no caso é grande.")]
    [InlineData("Ela prestava atenção\nem sua conversa.", "Ela prestava atenção\nà sua conversa.")]
    [InlineData("Presta atenção em teus filhos.", "Presta atenção aos teus filhos.")]
    [InlineData("Presta atenção na tua mulher\ntanto quanto na tua colega.", "Presta atenção à tua mulher\ntanto quanto à tua colega.")]
    [InlineData("Presta atenção no jogo. Como no outro dia.", "Presta atenção ao jogo. Como no outro dia.")]
    public void PayingAttentionTakesA(string input, string expected)
        => AssertConverts(input, expected);

    [Fact]
    public void AttentionToThoseKeepsTheEnglishDistance()
    {
        Assert.Equal("Não prestes atenção a esses.", new PortugueseConverter()
            .Convert("Não preste atenção nestes.", reference: ReferenceHint.FromText("Oh, don't pay any attention to those.")));
        Assert.Equal("Não prestes atenção a estes.", new PortugueseConverter().Convert("Não preste atenção nestes."));
    }

    [Theory]
    [InlineData("Não preste atenção nestes.", "Oh, don't pay any attention to those.", "Não preste atenção a esses.")]
    [InlineData("Não preste atenção a estes.", "Oh, don't pay any attention to those.", "Não preste atenção a esses.")]
    [InlineData("Não preste atenção nestas.", "This is important. Pay attention to those.", "Não preste atenção a essas.")]
    [InlineData("Não preste atenção nestes.", "Those are old. Pay attention to these.", "Não preste atenção a estes.")]
    [InlineData("Não preste atenção nestes.", null, "Não preste atenção a estes.")]
    public void AttentionDistanceUsesItsAlignedEnglishTarget(string input, string? english, string expected)
    {
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(input, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Acho que talvez ele esqueceu\nde tirar o lixo.", "Acho que talvez ele se tenha esquecido\nde tirar o lixo.")]
    [InlineData("Talvez é verdade.", "Talvez seja verdade.")]
    [InlineData("Talvez ele está certo.", "Talvez ele esteja certo.")]
    [InlineData("Talvez você tem razão.", "Talvez tenhas razão.")]
    [InlineData("Talvez ela estava com medo.", "Talvez ela estivesse com medo.")]
    [InlineData("Talvez ele foi embora.", "Talvez ele tenha ido embora.")]
    [InlineData("Talvez foi um acidente.", "Talvez tenha sido um acidente.")]
    [InlineData("Talvez eles viram você.", "Talvez eles te tenham visto.")]
    [InlineData("Talvez você esqueceu.", "Talvez te tenhas esquecido.")]
    [InlineData("Talvez ele precisa de ajuda.", "Talvez ele precise de ajuda.")]
    [InlineData("Talvez o pai sabe.", "Talvez o pai saiba.")]
    [InlineData("Talvez Jamie ligou para ela.", "Talvez Jamie lhe tenha ligado.")]
    [InlineData("Talvez ele não quer falar.", "Talvez ele não queira falar.")]
    [InlineData("Talvez ele me viu.", "Talvez ele me tenha visto.")]
    [InlineData("Talvez você podia ajudar.", "Talvez pudesses ajudar.")]
    [InlineData("Talvez para amanhã.", "Talvez para amanhã.")]
    [InlineData("Talvez há dois anos.", "Talvez há dois anos.")]
    [InlineData("Talvez há um problema.", "Talvez haja um problema.")]
    [InlineData("Hematomas, talvez marcas de agulha.", "Hematomas, talvez marcas de agulha.")]
    [InlineData("Algo que você tenha esquecido, ignorado.", "Algo que tenhas esquecido, ignorado.")]
    [InlineData("Talvez nada.", "Talvez nada.")]
    [InlineData("Ele esqueceu, talvez.", "Ele esqueceu-se, talvez.")]
    [InlineData("Talvez porque ele sabe.", "Talvez porque ele sabe.")]
    [InlineData("Talvez parte disso seja verdade.", "Talvez parte disso seja verdade.")]
    [InlineData("Espero que ele tenha esquecido o nome.", "Espero que ele se tenha esquecido do nome.")]
    [InlineData("Espero que você não tenha esquecido de mim.", "Espero que não te tenhas esquecido de mim.")]
    [InlineData("É possível que ele tenha-lhe mentido.", "É possível que ele lhe tenha mentido.")]
    public void MaybeBeforeTheVerbTakesTheSubjunctive(string input, string expected)
        => AssertConverts(input, expected);

    [Theory]
    [InlineData("<i>Mãos onde podemos vê-las!</i>", "", "<i>Mãos onde possamos vê-las!</i>")]
    [InlineData("Mãos onde eu posso ver!", "", "Mãos onde eu possa ver!")]
    [InlineData("As mãos no ar, onde podemos vê-las!", "", "As mãos no ar, onde possamos vê-las!")]
    [InlineData("Põe as mãos onde eu posso vê-las!", "", "Põe as mãos onde eu possa vê-las!")]
    [InlineData("- Coloque-as onde posso ver!\n- Não se mexa!", "", "- Põe-nas onde possa ver!\n- Não te mexas!")]
    [InlineData("Coloque-as mãos onde posso ver!", "", "Põe as mãos onde possa ver!")]
    [InlineData("Ponha as mãos para fora\nonde eu posso vê-las!", "And put your hands out the window\nwhere I can see 'em!", "Põe as mãos para fora\nonde eu possa vê-las!")]
    [InlineData("Fique onde a gente pode te ver.", "", "Fica onde possamos ver-te.")]
    [InlineData("Fica onde estás.", "", "Fica onde estás.")]
    [InlineData("Sabes onde posso encontrar o David?", "You know where I can find David?", "Sabes onde posso encontrar o David?")]
    [InlineData("Não me diga onde posso e não posso ir.", "Don't tell me where I can and cannot go.", "Não me digas onde posso e não posso ir.")]
    [InlineData("Há uma cama onde posso dormir.", "", "Há uma cama onde posso dormir.")]
    [InlineData("Põe-no onde ele pode ser visto.", "", "Põe-no onde ele possa ser visto.")]
    [InlineData("Mostra-me onde posso estacionar.", "", "Mostra-me onde posso estacionar.")]
    [InlineData("Leva o carro para onde ele pode ser visto.", "", "Leva o carro para onde ele pode ser visto.")]
    public void WhereClauseGivingTheCommandPurposeTakesTheSubjunctive(string input, string english, string expected)
    {
        var reference = english.Length == 0 ? null : ReferenceHint.FromText(english);
        var actual = new PortugueseConverter().Convert(input, reference: reference);
        Assert.Equal(expected, actual);
        Assert.Equal(expected, new PortugueseConverter().Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("No segundo tempo, ele marcou dois gols.", "", "Na segunda parte, ele marcou dois golos.")]
    [InlineData("Ele faturou no segundo tempo.", "", "Ele marcou na segunda parte.")]
    [InlineData("Trocou de caddie aos 45 do segundo tempo.", "", "Trocou de caddie aos 45 da segunda parte.")]
    [InlineData("Segundo tempo! Vamos!", "", "Segunda parte! Vamos!")]
    [InlineData("Todo o primeiro tempo foi horrível.", "", "Toda a primeira parte foi horrível.")]
    [InlineData("O 2º tempo começa já.", "", "A 2ª parte começa já.")]
    [InlineData("Pelo segundo tempo seguido, perderam.", "", "Pela segunda parte seguida, perderam.")]
    [InlineData("Num segundo tempo, vamos falar disso.", "", "Num segundo tempo, vamos falar disso.")]
    [InlineData("O segundo tempo verbal.", "", "O segundo tempo verbal.")]
    [InlineData("Segundo, tempo é dinheiro.", "", "Segundo, tempo é dinheiro.")]
    [InlineData("Qual o tempo?", "cut it out.\nWhat quarter?", "Qual o quarto?")]
    [InlineData("Estamos no terceiro período.", "We're in the third quarter.", "Estamos no terceiro quarto.")]
    [InlineData("Marcou no 3º período.", "He scored in the 3rd quarter.", "Marcou no 3º quarto.")]
    [InlineData("Perdemos os dois últimos tempos.", "We lost the last two quarters.", "Perdemos os dois últimos quartos.")]
    [InlineData("No último período, viraram o jogo.", "In the final quarter, they turned it around.", "No último quarto, viraram o jogo.")]
    [InlineData("Estamos no terceiro período.", "", "Estamos no terceiro período.")]
    [InlineData("No primeiro período tive boas notas.", "In the first term I got good grades.", "No primeiro período tive boas notas.")]
    [InlineData("O jogo foi para a prorrogação.", "The game went into overtime.", "O jogo foi para o prolongamento.")]
    [InlineData("Ganharam na prorrogação.", "They won it in overtime.", "Ganharam no prolongamento.")]
    [InlineData("Pedimos uma prorrogação do prazo.", "We asked for an extension.", "Pedimos uma prorrogação do prazo.")]
    [InlineData("Ele faz prorrogação.", "He works overtime.", "Ele faz prorrogação.")]
    public void MatchPeriodsUsePortugueseNames(string input, string english, string expected)
    {
        var reference = english.Length == 0 ? null : ReferenceHint.FromText(english);
        Assert.Equal(expected, new PortugueseConverter().Convert(input, reference: reference));
        Assert.Equal(expected, new PortugueseConverter().Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Talvez eu lembrei errado.", "Talvez eu me tenha lembrado mal.")]
    [InlineData("Acho que lembrei errado.", "Acho que me lembrei mal.")]
    [InlineData("O que deu errado?", "O que correu mal?")]
    [InlineData("Se der errado lá, o que fazes?", "Se correr mal lá, o que fazes?")]
    [InlineData("As coisas podem dar errado.", "As coisas podem correr mal.")]
    [InlineData("Deu tudo errado.", "Correu tudo mal.")]
    [InlineData("Vai dar tudo errado.", "Vai correr tudo mal.")]
    [InlineData("Ele deu errado o troco.", "Ele deu errado o troco.")]
    [InlineData("Você entendeu tudo errado.", "Percebeste tudo mal.")]
    [InlineData("Calculei errado o açúcar.", "Calculei mal o açúcar.")]
    [InlineData("Você me ouviu errado.", "Ouviste-me mal.")]
    [InlineData("Eu fiz tudo errado.", "Eu fiz tudo mal.")]
    [InlineData("Você leu errado.", "Leste mal.")]
    [InlineData("Ele está errado.", "Ele está errado.")]
    [InlineData("Acho errado.", "Acho errado.")]
    [InlineData("Esse é o jogo errado.", "Esse é o jogo errado.")]
    [InlineData("Escolheu o programa errado.", "Escolheu o programa errado.")]
    [InlineData("Usaram o dado errado.", "Usaram o dado errado.")]
    [InlineData("Ele tinha esquecido de ligar.", "Ele tinha-se esquecido de ligar.")]
    [InlineData("Eu tinha esquecido do nome dela.", "Eu tinha-me esquecido do nome dela.")]
    [InlineData("Você tinha esquecido de mim?", "Tinhas-te esquecido de mim?")]
    [InlineData("Ela teria esquecido de tudo.", "Ela ter-se-ia esquecido de tudo.")]
    [InlineData("Ele tinha esquecido o nome.", "Ele tinha-se esquecido do nome.")]
    [InlineData("Achei que ele tinha esquecido de mim.", "Achei que ele se tinha esquecido de mim.")]
    [InlineData("O que ele tinha esquecido?", "O que ele tinha esquecido?")]
    [InlineData("Eles tinham esquecido de trancar a porta.", "Eles tinham-se esquecido de trancar a porta.")]
    [InlineData("Nós tínhamos lembrado disso.", "Nós tínhamo-nos lembrado disso.")]
    [InlineData("Para você ver você mesmo.", "Para veres por ti mesmo.")]
    [InlineData("Veja você mesmo.", "Vê por ti mesmo.")]
    [InlineData("Vejam vocês mesmos.", "Vejam por vocês mesmos.")]
    [InlineData("Senhor, veja você mesmo.", "Senhor, veja por si mesmo.")]
    [InlineData("Faça você mesmo.", "Faz tu mesmo.")]
    [InlineData("-Deu errado.\n-Não sabemos que deu errado.", "-Correu mal.\n-Não sabemos que correu mal.")]
    public void WrongAsMannerAdverbAndReflexiveCompoundsUseEuropeanForms(string input, string expected)
    {
        Assert.Equal(expected, new PortugueseConverter().Convert(input));
        Assert.Equal(expected, new PortugueseConverter().Convert(expected));
    }

    [Fact]
    public void WhatWentWrongKeepsTheImpersonalSubject()
    {
        var reference = ReferenceHint.FromText("But unless you can articulate what went wrong, we're done.");
        Assert.Equal("Mas, se não disseres\no que correu mal, é isso.",
            new PortugueseConverter().Convert("Mas, se não disser\no que deu errado, é isso.", reference: reference));
    }

    [Theory]
    [InlineData("-Tinha muita gente neste andar.", "-Havia muita gente neste andar.")]
    [InlineData("Não tinha ninguém em casa.", "Não havia ninguém em casa.")]
    [InlineData("Tinha um cão quando era miúdo.", "Tinha um cão quando era miúdo.")]
    [InlineData("Desculpa por fazer vocês esperarem.", "Desculpem por fazer-vos esperar.")]
    [InlineData("Ele nunca fez vocês esperarem.", "Ele nunca vos fez esperar.")]
    [InlineData("Não quero deixar vocês irem sozinhos.", "Não quero deixar-vos ir sozinhos.")]
    [InlineData("Não foi o que Linda disse-lhe?", "Não foi o que Linda lhe disse?")]
    [InlineData("Sei que a Linda disse-lhe tudo.", "Sei que a Linda lhe disse tudo.")]
    [InlineData("Quando o Frank viu-me, fugiu.", "Quando o Frank me viu, fugiu.")]
    [InlineData("Não disse-lhe nada.", "Não lhe disse nada.")]
    [InlineData("Ninguém disse-me nada.", "Ninguém me disse nada.")]
    [InlineData("Ele disse-lhe tudo.", "Ele disse-lhe tudo.")]
    [InlineData("Não, disse-lhe tudo.", "Não, disse-lhe tudo.")]
    [InlineData("Não vou fazê-lo.", "Não vou fazê-lo.")]
    [InlineData("Uma das crianças pode ter pego.", "Uma das crianças pode ter pegado.")]
    [InlineData("Eu tinha chego tarde.", "Eu tinha chegado tarde.")]
    [InlineData("Tenham certeza.", "Tenham a certeza.")]
    [InlineData("O único jeito de resolver.", "A única forma de resolver.")]
    [InlineData("Ele tem jeito para isso.", "Ele tem jeito para isso.")]
    [InlineData("Ela nunca devia ter pego o caso.", "Ela nunca devia ter pegado no caso.")]
    [InlineData("Só lembre-se de mim.", "Lembra-te só de mim.")]
    [InlineData("Só lembra-te de quem enfrentas.", "Lembra-te só de quem enfrentas.")]
    [InlineData("Ele só lembrou-se de mim.", "Ele só se lembrou de mim.")]
    [InlineData("Já disse-te a verdade.", "Já te disse a verdade.")]
    public void BlueBloodsLeftoversUseEuropeanConstructions(string input, string expected)
    {
        Assert.Equal(expected, new PortugueseConverter().Convert(input));
        Assert.Equal(expected, new PortugueseConverter().Convert(expected));
    }

    [Theory]
    [InlineData("Se não puder, apoie-se na sua família.", "Se não puderes, apoia-te na tua família.")]
    [InlineData("Quando chegar, ligue-me.", "Quando chegares, liga-me.")]
    [InlineData("Se se sentir mal, deite-se.", "Se te sentires mal, deita-te.")]
    [InlineData("Caso precise, chame a polícia.", "Caso precises, chama a polícia.")]
    [InlineData("Se ele ligar, avise-me.", "Se ele ligar, avisa-me.")]
    [InlineData("Se puder, ligo-te amanhã.", "Se puder, ligo-te amanhã.")]
    [InlineData("Senhor, se puder, sente-se.", "Senhor, se puder, sente-se.")]
    [InlineData("Desculpe por fazer vocês esperarem.", "Desculpem por fazer-vos esperar.")]
    [InlineData("Desculpe pelo vosso carro.", "Desculpem pelo vosso carro.")]
    [InlineData("Desculpa, pessoal.", "Desculpem, pessoal.")]
    [InlineData("Desculpe pelo atraso.", "Desculpa pelo atraso.")]
    [InlineData("Desculpe, Frank.", "Desculpa, Frank.")]
    [InlineData("Frank tinha esquecido de ligar.", "Frank tinha-se esquecido de ligar.")]
    [InlineData("Ontem tinha esquecido de ligar.", "Ontem tinha esquecido de ligar.")]
    [InlineData("Tinhas entendido mal.", "Tinhas percebido mal.")]
    [InlineData("Eu tinha entendido tudo.", "Eu tinha percebido tudo.")]
    [InlineData("Entendido.", "Entendido.")]
    [InlineData("Tudo bem, fique aqui.", "Tudo bem, fica aqui.")]
    [InlineData("Se correr mal, chame reforço.", "Se correr mal, chama reforço.")]
    [InlineData("Se é o que quer, empreste-me dez mil.", "Se é o que queres, empresta-me dez mil.")]
    [InlineData("Quando é seguido, fique parado.", "Quando és seguido, fica parado.")]
    [InlineData("Se é proibido, saia.", "Se é proibido, sai.")]
    [InlineData("Se for preciso, ligue.", "Se for preciso, liga.")]
    [InlineData("Se ele lembrar-se de algo útil, avise-me.", "Se ele se lembrar de algo útil, avisa-me.")]
    [InlineData("Se está cansado, durma.", "Se estás cansado, dorme.")]
    [InlineData("És bom e ganha sempre.", "És bom e ganhas sempre.")]
    [InlineData("Só não ferva minhas bolas de novo.", "Só não fervas as minhas bolas de novo.")]
    [InlineData("Não toque minhas bolas.", "Não toques nas minhas bolas.")]
    [InlineData("Não toque a minha mão.", "Não me toques na mão.")]
    [InlineData("Ele tocou sua música.", "Ele tocou a sua música.")]
    [InlineData("Não corte a corda.", "Não cortes a corda.")]
    [InlineData("O corte foi fundo.", "O corte foi fundo.")]
    [InlineData("Desculpa fazer vocês celebrarem do meu jeito.", "Desculpem fazer-vos celebrar à minha maneira.")]
    [InlineData("Faz do teu jeito.", "Faz à tua maneira.")]
    [InlineData("Fiz do jeito dele.", "Fiz à maneira dele.")]
    [InlineData("Do jeito que está, não dá.", "Como está, não dá.")]
    [InlineData("Não atenda se não quiser falar.", "Não atendas se não quiseres falar.")]
    [InlineData("Não cometa o crime,\nse não puder cumprir a pena.", "Não cometas o crime,\nse não puderes cumprir a pena.")]
    [InlineData("Não venda a casa.", "Não vendas a casa.")]
    [InlineData("Liga-me quando puder.", "Liga-me quando puderes.")]
    [InlineData("Sente-se aí.", "Senta-te aí.")]
    [InlineData("Não se sente aí.", "Não te sentes aí.")]
    [InlineData("Se sentirá diferente amanhã.", "Sentir-se-á diferente amanhã.")]
    [InlineData("Se fará justiça.", "Far-se-á justiça.")]
    [InlineData("Sinto o meu útero se rasgando ao meio.", "Sinto o meu útero a rasgar-se ao meio.")]
    [InlineData("Estás a cuidar de si mesmo.", "Estás a cuidar de ti mesmo.")]
    [InlineData("Você devia se livrar dessa gente, não se juntar a eles.", "Devias livrar-te dessa gente, não te juntar a eles.")]
    [InlineData("Devias se livrar dela.", "Devias livrar-te dela.")]
    [InlineData("Provável demoremos aqui.", "É provável que demoremos aqui.")]
    [InlineData("Impossível que ele saiba.", "É impossível que ele saiba.")]
    [InlineData("Nunca me retornou sobre o negócio.", "Nunca me respondeu sobre o negócio.")]
    [InlineData("Ele me retornou o dinheiro.", "Ele devolveu-me o dinheiro.")]
    [InlineData("Vou retornar a ligação.", "Vou devolver a chamada.")]
    [InlineData("Vou retornar para casa.", "Vou retornar para casa.")]
    [InlineData("Porque você nunca sentava quando vinha aqui.", "Porque nunca te sentavas quando vinhas aqui.")]
    [InlineData("Sentamos na mesa.", "Sentamo-nos na mesa.")]
    [InlineData("Quando ele sentou, começou.", "Quando ele se sentou, começou.")]
    [InlineData("Você sentou ali.", "Sentaste-te ali.")]
    [InlineData("Ela sentou o bebé na cadeira.", "Ela sentou o bebé na cadeira.")]
    [InlineData("Mentiste para o Lugo.", "Mentiste ao Lugo.")]
    [InlineData("Perguntei para a minha mãe.", "Perguntei à minha mãe.")]
    [InlineData("Pergunta para o pai vir.", "Pergunta para o pai vir.")]
    [InlineData("Tu ligaste quando chegou?", "Tu ligaste quando chegaste?")]
    [InlineData("Viste o homem quando chegou?", "Viste o homem quando chegou?")]
    [InlineData("Tu choravas quando era tarde.", "Tu choravas quando era tarde.")]
    [InlineData("Você bem que podia ajudar.", "Bem que podias ajudar.")]
    [InlineData("Ele bem que podia ajudar.", "Ele bem que podia ajudar.")]
    [InlineData("Para de se mexer.", "Para de te mexer.")]
    [InlineData("Tenta não se machucar.", "Tenta não te magoar.")]
    [InlineData("Vem e se junte a nós.", "Vem e junta-te a nós.")]
    [InlineData("Vem se sentar aqui.", "Vem sentar-te aqui.")]
    [InlineData("Ela adora se divertir.", "Ela adora divertir-se.")]
    [InlineData("Cubra o cabelo.", "Cobre o cabelo.")]
    [InlineData("Eu te retorno.", "Eu respondo-te.")]
    [InlineData("Possível ataque no porto.", "Possível ataque no porto.")]
    [InlineData("Tenho perguntas pro papai.", "Tenho perguntas para o pai.")]
    [InlineData("Fica quieto e me ligue.", "Fica quieto e liga-me.")]
    [InlineData("Respeite isso, entendeu?", "Respeita isso, percebeste?")]
    [InlineData("Olhe, ele é bom amigo, mas quando se trata de mulheres, não presta.", "Olha, ele é bom amigo, mas quando se trata de mulheres, não presta.")]
    [InlineData("Vamos sair domingo a noite.", "Vamos sair domingo à noite.")]
    [InlineData("Saí ontem a noite.", "Saí ontem à noite.")]
    [InlineData("Passei sábado a noite toda a ler.", "Passei sábado a noite toda a ler.")]
    [InlineData("Vemos-nos amanhã.", "Vemo-nos amanhã.")]
    [InlineData("Ouça, não vai contar à minha mãe, certo?", "Ouça, não vai contar à minha mãe, certo?")]
    [InlineData("Você adora se abrir demais.", "Adoras abrir-te demais.")]
    [InlineData("Você tenta se esconder.", "Tentas esconder-te.")]
    [InlineData("Tu precisas de acalmar-se.", "Tu precisas de acalmar-te.")]
    [InlineData("Ele deixou-o se vestir.", "Ele deixou-o vestir-se.")]
    [InlineData("Ela mandou-nos se sentar.", "Ela mandou-nos sentar.")]
    [InlineData("Ele deixa a menina se vestir.", "Ele deixa a menina vestir-se.")]
    [InlineData("Eu vi o homem se afogar.", "Eu vi o homem afogar-se.")]
    [InlineData("Ela faz as crianças se deitarem cedo.", "Ela faz as crianças deitarem-se cedo.")]
    [InlineData("Você viu ele se jogar?", "Viste-o jogar-se?")]
    [InlineData("Ele deixa o carro se ganhar.", "Ele deixa o carro se ganhar.")]
    [InlineData("Que papo é esse?", "Que conversa é essa?")]
    [InlineData("Que papo era aquele?", "Que conversa era aquela?")]
    [InlineData("Se faz você se sentir melhor, já me vinguei dele.", "Se te faz sentir melhor, já me vinguei dele.")]
    [InlineData("Eu mandei você se calar.", "Eu mandei-te calar.")]
    [InlineData("Não que estar certo te ajude.", "Não que estar certo te ajude.")]
    [InlineData("O problema é que estar sozinho me assusta.", "O problema é que estar sozinho me assusta.")]
    [InlineData("O que você andou fazendo?", "O que andaste a fazer?")]
    [InlineData("Eu via você se afastar\nquando eu me aproximava demais", "Eu via-te afastar\nquando eu me aproximava demais")]
    [InlineData("Que cara de bunda é essa?", "Que cara de rabo é essa?")]
    [InlineData("Mas eu acho você chique demais.", "Mas eu acho-te muito chique.")]
    [InlineData("Não acho você bonita.", "Não te acho bonita.")]
    [InlineData("Sempre achei você muito inteligente, sabia?", "Sempre te achei muito inteligente, sabia?")]
    [InlineData("Ela acha você engraçado e simpático.", "Ela acha-te engraçado e simpático.")]
    public void RemainingAddressAndReflexiveGapsUseEuropeanForms(string input, string expected)
        => Assert.Equal(expected, new PortugueseConverter().Convert(input));

    [Theory]
    [InlineData("Mandy, como se sentiu\nquando encontrou sua mãe assim?", "Mandy, how did you feel when you found your mother this way?",
        "Mandy, como te sentiste\nquando encontraste a tua mãe assim?")]
    [InlineData("você só aparece quando precisa de algo,\nnunca me retornou sobre o negócio,", "You only appear when you need something,\nyou never got back to me about the thing,",
        "só apareces quando precisas de algo,\nnunca me respondeste sobre o negócio,")]
    [InlineData("Pergunte ao Bobby\nquando fizer a oferta.", "You'll have to ask Bobby when you make the offer.",
        "Pergunta ao Bobby\nquando fizeres a oferta.")]
    [InlineData("Venha me procurar\nquando tiver mais.", "You need to find me... when you have more.",
        "Vem procurar-me\nquando tiveres mais.")]
    [InlineData("Se sentirá diferente\nquando socar pessoas.", "You might feel differently when you start punching people.",
        "Sentir-te-ás diferente\nquando socares pessoas.")]
    [InlineData("Serei honesta, Reagan,\nquando mentiu para o Lugo", "I'll be honest with you, Reagan, when you lied to Lugo",
        "Serei honesta, Reagan,\nquando mentiste ao Lugo")]
    [InlineData("Você podia se beneficiar ao pegar\ndinheiro de Bobby Glass, mas recusou.", "You were offered the benefits\nof taking Bobby Glass's money, but you refused.",
        "Podias beneficiar-te ao pegar\ndinheiro de Bobby Glass, mas recusaste.")]
    [InlineData("Então, quando ganhar hoje,\nvocê bem que podia me beijar.", "So when you win today, you could kiss me.",
        "Então, quando ganhares hoje,\nbem que me podias beijar.")]
    [InlineData("Caramba, Eddie,\npor que pergunta se já sabe?", "Jeez, Eddie, why do you ask if you already know?",
        "Caramba, Eddie,\nporque perguntas se já sabes?")]
    [InlineData("Não pode fazer isso\nse não estiver cuidando de si mesmo.", "You can't really do that if you're not taking care of yourself then.",
        "Não podes fazer isso\nse não estiveres a cuidar de ti mesmo.")]
    [InlineData("O que está acontecendo?", "What are you doing?", "O que está a acontecer?")]
    [InlineData("Por que quer mesmo\ndesistir do seu distintivo?", "Why do you really want to hang up your shield?",
        "Porque queres mesmo\ndesistir do teu distintivo?")]
    [InlineData("Porque se você não está falhando,\nnão está tentando,", "Because if you aren't failing, you aren't trying,",
        "Porque se não estás a falhar,\nnão estás a tentar,")]
    [InlineData("Quando você era pequena,\nnão conseguia dormir.", "Yeah, when you were little, you couldn't sleep.",
        "Quando eras pequena,\nnão conseguias dormir.")]
    [InlineData("O que aconteceu? Do que se lembra?", "Tell me what happened. What do you remember?",
        "O que aconteceu? Do que te lembras?")]
    [InlineData("Se você acha que é fácil, esquece.", "If you think it's easy, forget it.",
        "Se achas que é fácil, esquece.")]
    [InlineData("O que se passou?", "What happened to you?", "O que se passou?")]
    [InlineData("que lutou para salvar quando\nestava na Comissão Histórica.", "you fought to save when you were on the landmark commission.",
        "que lutaste para salvar quando\nestavas na Comissão Histórica.")]
    [InlineData("Não faça perguntas\npara as quais não quer a resposta.", "Look, don't ask questions you don't want to know the answer to.",
        "Não faças perguntas\npara as quais não queres a resposta.")]
    [InlineData("Você mostrou o livro que estava na mesa.", "You showed the book that was on the table.",
        "Mostraste o livro que estava na mesa.")]
    [InlineData("Desculpe, por que vai apreender?", "Excuse me, why are you taking this?",
        "Desculpa, porque vais apreender?")]
    [InlineData("Desculpe, mas o senhor vai sair?", "Excuse me, sir, are you leaving?",
        "Desculpe, mas o senhor vai sair?")]
    [InlineData("Ele quer sair com você\ndomingo a noite, se puder.", "He wants to take you out Sunday night if you're free.",
        "Ele quer sair contigo\ndomingo à noite, se puderes.")]
    [InlineData("Ele vai te ajudar se puder.", "He'll help you if he can.", "Ele vai ajudar-te se puder.")]
    [InlineData("Respeite isso, entendeu?", "You need to respect that, understand?", "Respeita isso, percebeste?")]
    [InlineData("Você, fique com a avó, está bem?", "You stay with grandma, okay?", "Tu, fica com a avó, está bem?")]
    [InlineData("Você precisa se acalmar.", "You need to calm down.", "Precisas de te acalmar.")]
    [InlineData("E você, me viu\nou falou comigo depois do café?", "And you, did you see me or talk to me since breakfast?",
        "E tu, viste-me\nou falaste comigo depois do café?")]
    [InlineData("Depois que você sabe, não tem volta.", "Once you know, you can't unknow.", "Depois que sabes, não tem volta.")]
    [InlineData("Depois que terminou seu aperitivo,\npassou para a entrada.", "After you finished your appetizer, you moved on to the entree.",
        "Depois que terminaste o teu aperitivo,\npassaste para a entrada.")]
    [InlineData("Tenha modos, porra.", "You watch your manners.", "Tem modos, porra.")]
    [InlineData("Saia daqui!", "Get out!", "Sai daqui!")]
    [InlineData("Espere aqui, amigo.", "You wait here, pal.", "Espera aqui, amigo.")]
    [InlineData("Explique como essa mulher", "Now, you explain to me how this woman", "Explica como esta mulher")]
    [InlineData("Avise que o disfarce dele já era.", "You've gotta call him and tell him his cover's blown.", "Avisa que o disfarce dele já era.")]
    [InlineData("Você sabe que não.", "You know it's not.", "Sabes que não.")]
    [InlineData("Por que decidiu nos ajudar?", "Why did you decide to help us?", "Porque decidiste ajudar-nos?")]
    [InlineData("Por que nos ajudou?", "Why did you help us?", "Porque nos ajudaste?")]
    [InlineData("Onde o deixou?", "Where did you leave him?", "Onde o deixaste?")]
    [InlineData("Onde deixou isso?", "Where did you leave it?", "Onde deixaste isso?")]
    [InlineData("Onde colocou a arma?", "Where did you put the gun?", "Onde puseste a arma?")]
    [InlineData("Quando chegou a carta?", "When did you get the letter?", "Quando chegou a carta?")]
    [InlineData("Por que ele nos ajudou?", "Why did you think he helped us?", "Porque ele nos ajudou?")]
    [InlineData("Como ele te achou?", "How did he find you?", "Como ele te achou?")]
    [InlineData("Tem o meu número.", "You have my number.\nGive me a call when you get a bead on that kid.", "Tens o meu número.")]
    [InlineData("Pode ir aproveitar seu fim de semana.", "You go have your little weekend.", "Podes ir aproveitar o teu fim de semana.")]
    [InlineData("Lembra do meu noivo, David.", "You remember my fiancé, David.", "Lembras-te do meu noivo, David.")]
    [InlineData("Que papo é esse? Como pode ter certeza?", "What the hell? Um, how... How are you sure though?", "Que conversa é essa? Como podes ter a certeza?")]
    [InlineData("Pode pegar um caderninho\nque está na minha bolsa?", "Can you reach into my purse and hand me my tiny notebook?", "Podes pegar num caderninho\nque está na minha bolsa?")]
    [InlineData("De que merda está falando?", "What the fuck are you talkin' about?", "De que merda estás a falar?")]
    [InlineData("Para que arma se só vai bater na porta?", "Why are you takin' a gun if you just have to knock on the door?", "Para que arma se só vais bater à porta?")]
    [InlineData("Isso vai a julgamento mês que vem.", "Would you mind if I take a look? This is going to trial next month.", "Isto vai a julgamento mês que vem.")]
    [InlineData("O que andou fazendo?", "what have you been up to?", "O que andaste a fazer?")]
    [InlineData("É a última vez que esvaziou isso?", "When was the last time you emptied these?", "É a última vez que esvaziaste isto?")]
    [InlineData("E na próxima vez que precisar de ajuda", "And next time you need help", "E na próxima vez que precisares de ajuda")]
    [InlineData("para que aprenda com seus erros?", "so that you can learn from your mistake?", "para que aprendas com os teus erros?")]
    [InlineData("com que ia nos dizer, Joe?", "what you were gonna tell us, Joe?", "com que nos ias dizer, Joe?")]
    [InlineData("Com que vai estourar o balão\ndessa vez?", "What are you gonna pop the balloon with this time?", "Com que vais estourar o balão\ndesta vez?")]
    [InlineData("Que teve um problema ruim.", "but you got a bad problem.", "Que tiveste um problema mau.")]
    [InlineData("- Que paga muito mais.\n- Pai, você ama seu trabalho.", "A much higher paying job.\nBut Dad, you love your job.", "- Que paga muito mais.\n- Pai, amas o teu trabalho.")]
    [InlineData("Para que serve isso?", "What is it for? You know.", "Para que serve isso?")]
    [InlineData("Que sorte!", "You are so lucky.", "Que sorte!")]
    [InlineData("Que drama é esse com a noite livre?", "You're making way too much of this night off.", "Que drama é esse com a noite livre?")]
    [InlineData("Que porra é essa?", "What the fuck is this?", "Que porra é esta?")]
    [InlineData("mas eu te acho chique demais.", "but I think you're very chic.", "mas eu acho-te muito chique.")]
    public void ReferenceConfirmedSecondPersonClausesUseTu(string input, string english, string expected)
        => Assert.Equal(expected, new PortugueseConverter().Convert(input, null, ReferenceHint.FromText(english)));
}
