using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>
/// Small Portuguese verb toolkit: maps "você" (3rd person singular) forms to "tu" (2nd person
/// singular) forms, knows which words are verb forms that can take an object pronoun, and attaches
/// enclitic pronouns the European way (fazer + o → fazê-lo, amam + o → amam-no, vemos + nos → vemo-nos).
/// Listed forms and dictionary-validated regular inflections are conjugated only in supported subject contexts.
/// </summary>
internal static class PortugueseVerbs
{
    private static readonly string[] RegularAr =
    {
        "abraçar", "acabar", "aceitar", "achar", "acordar", "acreditar", "acusar", "adorar", "ajudar", "alugar",
        "amar", "andar", "apagar", "apertar", "apontar", "apostar", "apresentar", "arrumar", "assinar", "assustar",
        "atacar", "atirar", "atrapalhar", "avisar", "beijar", "brincar", "buscar", "cancelar", "cantar", "carregar",
        "casar", "chamar", "chegar", "chorar", "cobrar", "colocar", "começar", "combinar", "comprar", "confessar",
        "confiar", "consertar", "contar", "continuar", "contratar", "conversar", "convidar", "cortar", "cozinhar", "criar",
        "cuidar", "culpar", "custar", "dançar", "deixar", "deitar", "descansar", "desculpar", "desligar", "desejar",
        "desenhar", "detestar", "dispensar", "duvidar", "economizar", "empurrar", "encontrar", "enganar", "ensinar", "entrar",
        "entregar", "enviar", "errar", "escutar", "esperar", "estudar", "evitar", "explicar", "falar", "faltar",
        "fechar", "ficar", "filmar", "fumar", "ganhar", "gastar", "gostar", "gritar", "guardar", "ignorar",
        "imaginar", "importar", "incomodar", "jantar", "jogar", "julgar", "lavar", "lembrar", "levantar", "levar",
        "ligar", "limpar", "lutar", "machucar", "magoar", "mandar", "marcar", "matar", "melhorar", "mudar",
        "morar", "mostrar", "nadar", "namorar", "notar", "obrigar", "olhar", "organizar", "pagar", "parar",
        "passar", "pegar", "pensar", "perdoar", "perguntar", "pisar", "precisar", "preocupar", "preparar", "procurar",
        "provar", "pular", "puxar", "quebrar", "reparar", "respirar", "roubar", "salvar", "segurar", "sentar",
        "separar", "soltar", "sonhar", "tentar", "terminar", "tirar", "tocar", "tomar", "trabalhar", "tratar",
        "trancar", "trocar", "usar", "viajar", "vigiar", "visitar", "voltar", "votar",
    };

    private static readonly string[] RegularEr =
    {
        "aprender", "atender", "bater", "beber", "comer", "compreender", "conhecer", "convencer", "correr", "defender",
        "depender", "dever", "devolver", "entender", "escolher", "esconder", "escrever", "esquecer", "merecer", "meter",
        "mexer", "morrer", "mover", "oferecer", "prender", "proteger", "prometer", "receber", "reconhecer", "resolver",
        "responder", "sofrer", "surpreender", "temer", "vender", "viver", "agradecer", "aparecer", "acontecer", "parecer",
    };

    private static readonly string[] RegularIr =
    {
        "abrir", "admitir", "assistir", "decidir", "desistir", "discutir", "dividir", "existir", "insistir", "partir",
        "permitir", "resistir", "garantir", "cumprir", "assumir", "dirigir", "exigir", "fingir", "corrigir", "atingir",
        "omitir", "unir", "imprimir", "invadir", "proibir",
    };

    /// <summary>Verbs that can take a direct object pronoun ("vi ele" → "vi-o").</summary>
    private static readonly HashSet<string> TransitiveInfinitives = new(StringComparer.Ordinal)
    {
        "abraçar", "acordar", "acusar", "adorar", "ajudar", "amar", "apresentar", "assustar", "atacar", "avisar",
        "beijar", "buscar", "chamar", "colocar", "confessar", "contratar", "convidar", "culpar", "deixar", "desculpar",
        "detestar", "encontrar", "enganar", "esperar", "escutar", "evitar", "ignorar", "incomodar", "julgar", "levar",
        "machucar", "magoar", "matar", "mudar", "mostrar", "olhar", "pegar", "perdoar", "preocupar", "procurar",
        "proteger", "provar", "puxar", "roubar", "salvar", "segurar", "soltar", "trancar", "tratar", "vigiar", "visitar",
        "conhecer", "convencer", "defender", "devolver", "entender", "escolher", "esconder", "esquecer", "prender",
        "receber", "reconhecer", "surpreender", "abrir", "assistir", "corrigir", "dividir", "fechar", "empurrar",
        "guardar", "limpar", "lavar", "trocar", "ver", "fazer", "trazer", "pôr", "ouvir", "seguir", "perder",
        "querer", "amar", "odiar", "achar", "ligar", "imaginar", "curtir", "apoiar", "respeitar", "admirar", "acompanhar",
        "desapontar", "decepcionar", "expulsar", "demitir", "despedir", "sequestrar", "raptar", "apanhar", "agarrar", "largar",
        "abandonar", "trair", "chantagear", "ameaçar", "denunciar", "substituir", "treinar", "impedir", "deter", "interrogar",
        "investigar", "aceitar", "enterrar", "tirar", "vigiar", "testar", "avaliar", "obrigar", "forçar", "prejudicar",
        "mencionar", "mandar", "usar", "entregar", "separar", "impressionar", "estrangular", "caçar", "comer", "sacanear",
        "dissuadir", "ferir", "criar", "algemar", "ensinar", "distrair", "socar", "recrutar", "verificar", "rejeitar",
        "subestimar", "pressionar", "citar", "inventar", "desafiar", "atualizar", "cheirar", "espionar", "vender", "rastrear",
        "superar", "vaiar", "derrubar", "abordar", "liquidar", "atrasar", "conquistar", "invejar", "alcançar", "traumatizar",
        "envolver", "chutar", "revistar", "drogar", "atropelar", "calar", "afastar", "elogiar", "convocar", "juntar",
        "resgatar", "namorar", "controlar", "levantar", "provocar", "sacrificar", "desligar", "algemar", "amarrar", "apertar",
        "assassinar", "atropelar", "avistar", "beliscar", "capturar", "cercar", "cumprimentar", "deixar", "desmascarar",
        "despir", "dominar", "educar", "empregar", "encurralar", "enfrentar", "enganar", "esfaquear", "esmagar", "espancar",
        "estragar", "examinar", "humilhar", "identificar", "interromper", "irritar", "libertar", "localizar", "mencionar",
        "molestar", "odiar", "operar", "perseguir", "persuadir", "picar", "prender", "processar", "proibir", "raptar",
        "receber", "salvar", "seduzir", "sufocar", "torturar", "transferir", "vestir", "acalmar", "alimentar", "amaldiçoar",
    };

