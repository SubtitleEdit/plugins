using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Utilities
    {
        public static string UppercaseLetters = GetLetters(true, false, false);
        public static string LowercaseLetters = GetLetters(false, true, false);
        public static string LowercaseLettersWithNumbers = GetLetters(false, true, false);
        public static string AllLetters = GetLetters(true, true, false);
        public static string AllLettersAndNumbers = GetLetters(true, true, false);

        //"ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑÎ";
        private static string GetLetters(bool uppercase, bool lowercase, bool numbers)
        {
            var sb = new StringBuilder();

            if (uppercase)
                sb.Append("ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑÎ");

            if (lowercase)
                sb.Append("abcdefghijklmnopqrstuvwzyxæøãåäöéèáâàçêíóôõúłабвгдеёжзийклмнопрстуфхцчшщъыьэюяğişüùáìñî");

            if (numbers)
                sb.Append("0123456789");

            return sb.ToString();
        }

        internal static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static bool IsInteger(string s)
        {
            int i;
            if (int.TryParse(s, out i))
                return true;
            return false;
        }

        public static string RemoveHtmlFontTag(string s)
        {
            s = s.Replace("</font>", string.Empty);
            s = s.Replace("</FONT>", string.Empty);
            s = s.Replace("</Font>", string.Empty);
            s = s.Replace("<font>", string.Empty);
            s = s.Replace("<FONT>", string.Empty);
            s = s.Replace("<Font>", string.Empty);

            while (s.ToLower().Contains("<font"))
            {
                int startIndex = s.ToLower().IndexOf("<font");
                int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
                s = s.Remove(startIndex, (endIndex - startIndex) + 1);
            }
            return s;
        }

        public static string RemoveHtmlTags(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            //if (s == null)
            //    return null;
            if (!s.Contains("<"))
                return s;
            s = s.Replace("<i>", string.Empty);
            s = s.Replace("</i>", string.Empty);
            s = s.Replace("<b>", string.Empty);
            s = s.Replace("</b>", string.Empty);
            s = s.Replace("<u>", string.Empty);
            s = s.Replace("</u>", string.Empty);
            s = s.Replace("<I>", string.Empty);
            s = s.Replace("</I>", string.Empty);
            s = s.Replace("<B>", string.Empty);
            s = s.Replace("</B>", string.Empty);
            s = s.Replace("<U>", string.Empty);
            s = s.Replace("</U>", string.Empty);
            s = RemoveParagraphTag(s);
            return RemoveHtmlFontTag(s).Trim();
        }

        internal static string GetHtmlColorCode(Color color)
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        internal static string RemoveBrackets(string inputString)
        {
            string pattern = @"^[\[\{\(]|[\]\}\)]$";
            return Regex.Replace(inputString, pattern, string.Empty).Trim();
        }

        internal static string RemoveParagraphTag(string s)
        {
            s = s.Replace("</p>", string.Empty);
            s = s.Replace("</P>", string.Empty);
            s = s.Replace("<P>", string.Empty);
            s = s.Replace("<P>", string.Empty);

            while (s.ToLower().Contains("<p "))
            {
                int startIndex = s.ToLower().IndexOf("<p ");
                int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
                s = s.Remove(startIndex, (endIndex - startIndex) + 1);
            }
            return s;
        }

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

        public static string AutoDetectGoogleLanguage(Encoding encoding)
        {
            if (encoding.CodePage == 949)
                return "ko"; // Korean
            if (encoding.CodePage == 950)
                return "zh"; // Chinese
            if (encoding.CodePage == 1253)
                return "el"; // Greek
            if (encoding.CodePage == 1254)
                return "tr"; // Turkish
            if (encoding.CodePage == 1255)
                return "he"; // Hebrew
            if (encoding.CodePage == 1256)
                return "ar"; // Arabic
            if (encoding.CodePage == 1258)
                return "vi"; // Vietnamese
            if (encoding.CodePage == 1361)
                return "ko"; // Korean
            if (encoding.CodePage == 10001)
                return "ja"; // Japanese
            if (encoding.CodePage == 20000)
                return "zh"; // Chinese
            if (encoding.CodePage == 20002)
                return "zh"; // Chinese
            if (encoding.CodePage == 20932)
                return "ja"; // Japanese
            if (encoding.CodePage == 20936)
                return "zh"; // Chinese
            if (encoding.CodePage == 20949)
                return "ko"; // Korean
            if (encoding.CodePage == 28596)
                return "ar"; // Arabic
            if (encoding.CodePage == 28597)
                return "el"; // Greek
            if (encoding.CodePage == 28598)
                return "he"; // Hebrew
            if (encoding.CodePage == 28599)
                return "tr"; // Turkish
            if (encoding.CodePage == 50220)
                return "ja"; // Japanese
            if (encoding.CodePage == 50221)
                return "ja"; // Japanese
            if (encoding.CodePage == 50222)
                return "ja"; // Japanese
            if (encoding.CodePage == 50225)
                return "ko"; // Korean
            if (encoding.CodePage == 51932)
                return "ja"; // Japanese
            if (encoding.CodePage == 51936)
                return "zh"; // Chinese
            if (encoding.CodePage == 51949)
                return "ko"; // Korean
            if (encoding.CodePage == 52936)
                return "zh"; // Chinese
            if (encoding.CodePage == 54936)
                return "zh"; // Chinese
            return null;
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

            return string.Empty;
        }

        public static string AutoDetectGoogleLanguage(Subtitle subtitle)
        {
            int bestCount = subtitle.Paragraphs.Count / 14;

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);
            string text = sb.ToString();

            string languageId = AutoDetectGoogleLanguage(text, bestCount);

            if (string.IsNullOrEmpty(languageId))
                return "en";

            return languageId;
        }

        public static string AutoDetectGoogleLanguageOrNull(Subtitle subtitle)
        {
            int bestCount = subtitle.Paragraphs.Count / 14;

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);
            string text = sb.ToString();

            string languageId = AutoDetectGoogleLanguage(text, bestCount);

            if (string.IsNullOrEmpty(languageId))
                return null;

            return languageId;
        }

        //public static string AutoDetectLanguageName(string languageName, Subtitle subtitle)
        //{
        //    if (string.IsNullOrEmpty(languageName))
        //        languageName = "en_US";
        //    int bestCount = subtitle.Paragraphs.Count / 14;

        //    var sb = new StringBuilder();
        //    foreach (Paragraph p in subtitle.Paragraphs)
        //        sb.AppendLine(p.Text);
        //    string text = sb.ToString();

        //    List<string> dictionaryNames = GetDictionaryLanguages();

        //    bool containsEnGb = false;
        //    bool containsEnUs = false;
        //    foreach (string name in dictionaryNames)
        //    {
        //        if (name.Contains("[en_GB]"))
        //            containsEnGb = true;
        //        if (name.Contains("[en_US]"))
        //            containsEnUs = true;
        //    }

        //    foreach (string name in dictionaryNames)
        //    {
        //        string shortName = string.Empty;
        //        int start = name.IndexOf("[");
        //        int end = name.IndexOf("]");
        //        if (start > 0 && end > start)
        //        {
        //            start++;
        //            shortName = name.Substring(start, end - start);
        //        }

        //        int count;
        //        switch (shortName)
        //        {
        //            case "da_DK":
        //                count = GetCount(text, "vi", "er", "og", "jeg", "var", "men");
        //                if (count > bestCount)
        //                {
        //                    int norweigianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
        //                    if (norweigianCount < 2)
        //                        languageName = shortName;
        //                }
        //                break;

        //            case "nb_NO":
        //                count = GetCount(text, "vi", "er", "og", "jeg", "var", "men");
        //                if (count > bestCount)
        //                {
        //                    int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
        //                    if (danishCount < 2)
        //                        languageName = shortName;
        //                }
        //                break;

        //            case "en_US":
        //                count = GetCount(text, "we", "are", "and", "you", "your", "what");
        //                if (count > bestCount)
        //                {
        //                    if (containsEnGb)
        //                    {
        //                        int usCount = GetCount(text, "color", "flavor", "honor", "humor", "neighbor", "honor");
        //                        int gbCount = GetCount(text, "colour", "flavour", "honour", "humour", "neighbour", "honour");
        //                        if (usCount >= gbCount)
        //                            languageName = "en_US";
        //                        else
        //                            languageName = "en_GB";
        //                    }
        //                    else
        //                    {
        //                        languageName = shortName;
        //                    }
        //                }
        //                break;

        //            case "en_GB":
        //                count = GetCount(text, "we", "are", "and", "you", "your", "what");
        //                if (count > bestCount)
        //                {
        //                    if (containsEnUs)
        //                    {
        //                        int usCount = GetCount(text, "color", "flavor", "honor", "humor", "neighbor", "honor");
        //                        int gbCount = GetCount(text, "colour", "flavour", "honour", "humour", "neighbour", "honour");
        //                        if (usCount >= gbCount)
        //                            languageName = "en_US";
        //                        else
        //                            languageName = "en_GB";
        //                    }
        //                }
        //                break;

        //            case "sv_SE":
        //                count = GetCount(text, "vi", "är", "och", "Jag", "inte", "för");
        //                if (count > bestCount)
        //                    languageName = shortName;
        //                break;

        //            case "es_ES":
        //                count = GetCount(text, "el", "bien", "Vamos", "Hola", "casa", "con");
        //                if (count > bestCount)
        //                {
        //                    int frenchWords = GetCount(text, "Cest", "cest", "pas", "vous", "pour", "suis"); // not spanish words
        //                    if (frenchWords < 2)
        //                        languageName = shortName;
        //                }
        //                break;

        //            case "fr_FR":
        //                count = GetCount(text, "un", "vous", "avec", "pas", "ce", "une");
        //                if (count > bestCount)
        //                {
        //                    int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
        //                    int italianWords = GetCount(text, "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo"); // not italian words
        //                    if (spanishWords < 2 && italianWords < 2)
        //                        languageName = shortName;
        //                }
        //                break;

        //            case "it_IT":
        //                count = GetCount(text, "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo");
        //                if (count > bestCount)
        //                {
        //                    int frenchWords = GetCount(text, "Cest", "cest", "pas", "vous", "pour", "suis"); // not spanish words
        //                    int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
        //                    if (frenchWords < 2 && spanishWords < 2)
        //                        languageName = shortName;
        //                }
        //                break;

        //            case "de_DE":
        //                count = GetCount(text, "und", "auch", "sich", "bin", "hast", "möchte");
        //                if (count > bestCount)
        //                    languageName = shortName;
        //                break;

        //            case "nl_NL":
        //                count = GetCount(text, "van", "het", "een", "Het", "mij", "zijn");
        //                if (count > bestCount)
        //                    languageName = shortName;
        //                break;

        //            case "pl_PL":
        //                count = GetCount(text, "Czy", "ale", "ty", "siê", "jest", "mnie");
        //                if (count > bestCount)
        //                    languageName = shortName;
        //                break;

        //            case "el_GR":
        //                count = GetCount(text, "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά");
        //                if (count > bestCount)
        //                    languageName = shortName;
        //                break;

        //            case "ru_RU":
        //                count = GetCount(text, "все", "это", "как", "Воробей", "сюда", "Давай");
        //                if (count > bestCount)
        //                    languageName = shortName;
        //                break;

        //            case "ro_RO":
        //                count = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau") +
        //                        GetCount(text, "trãiascã", "niciodatã", "înseamnã", "vorbesti", "oamenii", "Asteaptã") +
        //                        GetCount(text, "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
        //                if (count > bestCount)
        //                {
        //                    languageName = shortName;
        //                }
        //                else
        //                {
        //                    count = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie") +
        //                            GetCount(text, "nevoie", "decat", "echilibrul", "vorbesti", "oamenii", "zeului") +
        //                            GetCount(text, "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");

        //                    if (count > bestCount)
        //                        languageName = shortName;
        //                }
        //                break;

        //            case "hr_HR": // Croatia
        //                count = GetCount(text, "sam", "öto", "äto", "ovo", "vas", "što");
        //                if (count > bestCount)
        //                    languageName = shortName;
        //                break;

        //            case "pt_PT": // Portuguese
        //                count = GetCount(text, "não", "Não", "Estás", "Então", "isso", "com");
        //                if (count > bestCount)
        //                    languageName = shortName;
        //                break;

        //            case "pt_BR": // Portuguese (Brasil)
        //                count = GetCount(text, "não", "Não", "Estás", "Então", "isso", "com");
        //                if (count > bestCount)
        //                    languageName = shortName;
        //                break;

        //            case "hu_HU": // Hungarian
        //                count = GetCount(text, "hogy", "lesz", "tudom", "vagy", "mondtam", "még");
        //                if (count > bestCount)
        //                    languageName = shortName;
        //                break;

        //            default:
        //                break;
        //        }
        //    }
        //    return languageName;
        //}
    }
}