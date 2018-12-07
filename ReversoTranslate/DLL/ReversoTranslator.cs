using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace SubtitleEdit
{

    public class PapagoTranslator : ITranslator
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
                new TranslationPair { Code = "it", Name = "Italian", IsoCode = "it" },
                new TranslationPair { Code = "pt", Name = "Portuguese", IsoCode = "pt" },
                new TranslationPair { Code = "ru", Name = "Russian", IsoCode = "ru" },
                new TranslationPair { Code = "ro", Name = "Romanian", IsoCode = "ro" },
                new TranslationPair { Code = "zh", Name = "Chinese", IsoCode = "zh" },
                new TranslationPair { Code = "ja", Name = "Japanese", IsoCode = "cz" },
                new TranslationPair { Code = "cz", Name = "Vietnamese", IsoCode = "cz" },
                new TranslationPair { Code = "ar", Name = "Arabic", IsoCode = "zh" },
            };
        }

        public string Translate(string sourceLanguage, string targetLanguage, string text, StringBuilder log)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            string url = "http://www.reverso.net/WebReferences/WSAJAXInterface.asmx/TranslateWS";
            var handler = new HttpClientHandler { UseCookies = false };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:62.0) Gecko/20100101 Firefox/62.0");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            client.DefaultRequestHeaders.Add("Referer", "http://www.reverso.net/translationresults.aspx?lang=EN&direction=english-german");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("Cookie", "reverso.net.LanguageInterface=en; reverso.net.autoHome=1; cookiescriptaccept=visit; reverso.net.dir=262145; ASP.NET_SessionId=rs4ij3yswtkrkjtivzgiqfhu");
            client.DefaultRequestHeaders.Add("Host", "www.reverso.net");

            var content = new StringContent(@"{'searchText': '" + text.Replace("'", "\\'") + "','direction': '262145', 'maxTranslationChars':'-1'}", Encoding.UTF8, "application/json");
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