    // Present-tense "tu" forms that are more often plural nouns ("as chamas", "as provas").
    private static readonly HashSet<string> SecondPersonNouns = new(StringComparer.Ordinal)
    {
        "chamas", "provas", "contas", "marcas", "visitas", "notas", "cobras", "trocas", "culpas", "esperas", "ajudas", "mudas",
        "pegas", "buscas", "desculpas", "levas", "operas", "picas", "cercas", "juntas", "criadas", "vendas", "receitas", "salvas",
        "torturas", "drogas", "citas", "pressas", "dominas", "fechas", "lavas", "limpas", "trancas", "seguras", "olhas", "ligas",
    };

    /// <summary>Irregular "você" → "tu" forms (present, past, imperfect, subjunctive, future subjunctive).</summary>
    private static readonly Dictionary<string, string> IrregularThirdToSecond = new(StringComparer.Ordinal)
    {
        // ser
        ["é"] = "és", ["foi"] = "foste", ["era"] = "eras", ["seja"] = "sejas", ["fosse"] = "fosses", ["for"] = "fores", ["será"] = "serás", ["seria"] = "serias", ["ser"] = "seres",
        // estar
        ["está"] = "estás", ["esteve"] = "estiveste", ["estava"] = "estavas", ["esteja"] = "estejas", ["estivesse"] = "estivesses", ["estiver"] = "estiveres", ["estará"] = "estarás", ["estaria"] = "estarias",
        // ter
        ["tem"] = "tens", ["teve"] = "tiveste", ["tinha"] = "tinhas", ["tenha"] = "tenhas", ["tivesse"] = "tivesses", ["tiver"] = "tiveres", ["terá"] = "terás", ["teria"] = "terias",
        // ir
        ["vai"] = "vais", ["ia"] = "ias", ["vá"] = "vás", ["irá"] = "irás", ["iria"] = "irias",
        // vir
        ["vem"] = "vens", ["veio"] = "vieste", ["vinha"] = "vinhas", ["venha"] = "venhas", ["viesse"] = "viesses", ["vier"] = "vieres", ["virá"] = "virás", ["viria"] = "virias",
        // fazer
        ["faz"] = "fazes", ["fez"] = "fizeste", ["fazia"] = "fazias", ["faça"] = "faças", ["fizesse"] = "fizesses", ["fizer"] = "fizeres", ["fará"] = "farás", ["faria"] = "farias",
        // dizer
        ["diz"] = "dizes", ["disse"] = "disseste", ["dizia"] = "dizias", ["diga"] = "digas", ["dissesse"] = "dissesses", ["disser"] = "disseres", ["dirá"] = "dirás", ["diria"] = "dirias",
        // trazer
        ["traz"] = "trazes", ["trouxe"] = "trouxeste", ["trazia"] = "trazias", ["traga"] = "tragas", ["trouxer"] = "trouxeres", ["trará"] = "trarás", ["traria"] = "trarias",
        // poder
        ["pode"] = "podes", ["pôde"] = "pudeste", ["podia"] = "podias", ["possa"] = "possas", ["pudesse"] = "pudesses", ["puder"] = "puderes", ["poderá"] = "poderás", ["poderia"] = "poderias",
        // querer
        ["quer"] = "queres", ["quis"] = "quiseste", ["queria"] = "querias", ["queira"] = "queiras", ["quisesse"] = "quisesses", ["quiser"] = "quiseres", ["quererá"] = "quererás",
        // saber
        ["sabe"] = "sabes", ["soube"] = "soubeste", ["sabia"] = "sabias", ["saiba"] = "saibas", ["soubesse"] = "soubesses", ["souber"] = "souberes", ["saberá"] = "saberás", ["saberia"] = "saberias",
        // dar
        ["dá"] = "dás", ["deu"] = "deste", ["dava"] = "davas", ["dê"] = "dês", ["desse"] = "desses", ["der"] = "deres", ["dará"] = "darás", ["daria"] = "darias",
        // ver
        ["vê"] = "vês", ["viu"] = "viste", ["via"] = "vias", ["veja"] = "vejas", ["visse"] = "visses", ["vir"] = "vires", ["verá"] = "verás", ["veria"] = "verias",
        // ler, crer, pôr
        ["lê"] = "lês", ["leu"] = "leste", ["lia"] = "lias", ["leia"] = "leias",
        ["crê"] = "crês",
        ["põe"] = "pões", ["pôs"] = "puseste", ["punha"] = "punhas", ["ponha"] = "ponhas", ["puser"] = "puseres", ["pusesse"] = "pusesses",
        // sair, cair, rir, ouvir, pedir, perder, dormir, sentir, mentir, seguir, conseguir, preferir, subir, fugir, odiar
        ["sai"] = "sais", ["saiu"] = "saíste", ["saía"] = "saías", ["saia"] = "saias",
        ["cai"] = "cais", ["caiu"] = "caíste", ["caía"] = "caías",
        ["ri"] = "ris", ["riu"] = "riste",
        ["ouve"] = "ouves", ["ouviu"] = "ouviste", ["ouvia"] = "ouvias", ["ouça"] = "ouças",
        ["pede"] = "pedes", ["pediu"] = "pediste", ["pedia"] = "pedias", ["peça"] = "peças",
        ["perde"] = "perdes", ["perdeu"] = "perdeste", ["perdia"] = "perdias", ["perca"] = "percas",
        ["dorme"] = "dormes", ["dormiu"] = "dormiste", ["dormia"] = "dormias", ["durma"] = "durmas",
        ["sente"] = "sentes", ["sentiu"] = "sentiste", ["sentia"] = "sentias", ["sinta"] = "sintas",
        ["mente"] = "mentes", ["mentiu"] = "mentiste", ["mentia"] = "mentias", ["minta"] = "mintas",
        ["segue"] = "segues", ["seguiu"] = "seguiste", ["seguia"] = "seguias", ["siga"] = "sigas",
        ["consegue"] = "consegues", ["conseguiu"] = "conseguiste", ["conseguia"] = "conseguias", ["consiga"] = "consigas", ["conseguir"] = "conseguires",
        ["prefere"] = "preferes", ["preferiu"] = "preferiste", ["preferia"] = "preferias", ["prefira"] = "prefiras",
        ["sobe"] = "sobes", ["subiu"] = "subiste",
        ["foge"] = "foges", ["fugiu"] = "fugiste",
        ["odeia"] = "odeias", ["odiou"] = "odiaste", ["odiava"] = "odiavas",
        ["chateia"] = "chateias", ["passeia"] = "passeias",
        ["acha"] = "achas",
    };

