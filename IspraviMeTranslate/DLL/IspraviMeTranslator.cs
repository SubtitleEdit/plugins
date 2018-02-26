using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SubtitleEdit
{

    public class IspraviMeTranslator : ITranslator
    {
        public string Key { get; set; }

        public string GetName()
        {
            return "ISPRAVI.ME";
        }

        public string GetUrl()
        {
            return "https://ispravi.me";
        }

        public string Translate(string sourceLanguage, string targetLanguage, string text, StringBuilder log)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            string url = "https://ispravi.me/api/ispravi.pl";
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("textarea", text));
            nvc.Add(new KeyValuePair<string, string>("context", sourceLanguage));
            nvc.Add(new KeyValuePair<string, string>("key", targetLanguage));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            //client.DefaultRequestHeaders.Add("x-apigw-partnerid", "papago");
            //client.DefaultRequestHeaders.Add("device-type", "pc");
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };
            var res = client.SendAsync(request).Result;
            if (res.IsSuccessStatusCode)
            {
                string s = res.Content.ReadAsStringAsync().Result;
                log.AppendLine("Result from " + GetName() + ": " + s);
                var tag = "translatedText\":";
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
                }
                log.AppendLine();
                return s;
            }
            else
            {
                try
                {
                    log.AppendLine("Status code: " + res.StatusCode);
                    log.AppendLine("Error result from " + GetName() + ": " + res.Content.ReadAsStringAsync().Result);
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
