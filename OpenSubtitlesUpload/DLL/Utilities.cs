using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic
{
    public static class Utilities
    {
        private static int GetCount(string text,
                            string word1,
                            string word2,
                            string word3,
                            string word4,
                            string word5,
                            string word6)
        {
            var regEx1 = new Regex("\\b" + word1 + "\\b");
            var regEx2 = new Regex("\\b" + word2 + "\\b");
            var regEx3 = new Regex("\\b" + word3 + "\\b");
            var regEx4 = new Regex("\\b" + word4 + "\\b");
            var regEx5 = new Regex("\\b" + word5 + "\\b");
            var regEx6 = new Regex("\\b" + word6 + "\\b");
            int count = regEx1.Matches(text).Count;
            count += regEx2.Matches(text).Count;
            count += regEx3.Matches(text).Count;
            count += regEx4.Matches(text).Count;
            count += regEx5.Matches(text).Count;
            count += regEx6.Matches(text).Count;
            return count;
        }

        public static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            int count = GetCount(text, "we", "are", "and", "you", "your", "what");
            if (count > bestCount)
                return "en";

            count = GetCount(text, "vi", "er", "og", "jeg", "var", "men");
            if (count > bestCount)
            {
                int norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                if (norwegianCount < 2)
                    return "da";
            }

            count = GetCount(text, "vi", "er", "og", "jeg", "var", "men");
            if (count > bestCount)
            {
                int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                if (danishCount < 2)
                    return "no";
            }

            count = GetCount(text, "vi", "är", "och", "Jag", "inte", "för");
            if (count > bestCount)
                return "sv";

            count = GetCount(text, "el", "bien", "Vamos", "Hola", "casa", "con");
            if (count > bestCount)
            {
                int frenchWords = GetCount(text, "Cest", "cest", "pas", "vous", "pour", "suis"); // not spanish words
                if (frenchWords < 2)
                    return "es";
            }

            count = GetCount(text, "un", "vous", "avec", "pas", "ce", "une");
            if (count > bestCount)
            {
                int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                int italianWords = GetCount(text, "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo");
                int romanianWords = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau");
                if (spanishWords < 2 && italianWords < 2 && romanianWords < 5)
                    return "fr";
            }

            count = GetCount(text, "und", "auch", "sich", "bin", "hast", "möchte");
            if (count > bestCount)
                return "de";

            count = GetCount(text, "van", "het", "een", "Het", "mij", "zijn");
            if (count > bestCount)
                return "nl";

            count = GetCount(text, "Czy", "ale", "ty", "siê", "jest", "mnie");
            if (count > bestCount)
                return "pl";

            count = GetCount(text, "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo");
            if (count > bestCount)
            {
                int frenchWords = GetCount(text, "Cest", "cest", "pas", "vous", "pour", "suis"); // not spanish words
                int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                if (frenchWords < 2 && spanishWords < 2)
                    return "it";
            }

            count = GetCount(text, "não", "Não", "Estás", "Então", "isso", "com");
            if (count > bestCount)
                return "pt"; // Portuguese

            count = GetCount(text, "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά") +
                    GetCount(text, "Ενταξει", "Ενταξει", "πρεπει", "Λοιπον", "τιποτα", "ξερεις");
            if (count > bestCount)
                return "el"; // Greek

            count = GetCount(text, "все", "это", "как", "Воробей", "сюда", "Давай");
            if (count > bestCount)
                return "ru"; // Russian

            count = GetCount(text, "Какво", "тук", "може", "Как", "Ваше", "какво");
            if (count > bestCount)
                return "bg"; // Bulgarian

            count = GetCount(text, "sam", "öto", "äto", "ovo", "vas", "što");
            if (count > bestCount && GetCount(text, "htjeti ", "htjeti ", "htjeti ", "htjeti ", "htjeti ", "htjeti ") > 0)
                return "hr"; // Croatia

            count = GetCount(text, "من", "هل", "لا", "فى", "لقد", "ما");
            if (count > bestCount)
            {
                if (GetCount(text, "אולי", "אולי", "אולי", "אולי", "טוב", "טוב") > 10)
                    return "he";

                int countRo = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau") +
                                   GetCount(text, "trãiascã", "niciodatã", "înseamnã", "vorbesti", "oamenii", "Asteaptã") +
                                   GetCount(text, "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
                if (countRo > count)
                    return "ro"; // Romanian

                count = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie") +
                        GetCount(text, "nevoie", "decat", "echilibrul", "vorbesti", "oamenii", "zeului") +
                        GetCount(text, "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");
                if (countRo > count)
                    return "ro"; // Romanian

                return "ar"; // Arabic
            }

            count = GetCount(text, "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב");
            if (count > bestCount)
                return "he"; // Hebrew

            count = GetCount(text, "sam", "što", "nije", "Šta", "ovde", "za");
            if (count > bestCount)
                return "sr"; // Serbian

            count = GetCount(text, "không", "tôi", "anh", "đó", "Tôi", "ông");
            if (count > bestCount)
                return "vi"; // Vietnamese

            count = GetCount(text, "hogy", "lesz", "tudom", "vagy", "mondtam", "még");
            if (count > bestCount)
                return "hu"; // Hungarian

            count = GetCount(text, "için", "Tamam", "Hayır", "benim", "daha", "deðil") + GetCount(text, "önce", "lazým", "benim", "çalýþýyor", "burada", "efendim");
            if (count > bestCount)
                return "tr"; // Turkish

            count = GetCount(text, "yang", "tahu", "bisa", "akan", "tahun", "tapi") + GetCount(text, "dengan", "untuk", "rumah", "dalam", "sudah", "bertemu");
            if (count > bestCount)
                return "id"; // Indonesian

            count = GetCount(text, "โอ", "โรเบิร์ต", "วิตตอเรีย", "ดร", "คุณตำรวจ", "ราเชล") + GetCount(text, "ไม่", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์");
            if (count > 10 || count > bestCount)
                return "th"; // Thai

            count = GetCount(text, "그리고", "아니야", "하지만", "말이야", "그들은", "우리가");
            if (count > 10 || count > bestCount)
                return "ko"; // Korean

            count = GetCount(text, "että", "kuin", "minä", "mitään", "Mutta", "siitä") + GetCount(text, "täällä", "poika", "Kiitos", "enää", "vielä", "tässä");
            if (count > bestCount)
                return "fi"; // Finnish

            count = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau") +
                    GetCount(text, "trãiascã", "niciodatã", "înseamnã", "vorbesti", "oamenii", "Asteaptã") +
                    GetCount(text, "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
            if (count > bestCount)
                return "ro"; // Romanian

            count = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie") +
                    GetCount(text, "nevoie", "decat", "echilibrul", "vorbesti", "oamenii", "zeului") +
                    GetCount(text, "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");
            if (count > bestCount)
                return "ro"; // Romanian

            count = GetCountContains(text, "シ", "ュ", "シン", "シ", "ン", "ユ");
            count += GetCountContains(text, "イ", "ン", "チ", "ェ", "ク", "ハ");
            count += GetCountContains(text, "シ", "ュ", "う", "シ", "ン", "サ");
            count += GetCountContains(text, "シ", "ュ", "シ", "ン", "だ", "う");
            if (count > bestCount * 2)
                return "jp"; // Japanese - not tested...

            count = GetCountContains(text, "是", "是早", "吧", "的", "爱", "上好");
            count += GetCountContains(text, "的", "啊", "好", "好", "亲", "的");
            count += GetCountContains(text, "谢", "走", "吧", "晚", "上", "好");
            count += GetCountContains(text, "来", "卡", "拉", "吐", "滚", "他");
            if (count > bestCount * 2)
                return "zh"; // Chinese (simplified) - not tested...

            return "en";
        }

        private static int GetCountContains(string text,
                          string word1,
                          string word2,
                          string word3,
                          string word4,
                          string word5,
                          string word6)
        {
            var regEx1 = new Regex(word1);
            var regEx2 = new Regex(word2);
            var regEx3 = new Regex(word3);
            var regEx4 = new Regex(word4);
            var regEx5 = new Regex(word5);
            var regEx6 = new Regex(word6);
            int count = regEx1.Matches(text).Count;
            count += regEx2.Matches(text).Count;
            count += regEx3.Matches(text).Count;
            count += regEx4.Matches(text).Count;
            count += regEx5.Matches(text).Count;
            count += regEx6.Matches(text).Count;
            return count;
        }

        public static string AutoDetectGoogleLanguage(string text)
        {
            string languageId = AutoDetectGoogleLanguage(text, 10);

            if (string.IsNullOrEmpty(languageId))
                return "en";

            return languageId;
        }
    }
}