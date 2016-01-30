using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core
{
    public static class LanguageAutoDetect
    {

        private static int GetCount(string text, params string[] words)
        {
            var options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
            var pattern = "\\b(" + string.Join("|", words) + ")\\b";
            return Regex.Matches(text, pattern, options).Count;
        }

        private static int GetCountContains(string text, params string[] words)
        {
            int count = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var regEx = new Regex(words[i]);
                count += regEx.Matches(text).Count;
            }
            return count;
        }

        public static string AutoDetectGoogleLanguage(Encoding encoding)
        {
            switch (encoding.CodePage)
            {
                case 860:
                    return "pt"; // Portuguese
                case 28599:
                case 1254:
                    return "tr"; // Turkish
                case 28598:
                case 1255:
                    return "he"; // Hebrew
                case 28596:
                case 1256:
                    return "ar"; // Arabic
                case 1258:
                    return "vi"; // Vietnamese
                case 949:
                case 1361:
                case 20949:
                case 51949:
                case 50225:
                    return "ko"; // Korean
                case 1253:
                case 28597:
                    return "el"; // Greek
                case 50220:
                case 50221:
                case 50222:
                case 51932:
                case 20932:
                case 10001:
                    return "ja"; // Japanese
                case 20000:
                case 20002:
                case 20936:
                case 950:
                case 52936:
                case 54936:
                case 51936:
                    return "zh"; // Chinese
                default:
                    return null;
            }
        }

        private static readonly string[] AutoDetectWordsEnglish = { "we", "are", "and", "your?", "what" };
        private static readonly string[] AutoDetectWordsDanish = { "vi", "han", "og", "jeg", "var", "men", "gider", "bliver", "virkelig", "kommer", "tilbage", "Hej" };
        private static readonly string[] AutoDetectWordsNorwegian = { "vi", "er", "og", "jeg", "var", "men" };
        private static readonly string[] AutoDetectWordsSwedish = { "vi", "är", "och", "Jag", "inte", "för" };
        private static readonly string[] AutoDetectWordsSpanish = { "qué", "eso", "muy", "estoy?", "ahora", "hay", "tú", "así", "cuando", "cómo", "él", "sólo", "quiero", "gracias", "puedo", "bueno", "soy", "hacer", "fue", "eres", "usted", "tienes", "puede",
                                                                    "[Ss]eñor", "ese", "voy", "quién", "creo", "hola", "dónde", "sus", "verdad", "quieres", "mucho", "entonces", "estaba", "tiempo", "esa", "mejor", "hombre", "hace", "dios", "también", "están",
                                                                    "siempre", "hasta", "ahí", "siento", "puedes" };
        private static readonly string[] AutoDetectWordsItalian = { "Buongiorno", "Grazie", "Cosa", "quest[ao]", "quell[ao]", "tutt[io]", "[st]uo", "qualcosa", "ancora", "sono", "bene", "più", "andare", "essere", "venire", "abbiamo", "andiamo", "ragazzi",
                                                                    "signore", "numero", "giorno", "propriamente", "sensitivo", "negativo", "davvero", "faccio", "voglio", "vuole", "perché", "allora", "niente", "anche", "stai", "detto", "fatto", "hanno",
                                                                    "molto", "stato", "siamo", "così", "vuoi", "noi", "vero", "loro", "fare", "gli", "due" };
        private static readonly string[] AutoDetectWordsFrench = { "pas", "[vn]ous", "ça", "une", "pour", "[mt]oi", "dans", "elle", "tout", "plus", "[bmt]on", "suis", "avec", "oui", "fait", "ils", "être", "faire", "comme", "était", "quoi", "ici", "veux",
                                                                   "rien", "dit", "où", "votre", "pourquoi", "sont", "cette", "peux", "alors", "comment", "avez", "très", "même", "merci", "ont", "aussi", "chose", "voir", "allez", "tous", "ces", "deux" };
        private static readonly string[] AutoDetectWordsPortuguese = { "[Nn]ão", "[Ee]ntão", "uma", "ele", "bem", "isso", "você", "sim", "meu", "muito", "estou", "ela", "fazer", "tem", "já", "minha", "tudo", "só", "tenho", "agora", "vou", "seu", "quem",
                                                                       "há", "lhe", "quero", "nós", "coisa", "são", "ter", "dizer", "eles", "pode", "bom", "mesmo", "mim", "estava", "assim", "estão", "até", "quer", "temos", "acho", "obrigado", "também",
                                                                       "tens", "deus", "quê", "ainda", "noite" };
        private static readonly string[] AutoDetectWordsGerman = { "und", "auch", "sich", "bin", "hast", "möchte" };
        private static readonly string[] AutoDetectWordsDutch = { "van", "een", "[Hh]et", "m(ij|ĳ)", "z(ij|ĳ)n" };
        private static readonly string[] AutoDetectWordsPolish = { "Czy", "ale", "ty", "siê", "jest", "mnie" };
        private static readonly string[] AutoDetectWordsGreek = { "μου", "[Εε]ίναι", "αυτό", "Τόμπυ", "καλά", "Ενταξει", "πρεπει", "Λοιπον", "τιποτα", "ξερεις" };
        private static readonly string[] AutoDetectWordsRussian = { "[Ээч]?то", "[Нн]е", "[ТтМмбв]ы", "Да", "[Нн]ет", "Он", "его", "тебя", "как", "меня", "Но", "всё", "мне", "вас", "знаю", "ещё", "за", "нас", "чтобы", "был" };
        private static readonly string[] AutoDetectWordsUkrainian = { "[Нн]і", "[Пп]ривіт", "[Цц]е", "[Щщ]о", "[Йй]ого", "[Вв]ін", "[Яя]к", "[Гг]аразд", "[Яя]кщо", "[Мм]ені", "[Тт]вій", "[Її]х", "[Вв]ітаю", "[Дд]якую", "вже", "було", "був", "цього",
                                                                      "нічого", "немає", "може", "знову", "бо", "щось", "щоб", "цим", "тобі", "хотів", "твоїх", "мої", "мій", "має", "їм", "йому", "дуже" };
        private static readonly string[] AutoDetectWordsBulgarian = { "[Кк]акво", "тук", "може", "Как", "Ваше" };
        private static readonly string[] AutoDetectWordsArabic = { "من", "هل", "لا", "فى", "لقد", "ما" };
        private static readonly string[] AutoDetectWordsHebrew = { "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב" };
        private static readonly string[] AutoDetectWordsVietnamese = { "không", "[Tt]ôi", "anh", "đó", "ông" };
        private static readonly string[] AutoDetectWordsHungarian = { "hogy", "lesz", "tudom", "vagy", "mondtam", "még" };
        private static readonly string[] AutoDetectWordsTurkish = { "için", "Tamam", "Hayır", "benim", "daha", "deðil", "önce", "lazým", "çalýþýyor", "burada", "efendim" };
        private static readonly string[] AutoDetectWordsCroatianAndSerbian = { "sam", "ali", "nije", "samo", "ovo", "kako", "dobro", "sve", "tako", "će", "mogu", "ću", "zašto", "nešto", "za" };
        private static readonly string[] AutoDetectWordsCroatian = { "što", "ovdje", "gdje", "kamo", "tko", "prije", "uvijek", "vrijeme", "vidjeti", "netko",
                                                                     "vidio", "nitko", "bok", "lijepo", "oprosti", "htio", "mjesto", "oprostite", "čovjek", "dolje",
                                                                     "čovječe", "dvije", "dijete", "dio", "poslije", "događa", "vjerovati", "vjerojatno", "vjerujem", "točno",
                                                                     "razumijem", "vidjela", "cijeli", "svijet", "obitelj", "volio", "sretan", "dovraga", "svijetu", "htjela",
                                                                     "vidjeli", "negdje", "želio", "ponovno", "djevojka", "umrijeti", "čovjeka", "mjesta", "djeca", "osjećam",
                                                                     "uopće", "djecu", "naprijed", "obitelji", "doista", "mjestu", "lijepa", "također", "riječ", "tijelo" };
        private static readonly string[] AutoDetectWordsSerbian = { "šta", "ovde", "gde", "ko", "pre", "uvek", "vreme", "videti", "neko",
                                                                    "video", "niko", "ćao", "lepo", "izvini", "hteo", "mesto", "izvinite", "čovek", "dole",
                                                                    "čoveče", "dve", "dete", "deo", "posle", "dešava", "verovati", "verovatno", "verujem", "tačno",
                                                                    "razumem", "videla", "ceo", "svet", "porodica", "voleo", "srećan", "dođavola", "svetu", "htela",
                                                                    "videli", "negde", "želeo", "ponovo", "devojka", "umreti", "čoveka", "mesta", "deca", "osećam",
                                                                    "uopšte", "decu", "napred", "porodicu", "zaista", "mestu", "lepa", "takođe", "reč", "telo" };
        private static readonly string[] AutoDetectWordsSerbianCyrillic = { "сам", "али", "није", "само", "ово", "како", "добро", "све", "тако", "ће", "могу", "ћу", "зашто", "нешто", "за", "шта", "овде" };
        private static readonly string[] AutoDetectWordsIndonesian = { "yang", "tahu", "bisa", "akan", "tahun", "tapi", "dengan", "untuk", "rumah", "dalam", "sudah", "bertemu" };
        private static readonly string[] AutoDetectWordsThai = { "โอ", "โรเบิร์ต", "วิตตอเรีย", "ดร", "คุณตำรวจ", "ราเชล", "ไม่", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์" };
        private static readonly string[] AutoDetectWordsKorean = { "그리고", "아니야", "하지만", "말이야", "그들은", "우리가" };
        private static readonly string[] AutoDetectWordsFinnish = { "että", "kuin", "minä", "mitään", "Mutta", "siitä", "täällä", "poika", "Kiitos", "enää", "vielä", "tässä" };
        private static readonly string[] AutoDetectWordsRomanian1 = { "pentru", "oamenii", "decât", "[Vv]reau", "[Ss]înt", "Asteaptã", "Fãrã", "aici", "domnule", "trãiascã", "niciodatã", "înseamnã", "vorbesti", "fãcut", "spune" };
        private static readonly string[] AutoDetectWordsRomanian2 = { "pentru", "oamenii", "decat", "[Tt]rebuie", "[Aa]cum", "Poate", "vrea", "soare", "nevoie", "daca", "echilibrul", "vorbesti", "zeului", "atunci", "memoria", "soarele" };

        public static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            int count = GetCount(text, AutoDetectWordsEnglish);
            if (count > bestCount)
                return "en";

            count = GetCount(text, AutoDetectWordsDanish);
            if (count > bestCount)
            {
                int norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                int dutchCount = GetCount(text, AutoDetectWordsDutch);
                if (norwegianCount < 2 && dutchCount < count)
                    return "da";
            }

            count = GetCount(text, AutoDetectWordsNorwegian);
            if (count > bestCount)
            {
                int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                int dutchCount = GetCount(text, AutoDetectWordsDutch);
                if (danishCount < 2 && dutchCount < count)
                    return "no";
            }

            count = GetCount(text, AutoDetectWordsSwedish);
            if (count > bestCount)
                return "sv";

            count = GetCount(text, AutoDetectWordsSpanish);
            if (count > bestCount)
            {
                int frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
                int portugueseCount = GetCount(text, "[NnCc]ão", "Então", "h?ouve", "pessoal", "rapariga", "tivesse", "fizeste",
                                                     "jantar", "conheço", "atenção", "foste", "milhões", "devias", "ganhar", "raios"); // not spanish words
                if (frenchCount < 2 && portugueseCount < 2)
                    return "es";
            }

            count = GetCount(text, AutoDetectWordsItalian);
            if (count > bestCount)
            {
                int frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not italian words
                if (frenchCount < 2)
                    return "it";
            }

            count = GetCount(text, AutoDetectWordsFrench);
            if (count > bestCount)
            {
                int romanianCount = GetCount(text, "[Vv]reau", "[Ss]înt", "[Aa]cum", "pentru", "domnule", "aici");
                if (romanianCount < 5)
                    return "fr";
            }

            count = GetCount(text, AutoDetectWordsPortuguese);
            if (count > bestCount)
                return "pt"; // Portuguese

            count = GetCount(text, AutoDetectWordsGerman);
            if (count > bestCount)
                return "de";

            count = GetCount(text, AutoDetectWordsDutch);
            if (count > bestCount)
                return "nl";

            count = GetCount(text, AutoDetectWordsPolish);
            if (count > bestCount)
                return "pl";

            count = GetCount(text, AutoDetectWordsGreek);
            if (count > bestCount)
                return "el"; // Greek

            count = GetCount(text, AutoDetectWordsRussian);
            if (count > bestCount)
                return "ru"; // Russian

            count = GetCount(text, AutoDetectWordsUkrainian);
            if (count > bestCount)
                return "uk"; // Ukrainian

            count = GetCount(text, AutoDetectWordsBulgarian);
            if (count > bestCount)
                return "bg"; // Bulgarian

            count = GetCount(text, AutoDetectWordsArabic);
            if (count > bestCount)
            {
                int hebrewCount = GetCount(text, AutoDetectWordsHebrew);
                if (hebrewCount < count)
                    return "ar"; // Arabic
            }

            count = GetCount(text, AutoDetectWordsHebrew);
            if (count > bestCount)
                return "he"; // Hebrew

            count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
            if (count > bestCount)
            {
                int croatianCount = GetCount(text, AutoDetectWordsCroatian);
                int serbianCount = GetCount(text, AutoDetectWordsSerbian);
                if (croatianCount > serbianCount)
                    return "hr"; // Croatian
                return "sr"; // Serbian
            }

            count = GetCount(text, AutoDetectWordsVietnamese);
            if (count > bestCount)
                return "vi"; // Vietnamese

            count = GetCount(text, AutoDetectWordsHungarian);
            if (count > bestCount)
                return "hu"; // Hungarian

            count = GetCount(text, AutoDetectWordsTurkish);
            if (count > bestCount)
                return "tr"; // Turkish

            count = GetCount(text, AutoDetectWordsIndonesian);
            if (count > bestCount)
                return "id"; // Indonesian

            count = GetCount(text, AutoDetectWordsThai);
            if (count > 10 || count > bestCount)
                return "th"; // Thai

            count = GetCount(text, AutoDetectWordsKorean);
            if (count > 10 || count > bestCount)
                return "ko"; // Korean

            count = GetCount(text, AutoDetectWordsFinnish);
            if (count > bestCount)
                return "fi"; // Finnish

            count = GetCount(text, AutoDetectWordsRomanian1);
            if (count <= bestCount)
                count = GetCount(text, AutoDetectWordsRomanian2);
            if (count > bestCount)
                return "ro"; // Romanian

            count = GetCountContains(text, "シ", "ュ", "シン", "シ", "ン", "ユ");
            count += GetCountContains(text, "イ", "ン", "チ", "ェ", "ク", "ハ");
            count += GetCountContains(text, "シ", "ュ", "う", "シ", "ン", "サ");
            count += GetCountContains(text, "シ", "ュ", "シ", "ン", "だ", "う");
            if (count > bestCount * 2)
                return "ja"; // Japanese - not tested...

            count = GetCountContains(text, "是", "是早", "吧", "的", "爱", "上好");
            count += GetCountContains(text, "的", "啊", "好", "好", "亲", "的");
            count += GetCountContains(text, "谢", "走", "吧", "晚", "上", "好");
            count += GetCountContains(text, "来", "卡", "拉", "吐", "滚", "他");
            if (count > bestCount * 2)
                return "zh"; // Chinese (simplified) - not tested...

            return string.Empty;
        }

    }
}
