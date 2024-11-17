using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace SubtitleEdit
{

    public class ReversoTranslator : ITranslator
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string GetName()
        {
            return "reverso";
        }

        public string GetUrl()
        {
            return "http://reverso.net/";
        }

        public List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                new TranslationPair { Code = "en", Name = "English", IsoCode = "en" },
                new TranslationPair { Code = "fr", Name = "French", IsoCode = "fr" },
                new TranslationPair { Code = "es", Name = "Spanish", IsoCode = "es" },
                new TranslationPair { Code = "de", Name = "German", IsoCode = "de" },
                new TranslationPair { Code = "nl", Name = "Dutch", IsoCode = "nl" },
                new TranslationPair { Code = "it", Name = "Italian", IsoCode = "it" },
                new TranslationPair { Code = "pt", Name = "Portuguese", IsoCode = "pt" },
                new TranslationPair { Code = "ru", Name = "Russian", IsoCode = "ru" },
                new TranslationPair { Code = "ro", Name = "Romanian", IsoCode = "ro" },
                new TranslationPair { Code = "zh", Name = "Chinese", IsoCode = "zh" },
                new TranslationPair { Code = "ja", Name = "Japanese", IsoCode = "ja" },
//                new TranslationPair { Code = "vt", Name = "Vietnamese", IsoCode = "vt" },
                new TranslationPair { Code = "ar", Name = "Arabic", IsoCode = "zh" },
                new TranslationPair { Code = "he", Name = "Hebrew", IsoCode = "he" },
            };
        }

        private string GetDirection(string sourceLangauge, string targetLanguage)
        {
            if (sourceLangauge == "en")
            {
                switch (targetLanguage)
                {
                    case "ar":
                        return "eng-ara-5";
                    case "zh":
                        return "eng-chi-5";
                    case "nl":
                        return "eng-dut-5";
                    case "fr":
                        return "524289";
                    case "de":
                        return "262145";
                    case "he":
                        return "en-he-2";
                    case "es":
                        return "2097153";
                    case "pt":
                        return "1033-2070-2";
                    case "it":
                        return "en-it-2";
                    case "ru":
                        return "1033-1049-3";
                    case "ro":
                        return "eng-rum-5";
                    case "ja":
                        return "1033-1041-2";
                }
            }
            else if (sourceLangauge == "fr")
            {
                switch (targetLanguage)
                {
                    case "he":
                        return "1036-1037-2";
                    case "en":
                        return "65544";
                    case "it":
                        return "fr-it-2";
                    case "zh":
                        return "fra-chi-5";
                    case "de":
                        return "262152";
                    case "es":
                        return "2097160";
                    case "ja":
                        return "1036-1041-2";
                    case "ro":
                        return "1036-1041-2";
                    case "ru":
                        return "1036-1049-2";
                    case "ar":
                        return "fra-ara-5";
                    case "pt":
                        return "1036-2070-2";
                    case "nl":
                        return "fra-dut-5";
                }
            }
            else if (sourceLangauge == "zh")
            {
                switch (targetLanguage)
                {
                    case "he":
                        return "chi-heb-5";
                    case "en":
                        return "chi-eng-5";
                    case "it":
                        return "chi-ita-5";
                    case "fr":
                        return "chi-fra-5";
                    case "de":
                        return "chi-ger-5";
                    case "es":
                        return "chi-spa-5";
                    case "ja":
                        return "chi-jpn-5";
                    case "ro":
                        return "chi-rum-5";
                    case "ru":
                        return "chi-rus-5";
                    case "ar":
                        return "chi-ara-5";
                    case "pt":
                        return "chi-por-5";
                    case "nl":
                        return "chi-dut-5";
                }
            }
            else if (sourceLangauge == "es")
            {
                switch (targetLanguage)
                {
                    case "he":
                        return "1034-1037-2";
                    case "en":
                        return "65568";
                    case "it":
                        return "1034-1040-2";
                    case "fr":
                        return "524320";
                    case "de":
                        return "1034-1031-2";
                    case "zh":
                        return "spa-chi-5";
                    case "ja":
                        return "spa-jpn-5";
                    case "ro":
                        return "spa-rum-5";
                    case "ru":
                        return "1034-1049-2";
                    case "ar":
                        return "spa-ara-5";
                    case "pt":
                        return "1034-2070-2";
                    case "nl":
                        return "spa-dut-5";
                }
            }
            else if (sourceLangauge == "it")
            {
                switch (targetLanguage)
                {
                    case "he":
                        return "ita-heb-5";
                    case "en":
                        return "1040-1033-2";
                    case "es":
                        return "1034-1040-2";
                    case "fr":
                        return "524320";
                    case "de":
                        return "1034-1031-2";
                    case "zh":
                        return "ita-chi-5";
                    case "ja":
                        return "ita-jpn-5";
                    case "ro":
                        return "ita-rum-5";
                    case "ru":
                        return "1040-1049-2";
                    case "ar":
                        return "ita-ara-5";
                    case "pt":
                        return "ita-por-5";
                    case "nl":
                        return "ita-dut-5";
                }
            }
            else if (sourceLangauge == "de")
            {
                switch (targetLanguage)
                {
                    case "he":
                        return "ger-heb-5";
                    case "en":
                        return "65540";
                    case "es":
                        return "1031-1034-2";
                    case "fr":
                        return "524292";
                    case "it":
                        return "ger-ita-5";
                    case "zh":
                        return "ger-chi-5";
                    case "ja":
                        return "ger-jpn-5";
                    case "ro":
                        return "ger-rum-5";
                    case "ru":
                        return "1031-1049-2";
                    case "ar":
                        return "ger-ara-5";
                    case "pt":
                        return "ger-por-5";
                    case "nl":
                        return "ger-dut-5";
                }
            }
            else if (sourceLangauge == "ru")
            {
                switch (targetLanguage)
                {
                    case "he":
                        return "ger-heb-5";
                    case "en":
                        return "1049-1033-3";
                    case "es":
                        return "1049-1034-2";
                    case "fr":
                        return "1049-1036-2";
                    case "it":
                        return "1049-1040-2";
                    case "zh":
                        return "rus-chi-5";
                    case "ja":
                        return "1049-1041-2";
                    case "ro":
                        return "rus-rum-5";
                    case "de":
                        return "1049-1031-2";
                    case "ar":
                        return "rus-ara-5";
                    case "pt":
                        return "rus-por-5";
                    case "nl":
                        return "rus-dut-5";
                }
            }
            else if (sourceLangauge == "pt")
            {
                switch (targetLanguage)
                {
                    case "he":
                        return "por-heb-5";
                    case "en":
                        return "2070-1033-2";
                    case "es":
                        return "2070-1034-2";
                    case "fr":
                        return "2070-1036-2";
                    case "it":
                        return "por-ita-5";
                    case "zh":
                        return "por-chi-5";
                    case "ja":
                        return "por-jpn-5";
                    case "ro":
                        return "por-rum-5";
                    case "de":
                        return "2070-1031-2";
                    case "ar":
                        return "por-ara-5";
                    case "ru":
                        return "por-rus-5";
                    case "nl":
                        return "por-dut-5";
                }
            }
            else if (sourceLangauge == "jp")
            {
                switch (targetLanguage)
                {
                    case "he":
                        return "jpn-heb-5";
                    case "en":
                        return "1041-1033-2";
                    case "es":
                        return "jpn-spa-5";
                    case "fr":
                        return "1041-1036-2";
                    case "it":
                        return "jpn-ita-5";
                    case "zh":
                        return "jpn-chi-5";
                    case "pt":
                        return "jpn-por-5";
                    case "ro":
                        return "jpn-rum-5";
                    case "de":
                        return "jpn-ger-5";
                    case "ar":
                        return "jpn-ara-5";
                    case "ru":
                        return "jpn-rus-5";
                    case "nl":
                        return "jpn-dut-5";
                }
            }
            else if (sourceLangauge == "he")
            {
                switch (targetLanguage)
                {
                    case "jp":
                        return "heb-jpn-5";
                    case "en":
                        return "1041-1033-2";
                    case "es":
                        return "heb-spa-5";
                    case "fr":
                        return "1041-1036-2";
                    case "it":
                        return "heb-ita-5";
                    case "zh":
                        return "heb-chi-5";
                    case "pt":
                        return "heb-por-5";
                    case "ro":
                        return "heb-rum-5";
                    case "de":
                        return "heb-ger-5";
                    case "ar":
                        return "heb-ara-5";
                    case "ru":
                        return "jpn-rus-5";
                    case "nl":
                        return "heb-dut-5";
                }
            }
            else if (sourceLangauge == "ar")
            {
                switch (targetLanguage)
                {
                    case "jp":
                        return "ara-jpn-5";
                    case "en":
                        return "ara-eng-5";
                    case "es":
                        return "ara-spa-5";
                    case "fr":
                        return "ara-fra-5";
                    case "it":
                        return "ara-ita-5";
                    case "zh":
                        return "ara-chi-5";
                    case "pt":
                        return "ara-por-5";
                    case "ro":
                        return "ara-rum-5";
                    case "de":
                        return "ara-ger-5";
                    case "he":
                        return "ara-ara-5";
                    case "ru":
                        return "ara-rus-5";
                    case "nl":
                        return "ara-dut-5";
                }
            }
            else if (sourceLangauge == "ro")
            {
                switch (targetLanguage)
                {
                    case "jp":
                        return "rum-jpn-5";
                    case "en":
                        return "rum-eng-5";
                    case "es":
                        return "rum-spa-5";
                    case "fr":
                        return "rum-fra-5";
                    case "it":
                        return "rum-ita-5";
                    case "zh":
                        return "rum-chi-5";
                    case "pt":
                        return "rum-por-5";
                    case "ar":
                        return "rum-ara-5";
                    case "de":
                        return "rum-ger-5";
                    case "he":
                        return "rum-ara-5";
                    case "ru":
                        return "rum-rus-5";
                    case "nl":
                        return "rum-dut-5";
                }
            }

            return "262145";
        }

        public string Translate(string sourceLanguage, string targetLanguage, string text, StringBuilder log)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var pairs = GetTranslationPairs();
            var direction1 = pairs.First(p => p.Code == sourceLanguage).Name.ToLowerInvariant() + "-" +
                             pairs.First(p => p.Code == sourceLanguage).Name.ToLowerInvariant();
            var direction2 = GetDirection(sourceLanguage, targetLanguage);

            string url = "http://www.reverso.net/WebReferences/WSAJAXInterface.asmx/TranslateWS";
            var handler = new HttpClientHandler { UseCookies = false };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:62.0) Gecko/20100101 Firefox/62.0");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            client.DefaultRequestHeaders.Add("Referer", "http://www.reverso.net/translationresults.aspx?lang=EN&direction=" + direction1);
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("Cookie", "reverso.net.LanguageInterface=en; reverso.net.autoHome=1; cookiescriptaccept=visit; reverso.net.dir=262145; ASP.NET_SessionId=rs4ij3yswtkrkjtivzgiqfhu");
            client.DefaultRequestHeaders.Add("Host", "www.reverso.net");
            var content = new StringContent(@"{'searchText': '" + text.Replace("'", "\\'") + "','direction': '" + direction2 + "', 'maxTranslationChars':'-1'}", Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            var res = client.SendAsync(request).Result;

            if (res.IsSuccessStatusCode)
            {
                string s = res.Content.ReadAsStringAsync().Result;
                log.AppendLine("Result from Reverso: " + s);
                var tag = "result\":";
                var idx = s.IndexOf(tag, StringComparison.Ordinal);
                if (idx > 0)
                {
                    var sb = new StringBuilder();
                    if (idx > 0)
                    {
                        s = s.Substring(idx + tag.Length).TrimStart().TrimStart('"');
                        idx = s.IndexOf('"');
                        if (idx > 0)
                        {
                            sb.AppendLine(s.Substring(0, idx) + Environment.NewLine);
                        }
                    }
                    s = sb.ToString().TrimEnd();
                    s = Uri.UnescapeDataString(s);
                    s = System.Text.RegularExpressions.Regex.Unescape(s);
                    s = WebUtility.HtmlDecode(s);
                }
                log.AppendLine();
                return s;
            }
            else
            {
                try
                {
                    log.AppendLine("Status code: " + res.StatusCode);
                    log.AppendLine("Error result from Papago: " + res.Content.ReadAsStringAsync().Result);
                }
                catch (Exception ex)
                {
                    log.AppendLine(ex.Message + ": " + ex.StackTrace);
                }
            }
            log.AppendLine();
            return null;
        }
    }
}
