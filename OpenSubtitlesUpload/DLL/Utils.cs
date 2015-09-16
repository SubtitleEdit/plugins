using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic
{
    public static class Utils
    {
        #region String Extensions

        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static bool Contains(this string s, char c)
        {
            return s.Length > 0 && s.IndexOf(c) >= 0;
        }

        public static string[] SplitToLines(this string s)
        {
            return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }

        #endregion

        private static int GetCount(string text, params string[] words)
        {
            int count = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var regEx = new Regex("\\b" + words[i] + "\\b");
                count += regEx.Matches(text).Count;
            }
            return count;
        }

        private static int CountTagInText(string text, string tag)
        {
            var idx = text.IndexOf(tag, StringComparison.Ordinal);
            var count = 0;
            while (idx >= 0)
            {
                count++;
                idx = text.IndexOf(tag, idx + tag.Length, StringComparison.Ordinal);
            }
            return count;
        }

        public static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            int count = GetCount(text, "we", "are", "and", "you", "your", "what");
            if (count > bestCount)
                return "en";

            count = GetCount(text, "vi", "han", "og", "jeg", "var", "men", "gider", "bliver", "virkelig", "kommer", "tilbage", "Hej");
            if (count > bestCount)
            {
                int norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                int dutchCount = GetCount(text, "van", "het", "een", "Het", "mij", "zijn");
                if (norwegianCount < 2 && dutchCount < count)
                    return "da";
            }

            count = GetCount(text, "vi", "er", "og", "jeg", "var", "men");
            if (count > bestCount)
            {
                int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                int dutchCount = GetCount(text, "van", "het", "een", "Het", "mij", "zijn");
                if (danishCount < 2 && dutchCount < count)
                    return "no";
            }

            count = GetCount(text, "vi", "är", "och", "Jag", "inte", "för");
            if (count > bestCount)
                return "sv";

            count = GetCount(text, "el", "bien", "Vamos", "Hola", "casa", "con");
            if (count > bestCount)
            {
                int frenchWords = GetCount(text, "C'est", "c'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
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

            count = GetCount(text, "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo", "ragazzi", "propriamente", "numero", "hanno", "giorno", "faccio",
                                   "davvero", "negativo", "essere", "vuole", "sensitivo", "venire");

            if (count > bestCount)
            {
                int frenchWords = GetCount(text, "C'est", "c'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
                int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                if (frenchWords < 2 && spanishWords < 2)
                    return "it";
            }

            count = GetCount(text, "não", "Não", "Estás", "Então", "isso", "com");
            if (count > bestCount)
                return "pt"; // Portuguese

            count = GetCount(text, "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά", "Ενταξει", "Ενταξει", "πρεπει", "Λοιπον", "τιποτα", "ξερεις");
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

                int countRo = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau", "trãiascã", "niciodatã", "înseamnã", "vorbesti", "oamenii", "Asteaptã",
                                             "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
                if (countRo > count)
                    return "ro"; // Romanian

                count = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie", "nevoie", "decat", "echilibrul", "vorbesti", "oamenii", "zeului",
                                       "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");
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

            count = GetCount(text, "için", "Tamam", "Hayır", "benim", "daha", "deðil", "önce", "lazým", "benim", "çalýþýyor", "burada", "efendim");
            if (count > bestCount)
                return "tr"; // Turkish

            count = GetCount(text, "yang", "tahu", "bisa", "akan", "tahun", "tapi", "dengan", "untuk", "rumah", "dalam", "sudah", "bertemu");
            if (count > bestCount)
                return "id"; // Indonesian

            count = GetCount(text, "โอ", "โรเบิร์ต", "วิตตอเรีย", "ดร", "คุณตำรวจ", "ราเชล", "ไม่", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์");
            if (count > 10 || count > bestCount)
                return "th"; // Thai

            count = GetCount(text, "그리고", "아니야", "하지만", "말이야", "그들은", "우리가");
            if (count > 10 || count > bestCount)
                return "ko"; // Korean

            count = GetCount(text, "että", "kuin", "minä", "mitään", "Mutta", "siitä", "täällä", "poika", "Kiitos", "enää", "vielä", "tässä");
            if (count > bestCount)
                return "fi"; // Finnish

            count = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau", "trãiascã", "niciodatã", "înseamnã", "vorbesti", "oamenii", "Asteaptã",
                                   "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
            if (count > bestCount)
                return "ro"; // Romanian

            count = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie", "nevoie", "decat", "echilibrul", "vorbesti", "oamenii", "zeului",
                                   "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");
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

        private static int GetCountContains(string text, params string[] words)
        {
            if (words == null || words.Length == 0)
                return 0;
            var count = 0;
            for (int i = 0; i < words.Length; i++)
            {
                count += CountTagInText(text, words[i]);
            }
            return count;
        }

        public static string AutoDetectGoogleLanguage(string text)
        {
            string languageId = AutoDetectGoogleLanguage(text, 10);

            if (string.IsNullOrEmpty(languageId))
                return "en";

            return languageId;
        }

        public static string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "OpenSubtitles.xml");
        }

        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.Unicode.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return Encoding.Unicode.GetString(encodedDataAsBytes);
        }
    }
}