    /// <summary>Irregular forms that can take an object pronoun.</summary>
    private static readonly string[] IrregularTransitiveForms =
    {
        "vejo", "vê", "vemos", "veem", "vi", "viu", "vimos", "viram", "via", "viam", "veja", "ver",
        "faço", "faz", "fazemos", "fazem", "fiz", "fez", "fizemos", "fizeram", "fazia", "faça", "fazer",
        "trago", "traz", "trazemos", "trazem", "trouxe", "trouxemos", "trouxeram", "traga", "trazer",
        "ponho", "põe", "pomos", "põem", "pus", "pôs", "pusemos", "puseram", "ponha", "pôr",
        "ouço", "ouve", "ouvimos", "ouvem", "ouvi", "ouviu", "ouviram", "ouça", "ouvir",
        "sigo", "segue", "seguimos", "seguem", "segui", "seguiu", "seguiram", "siga", "seguir",
        "perco", "perde", "perdemos", "perdem", "perdi", "perdeu", "perderam", "perca", "perder",
        "quero", "quer", "queremos", "querem", "quis", "quiseram", "queria", "querer", "queira", "queiram", "quisesse", "quisessem", "ter",
        "odeio", "odeia", "odiamos", "odeiam", "odiei", "odiou", "odiar",
        "mantenho", "mantém", "mantemos", "mantêm", "mantive", "manteve", "mantivemos", "mantiveram", "mantinha", "mantenha", "manter", "mantiveste", "manténs",
        "traio", "trai", "traímos", "traem", "traí", "traiu", "traíram", "traía", "traia", "trair", "traíste", "trais",
        "viste", "vês", "vias", "fizeste", "fazes", "fazias", "trouxeste", "trazes", "puseste", "pões", "ouviste", "ouves",
        "seguiste", "segues", "perdeste", "perdes", "quiseste", "queres", "querias", "odiaste", "odeias",
    };

    private static readonly HashSet<string> InfinitiveStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "por", "mar", "lugar", "bar", "par", "lar", "ar", "já", "açúcar", "celular", "militar", "familiar",
        "particular", "popular", "similar", "singular", "polar", "escolar", "solar", "nuclear", "exemplar",
        "altar", "mulher", "qualquer", "talher", "prazer", "melhor", "pior", "maior", "menor", "amor", "favor",
        "senhor", "dor", "cor", "flor", "elixir", "faquir", "colher", "poder", "dever", "jantar", "olhar",
    };

    /// <summary>Irregular "a gente" (3sg) → "nós" (1pl) forms.</summary>
    private static readonly Dictionary<string, string> IrregularThirdToFirstPlural = new(StringComparer.Ordinal)
    {
        ["é"] = "somos", ["foi"] = "fomos", ["era"] = "éramos", ["seja"] = "sejamos", ["fosse"] = "fôssemos", ["for"] = "formos", ["será"] = "seremos", ["seria"] = "seríamos",
        ["está"] = "estamos", ["esteve"] = "estivemos", ["estava"] = "estávamos", ["esteja"] = "estejamos", ["estivesse"] = "estivéssemos", ["estiver"] = "estivermos", ["estará"] = "estaremos", ["estaria"] = "estaríamos",
        ["tem"] = "temos", ["teve"] = "tivemos", ["tinha"] = "tínhamos", ["tenha"] = "tenhamos", ["tivesse"] = "tivéssemos", ["tiver"] = "tivermos", ["terá"] = "teremos", ["teria"] = "teríamos",
        ["vai"] = "vamos", ["ia"] = "íamos", ["vá"] = "vamos", ["irá"] = "iremos", ["iria"] = "iríamos",
        ["vem"] = "vimos", ["veio"] = "viemos", ["vinha"] = "vínhamos", ["venha"] = "venhamos", ["viesse"] = "viéssemos", ["vier"] = "viermos", ["virá"] = "viremos", ["viria"] = "viríamos",
        ["faz"] = "fazemos", ["fez"] = "fizemos", ["fazia"] = "fazíamos", ["faça"] = "façamos", ["fizesse"] = "fizéssemos", ["fizer"] = "fizermos", ["fará"] = "faremos", ["faria"] = "faríamos",
        ["diz"] = "dizemos", ["disse"] = "dissemos", ["dizia"] = "dizíamos", ["diga"] = "digamos", ["dissesse"] = "disséssemos", ["disser"] = "dissermos", ["dirá"] = "diremos", ["diria"] = "diríamos",
        ["traz"] = "trazemos", ["trouxe"] = "trouxemos", ["trazia"] = "trazíamos", ["traga"] = "tragamos",
        ["pode"] = "podemos", ["pôde"] = "pudemos", ["podia"] = "podíamos", ["possa"] = "possamos", ["pudesse"] = "pudéssemos", ["puder"] = "pudermos", ["poderá"] = "poderemos", ["poderia"] = "poderíamos",
        ["quer"] = "queremos", ["quis"] = "quisemos", ["queria"] = "queríamos", ["queira"] = "queiramos", ["quisesse"] = "quiséssemos", ["quiser"] = "quisermos",
        ["sabe"] = "sabemos", ["soube"] = "soubemos", ["sabia"] = "sabíamos", ["saiba"] = "saibamos", ["soubesse"] = "soubéssemos", ["souber"] = "soubermos",
        ["dá"] = "damos", ["deu"] = "demos", ["dava"] = "dávamos", ["dê"] = "demos", ["desse"] = "déssemos", ["der"] = "dermos",
        ["vê"] = "vemos", ["viu"] = "vimos", ["via"] = "víamos", ["veja"] = "vejamos", ["visse"] = "víssemos", ["vir"] = "virmos",
        ["põe"] = "pomos", ["pôs"] = "pusemos", ["punha"] = "púnhamos", ["ponha"] = "ponhamos", ["pusesse"] = "puséssemos",
        ["lê"] = "lemos", ["leu"] = "lemos", ["sai"] = "saímos", ["saiu"] = "saímos", ["saía"] = "saíamos", ["cai"] = "caímos", ["caiu"] = "caímos",
        ["ri"] = "rimos", ["riu"] = "rimos", ["ouve"] = "ouvimos", ["ouviu"] = "ouvimos", ["ouvia"] = "ouvíamos", ["ouça"] = "ouçamos",
        ["pede"] = "pedimos", ["pediu"] = "pedimos", ["perde"] = "perdemos", ["perdeu"] = "perdemos", ["perca"] = "percamos",
        ["dorme"] = "dormimos", ["dormiu"] = "dormimos", ["sente"] = "sentimos", ["sentiu"] = "sentimos", ["segue"] = "seguimos",
        ["seguiu"] = "seguimos", ["consegue"] = "conseguimos", ["conseguiu"] = "conseguimos", ["conseguia"] = "conseguíamos",
        ["prefere"] = "preferimos", ["preferiu"] = "preferimos", ["mente"] = "mentimos", ["sobe"] = "subimos", ["subiu"] = "subimos",
        ["foge"] = "fugimos", ["fugiu"] = "fugimos", ["serve"] = "servimos", ["veste"] = "vestimos", ["diverte"] = "divertimos", ["divertiu"] = "divertimos",
        ["acha"] = "achamos", ["precisa"] = "precisamos", ["fica"] = "ficamos", ["ficou"] = "ficámos", ["anda"] = "andamos",
        ["conhece"] = "conhecemos", ["conheceu"] = "conhecemos", ["parece"] = "parecemos", ["mora"] = "moramos", ["vive"] = "vivemos",
        ["come"] = "comemos", ["bebe"] = "bebemos", ["escreve"] = "escrevemos", ["entende"] = "entendemos", ["percebe"] = "percebemos",
        ["aprende"] = "aprendemos", ["corre"] = "corremos", ["deve"] = "devemos", ["devia"] = "devíamos", ["deveria"] = "deveríamos",
        ["merece"] = "merecemos", ["esquece"] = "esquecemos", ["recebe"] = "recebemos", ["resolve"] = "resolvemos", ["responde"] = "respondemos",
        ["vende"] = "vendemos", ["abre"] = "abrimos", ["decide"] = "decidimos", ["divide"] = "dividimos", ["assiste"] = "assistimos",
        ["discute"] = "discutimos", ["parte"] = "partimos", ["existe"] = "existimos", ["insiste"] = "insistimos", ["desiste"] = "desistimos",
        ["mexe"] = "mexemos", ["move"] = "movemos", ["sofre"] = "sofremos", ["volta"] = "voltamos", ["acaba"] = "acabamos",
    };

    /// <summary>"a gente faz" → "fazemos", "a gente se divertiu" → "divertimo(s)-nos": 3rd person singular → 1st person plural.</summary>
    public static bool TryThirdToFirstPlural(string word, out string plural)
    {
        plural = word;
        var w = word.ToLowerInvariant();
        string? result = null;
        if (IrregularThirdToFirstPlural.TryGetValue(w, out var irregular))
        {
            result = irregular;
        }
        else if (w.Length >= 4 && w.All(char.IsLetter) && !GuessStopWords.Contains(w) && !AdjectiveLike.IsMatch(w))
        {
            foreach (var (ending, replacement) in new[]
            {
                ("ássemos", "ássemos"), ("aria", "aríamos"), ("eria", "eríamos"), ("iria", "iríamos"), ("ará", "aremos"), ("erá", "eremos"), ("irá", "iremos"),
                ("asse", "ássemos"), ("esse", "êssemos"), ("isse", "íssemos"), ("ava", "ávamos"), ("ia", "íamos"), ("ou", "ámos"), ("eu", "emos"), ("iu", "imos"),
            })
            {
                if (w.EndsWith(ending, StringComparison.Ordinal))
                {
                    result = w[..^ending.Length] + replacement;
                    break;
                }
            }

            if (result == null && (w.EndsWith("ar", StringComparison.Ordinal) || w.EndsWith("er", StringComparison.Ordinal) || w.EndsWith("ir", StringComparison.Ordinal)) && IsInfinitive(w))
            {
                // Future subjunctive / personal infinitive: "se a gente deixar" → "se deixarmos".
                result = w + "mos";
            }
            else if (result == null && ThirdToSecond.ContainsKey(w))
            {
                // Present tense of a known regular verb: -a → -amos; -e is -emos for -er verbs and -imos for -ir verbs.
                if (w.EndsWith('a'))
                {
                    result = w + "mos";
                }
                else if (w.EndsWith('e'))
                {
                    var stem = w[..^1];
                    result = RegularIr.Contains(stem + "ir") ? stem + "imos" : RegularEr.Contains(stem + "er") ? stem + "emos" : null;
                }
            }
            else if (result == null && w.EndsWith('a') && !w.EndsWith("ia", StringComparison.Ordinal))
            {
                result = w + "mos";
            }
        }

        if (result == null)
        {
            return false;
        }

        plural = CaseHelper.MatchCase(word, result);
        return true;
    }

    public static readonly IReadOnlyDictionary<string, string> ThirdToSecond;
    private static readonly HashSet<string> TransitiveForms;
    private static readonly HashSet<string> AllForms;

    static PortugueseVerbs()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        var transitive = new HashSet<string>(StringComparer.Ordinal);
        var all = new HashSet<string>(StringComparer.Ordinal);

        var irregular = new HashSet<string>(StringComparer.Ordinal)
        {
            "ver", "fazer", "trazer", "pôr", "ouvir", "seguir", "perder", "querer", "odiar", "deter", "trair", "substituir",
            "impedir", "despedir", "chantagear",
        };
        var extra = TransitiveInfinitives.Where(v => !irregular.Contains(v) && (v.EndsWith("ar") || v.EndsWith("er") || v.EndsWith("ir")));
        foreach (var verb in RegularAr.Concat(RegularEr).Concat(RegularIr).Concat(extra).Distinct())
        {
            var forms = Conjugate(verb);
            foreach (var (third, second) in forms.ThirdToSecond)
            {
                map.TryAdd(third, second);
            }

            all.UnionWith(forms.ObjectForms);
            if (TransitiveInfinitives.Contains(verb))
            {
                transitive.UnionWith(forms.ObjectForms);
                // "Viste ele?" / "Se deixarem ela" → "Viste-o?" / "Se a deixarem": a "tu" verb cannot have "ele" as its subject.
                transitive.UnionWith(forms.ThirdToSecond.Select(f => f.Second).Where(f => !SecondPersonNouns.Contains(f)));
                transitive.Add(verb + "em");
            }
        }

        foreach (var (third, second) in IrregularThirdToSecond)
        {
            map[third] = second;
            all.Add(third);
        }

        AddDerivedForms(new[] { "man", "de", "con", "ob", "re", "entre" },
            new[] { "tem", "teve", "tinha", "tenha", "tivesse", "tiver", "terá", "teria" }, stressTem: true);
        AddDerivedForms(new[] { "pre", "re", "ante", "entre" },
            new[] { "vê", "viu", "via", "veja", "visse", "vir", "verá", "veria" });
        AddDerivedForms(new[] { "re", "des", "satis" },
            new[] { "faz", "fez", "fazia", "faça", "fizesse", "fizer", "fará", "faria" });

        void AddDerivedForms(string[] prefixes, string[] forms, bool stressTem = false)
        {
            foreach (var prefix in prefixes)
            foreach (var form in forms)
            {
                var third = prefix + (stressTem && form == "tem" ? "tém" : form);
                var second = prefix + (stressTem && form == "tem" ? "téns" : IrregularThirdToSecond[form]);
                map[third] = second;
                all.Add(third);
            }
        }

        transitive.UnionWith(IrregularTransitiveForms);
        all.UnionWith(IrregularTransitiveForms);

        // Words that are much more often something else ("como" = how, "para" = for, "entre" = between...).
        foreach (var homograph in new[] { "como", "para", "entre", "sobre", "casa", "contra", "cara", "conta", "era", "ia", "fora", "mas", "segundo", "acho" })
        {
            transitive.Remove(homograph);
        }

        ThirdToSecond = map;
        TransitiveForms = transitive;
        AllForms = all;
    }

    private sealed record VerbForms(List<(string Third, string Second)> ThirdToSecond, List<string> ObjectForms);

    private static VerbForms Conjugate(string infinitive)
    {
        var stem = infinitive.Substring(0, infinitive.Length - 2);
        var kind = infinitive[^2];
        var pairs = new List<(string, string)>();
        var forms = new List<string> { infinitive };

        // Spelling changes before e (ar verbs) and before a/o (er/ir verbs).
        string StemBeforeE() =>
            stem.EndsWith("c", StringComparison.Ordinal) ? stem[..^1] + "qu" :
            stem.EndsWith("g", StringComparison.Ordinal) ? stem + "u" :
            stem.EndsWith("ç", StringComparison.Ordinal) ? stem[..^1] + "c" : stem;

        string StemBeforeAo() =>
            stem.EndsWith("c", StringComparison.Ordinal) ? stem[..^1] + "ç" :
            stem.EndsWith("g", StringComparison.Ordinal) ? stem[..^1] + "j" : stem;

        if (kind == 'a')
        {
            var subj = StemBeforeE();
            pairs.Add((stem + "a", stem + "as"));
            pairs.Add((stem + "ou", stem + "aste"));
            pairs.Add((stem + "ava", stem + "avas"));
            pairs.Add((subj + "e", subj + "es"));
            pairs.Add((stem + "asse", stem + "asses"));
            forms.AddRange(new[] { stem + "o", stem + "a", stem + "amos", stem + "am", subj + "ei", stem + "ou", stem + "aram", stem + "ava", stem + "avam", subj + "e", subj + "em" });
        }
        else
        {
            var subj = StemBeforeAo();
            var vowel = kind == 'e' ? "e" : "i";
            pairs.Add((stem + "e", stem + "es"));
            pairs.Add((stem + (kind == 'e' ? "eu" : "iu"), stem + (kind == 'e' ? "este" : "iste")));
            pairs.Add((stem + "ia", stem + "ias"));
            pairs.Add((subj + "a", subj + "as"));
            pairs.Add((stem + vowel + "sse", stem + vowel + "sses"));
            forms.AddRange(new[] { subj + "o", stem + "e", stem + vowel + "mos", stem + "em", stem + "i", stem + (kind == 'e' ? "eu" : "iu"), stem + vowel + "ram", stem + "ia", stem + "iam", subj + "a", subj + "am" });
        }

        pairs.Add((infinitive + "á", infinitive + "ás"));
        pairs.Add((infinitive + "ia", infinitive + "ias"));
        pairs.Add((infinitive, infinitive + "es"));
        return new VerbForms(pairs, forms);
    }

    public static bool TryThirdToSecond(string word, out string second)
    {
        if (ThirdToSecond.TryGetValue(word.ToLowerInvariant(), out var lower))
        {
            second = CaseHelper.MatchCase(word, lower);
            return true;
        }

        second = word;
        return false;
    }

    public static bool IsSecondPersonForm(string word) =>
        ThirdToSecond.Values.Contains(word.ToLowerInvariant(), StringComparer.Ordinal);

    private static readonly HashSet<string> GuessStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "mesma", "mesmo", "sozinha", "sozinho", "própria", "inteira", "toda", "nada", "nunca", "agora", "ainda", "hoje",
        "sempre", "aqui", "longe", "cedo", "tarde", "antes", "também", "nessa", "nesse", "dessa", "desse", "essa", "esse",
        "esta", "este", "aquela", "outra", "alguma", "nenhuma", "muita", "pouca", "demais", "depois", "logo", "onde",
        "porque", "enquanto", "embora", "como", "para", "pela", "sobre", "entre", "contra", "desde", "perante", "numa",
        "duma", "uma", "minha", "sua", "tua", "nossa", "dela", "dele", "cara", "besta", "idiota", "louca", "linda", "gata",
        "burra", "nojenta", "bonita", "chata", "doente", "grande", "pobre", "estou", "sou", "vou", "dou", "meu", "seu",
        "teu", "quase", "talvez", "lá", "cá", "aí", "ali", "nem", "agora", "mais", "menos", "acima", "abaixo", "fora",
        "dentro", "perto", "junto", "assim", "então", "contigo", "comigo", "inclusive", "primeira", "única", "sabia",
    };

    private static readonly Regex AdjectiveLike = new(@"(?:ada|ida|ado|ido|osa|oso|ista|ona|inha|inho|ente|ante|ável|ível|esa|eira|eiro)s?$", RegexOptions.CultureInvariant);

    /// <summary>
    /// Regular "você" forms not in the tables (você almoçou → almoçaste, você trabalhava → trabalhavas).
    /// The source and result must be recognised by the European Portuguese dictionary.
    /// </summary>
    public static bool TryGuessThirdToSecond(string word, out string second)
    {
        second = word;
        var w = word.ToLowerInvariant();
        if (w.Length < 4 || GuessStopWords.Contains(w) || AdjectiveLike.IsMatch(w) || !w.All(char.IsLetter))
        {
            return false;
        }

        string? result = null;
        if (w.EndsWith("ou", StringComparison.Ordinal))
        {
            result = w[..^2] + "aste";
        }
        else if (w.EndsWith("eu", StringComparison.Ordinal))
        {
            result = w[..^2] + "este";
        }
        else if (w.EndsWith("iu", StringComparison.Ordinal))
        {
            result = w[..^2] + "iste";
        }
        else if (w.EndsWith("ará", StringComparison.Ordinal) || w.EndsWith("erá", StringComparison.Ordinal) || w.EndsWith("irá", StringComparison.Ordinal))
        {
            result = w + "s";
        }
        else if (w.EndsWith('a') || w.EndsWith('e'))
        {
            result = w + "s";
        }
        else if (IsInfinitive(w))
        {
            result = w + "es";
        }

        if (result == null || !NaturaDictionary.Load(true).Check(w) || !NaturaDictionary.Load(true).Check(result))
        {
            return false;
        }

        second = CaseHelper.MatchCase(word, result);
        return true;
    }

    public static bool IsTransitiveForm(string word) => TransitiveForms.Contains(word.ToLowerInvariant());

    public static bool IsKnownVerbForm(string word) => AllForms.Contains(word.ToLowerInvariant());

    /// <summary>Looks like an infinitive: ends in -ar/-er/-ir/-por/-pôr and is not a common noun/adjective.</summary>
    public static bool IsInfinitive(string word)
    {
        var w = word.ToLowerInvariant();
        if (w.Length < 2 || InfinitiveStopWords.Contains(w))
        {
            return w is "poder" or "dever" or "jantar" or "olhar";
        }

        if (w == "pôr" || w.EndsWith("por", StringComparison.Ordinal) && w.Length > 4)
        {
            return true;
        }

        return w.EndsWith("ar", StringComparison.Ordinal) || w.EndsWith("er", StringComparison.Ordinal) || w.EndsWith("ir", StringComparison.Ordinal);
    }

    /// <summary>Future and conditional forms need mesoclisis (dir-me-á) - those are left alone.</summary>
    public static bool IsFutureOrConditional(string word)
    {
        var w = word.ToLowerInvariant();
        foreach (var ending in new[] { "íamos", "emos", "ias", "iam", "ia", "ei", "ás", "á", "ão" })
        {
            if (w.Length > ending.Length + 2 && w.EndsWith(ending, StringComparison.Ordinal))
            {
                var root = w[..^ending.Length];
                if (root.EndsWith("ar") || root.EndsWith("er") || root.EndsWith("ir") || root.EndsWith("or") ||
                    root is "dir" or "far" or "trar")
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Verbs that take a direct object in the future/conditional but are not conjugated by the regular tables.
    private static readonly HashSet<string> ExtraMesoclisisInfinitives = new(StringComparer.Ordinal)
    {
        "manter", "dizer", "contar", "informar", "pagar", "contactar", "telefonar", "devolver", "mandar", "enviar", "dar",
        "levar", "buscar", "ensinar", "lembrar", "agradecer", "explicar", "mostrar", "arranjar", "apanhar", "seguir",
    };

    // Only real infinitives: "queriam" (imperfect of querer) must not split into "quer" + "iam".
    private static readonly HashSet<string> Infinitives = new(
        RegularAr.Concat(RegularEr).Concat(RegularIr).Concat(TransitiveInfinitives).Concat(ExtraMesoclisisInfinitives)
            .Concat(new[] { "ter", "ver", "ser", "pôr", "por", "vir", "ir", "sair", "cair", "trair", "ouvir", "pedir", "sentir", "dormir", "seguir",
                "tornar", "queixar", "esquecer" }),
        StringComparer.Ordinal);

    public static bool IsKnownInfinitive(string word) => Infinitives.Contains(word.ToLowerInvariant());

    public static bool TryGetFirstPersonPastInfinitive(string word, out string infinitive)
    {
        var lower = word.ToLowerInvariant();
        var irregular = lower switch
        {
            "dei" => "dar",
            "disse" => "dizer",
            "fiz" => "fazer",
            "li" => "ler",
            "pude" => "poder",
            "pus" => "pôr",
            "quis" => "querer",
            "soube" => "saber",
            "trouxe" => "trazer",
            "tive" => "ter",
            "vi" => "ver",
            "vim" => "vir",
            _ => null,
        };
        if (irregular != null)
        {
            infinitive = CaseHelper.MatchCase(word, irregular);
            return true;
        }

        string? found = null;
        foreach (var verb in RegularAr.Concat(RegularEr).Concat(RegularIr))
        {
            var stem = verb[..^2];
            var preterite = verb.EndsWith("ar", StringComparison.Ordinal)
                ? (stem.EndsWith('c') ? stem[..^1] + "qu" : stem.EndsWith('g') ? stem + "u" : stem.EndsWith('ç') ? stem[..^1] + "c" : stem) + "ei"
                : stem + "i";
            var imperfect = verb.EndsWith("ar", StringComparison.Ordinal) ? stem + "ava" : stem + "ia";
            if (lower != preterite && lower != imperfect)
            {
                continue;
            }

            if (found != null && found != verb)
            {
                infinitive = word;
                return false;
            }

            found = verb;
        }

        if (found == null)
        {
            infinitive = word;
            return false;
        }

        infinitive = CaseHelper.MatchCase(word, found);
        return true;
    }

    internal static string PersonalInfinitive(string infinitive, string ending)
    {
        var lower = infinitive.ToLowerInvariant();
        var stem = lower == "pôr" ? "por" : lower;
        if (ending is "es" or "em" && (lower.EndsWith("air", StringComparison.Ordinal) ||
            lower.EndsWith("uir", StringComparison.Ordinal) && !lower.EndsWith("guir", StringComparison.Ordinal)))
            stem = stem[..^2] + "ír";
        return CaseHelper.MatchCase(infinitive, stem + ending);
    }

    internal static bool TryRestoreInfinitiveEnding(string word, out string infinitive)
    {
        infinitive = word;
        var lower = word.ToLowerInvariant();
        if (lower.Length < 3 || !"aei".Contains(lower[^1]) || lower == "que" || GuessStopWords.Contains(lower) ||
            (char.IsUpper(word[0]) && !CaseHelper.IsAllUpper(word)))
        {
            return false;
        }

        var candidate = lower + "r";
        if (!IsInfinitive(candidate) ||
            (!IsKnownInfinitive(candidate) && !NaturaDictionary.Load(true).Check(candidate)))
        {
            return false;
        }

        infinitive = CaseHelper.MatchCase(word, candidate);
        return true;
    }

    /// <summary>"manterei" → ("manter", "ei"), "diria" → ("dir", "ia"); false when the word is not a known future/conditional.</summary>
    public static bool TrySplitFuture(string word, out string stem, out string ending, bool requireTransitive)
    {
        stem = ending = string.Empty;
        var w = word.ToLowerInvariant();
        foreach (var e in new[] { "íamos", "íeis", "emos", "eis", "ias", "iam", "ia", "ei", "ás", "á", "ão" })
        {
            if (w.Length <= e.Length + 2 || !w.EndsWith(e, StringComparison.Ordinal))
            {
                continue;
            }

            var root = w[..^e.Length];
            var known = root is "dir" or "far" or "trar" ||
                        TransitiveInfinitives.Contains(root) || ExtraMesoclisisInfinitives.Contains(root) ||
                        (!requireTransitive && Infinitives.Contains(root));
            if (!known)
            {
                continue;
            }

            stem = word[..root.Length];
            ending = word[root.Length..];
            return true;
        }

        return false;
    }

    /// <summary>Mesoclisis: manterei + te → manter-te-ei, farei + o → fá-lo-ei, diria + me → dir-me-ia.</summary>
    public static string? AttachMesoclitic(string verb, string pronoun, bool requireTransitive = true)
    {
        if (!TrySplitFuture(verb, out var stem, out var ending, requireTransitive))
        {
            return null;
        }

        var upper = CaseHelper.IsAllUpper(verb);
        var p = pronoun.ToLowerInvariant();
        if (p is "lo" or "la" or "los" or "las")
        {
            p = p.Substring(1);
        }

        var result = p is "o" or "a" or "os" or "as"
            ? AccentFinalVowel(stem[..^1]) + "-l" + p + "-" + ending
            : stem + "-" + p + "-" + ending;
        return upper ? result.ToUpperInvariant() : result;
    }

    /// <summary>
    /// Verb + hyphen + pronoun with the European sound changes:
    /// fazer + o → fazê-lo, fez + o → fê-lo, vimos + o → vimo-lo, amam + o → amam-no, vemos + nos → vemo-nos.
    /// </summary>
    public static string AttachEnclitic(string verb, string pronoun)
    {
        var upper = CaseHelper.IsAllUpper(verb);
        var p = pronoun.ToLowerInvariant();
        if (p is "lo" or "la" or "los" or "las")
        {
            p = p.Substring(1);
        }

        string result;
        var lower = verb.ToLowerInvariant();
        if (p is "o" or "a" or "os" or "as")
        {
            var last = lower[^1];
            if (last is 'r' or 'z' or 's')
            {
                var head = verb[..^1];
                if (last is 'r' or 'z')
                {
                    head = AccentFinalVowel(head);
                }

                result = head + "-l" + p;
            }
            else if (lower.EndsWith('m') || lower.EndsWith("ão") || lower.EndsWith("õe") || lower.EndsWith("ãe"))
            {
                result = verb + "-n" + p;
            }
            else
            {
                result = verb + "-" + p;
            }
        }
        else if (p == "nos" && lower.EndsWith("mos", StringComparison.Ordinal))
        {
            result = verb[..^1] + "-" + p;
        }
        else
        {
            result = verb + "-" + p;
        }

        return upper ? result.ToUpperInvariant() : result;
    }

    private static string AccentFinalVowel(string head)
    {
        if (head.Length == 0)
        {
            return head;
        }

        var c = head[^1];
        var accented = c switch
        {
            'a' => 'á', 'A' => 'Á',
            'e' => 'ê', 'E' => 'Ê',
            'o' => 'ô', 'O' => 'Ô',
            _ => c,
        };
        return head[..^1] + accented;
    }
}

internal static class CaseHelper
{
    public static bool IsAllUpper(string s)
    {
        var letters = 0;
        foreach (var c in s)
        {
            if (char.IsLetter(c))
            {
                letters++;
                if (!char.IsUpper(c))
                {
                    return false;
                }
            }
        }

        return letters > 1;
    }

    /// <summary>Gives <paramref name="replacement"/> the casing of <paramref name="source"/> (lower, Title or UPPER).</summary>
    public static string MatchCase(string source, string replacement)
    {
        if (replacement.Length == 0 || source.Length == 0)
        {
            return replacement;
        }

        if (IsAllUpper(source))
        {
            return replacement.ToUpperInvariant();
        }

        if (char.IsUpper(source[0]))
        {
            return char.ToUpperInvariant(replacement[0]) + replacement.Substring(1);
        }

        return replacement;
    }

    public static string Capitalize(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
}
