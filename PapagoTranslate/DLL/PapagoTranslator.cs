using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SubtitleEdit
{

    public class PapagoTranslator : ITranslator
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string GetName()
        {
            return "papago";
        }

        public string GetUrl()
        {
            return "https://papago.naver.com/";
        }

        public List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                new TranslationPair { Code = "zh-CN", Name = "Chinese(simplified)", IsoCode = "zh-CN" },
                new TranslationPair { Code = "zh-TW", Name = "Chinese(traditional)", IsoCode = "zh-TW" },
                new TranslationPair { Code = "en", Name = "English", IsoCode = "en" },
                new TranslationPair { Code = "fr", Name = "French", IsoCode = "fr" },
                new TranslationPair { Code = "id", Name = "Indonesian", IsoCode = "id" },
                new TranslationPair { Code = "jp", Name = "Japanese", IsoCode = "jp" },
                new TranslationPair { Code = "ko", Name = "Korean", IsoCode = "ko" },
                new TranslationPair { Code = "es", Name = "Spanish", IsoCode = "es" },
                new TranslationPair { Code = "th", Name = "Thai", IsoCode = "th" },
                new TranslationPair { Code = "vi", Name = "Vietnamese", IsoCode = "vi" },
            };
        }

        public string Translate(string sourceLanguage, string targetLanguage, string text, StringBuilder log)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            string url = "https://openapi.naver.com/v1/papago/n2mt";
            var nvc = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("source", sourceLanguage),
                new KeyValuePair<string, string>("target", targetLanguage),
                new KeyValuePair<string, string>("text", text)
            };
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            //client.DefaultRequestHeaders.Add("x-apigw-partnerid", "papago");
            //client.DefaultRequestHeaders.Add("device-type", "pc");
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };
            request.Headers.Add("X-Naver-Client-Id", ClientId);
            request.Headers.Add("X-Naver-Client-Secret", ClientSecret);
            var res = client.SendAsync(request).Result;
            if (res.IsSuccessStatusCode)
            {
                string s = res.Content.ReadAsStringAsync().Result;
                log.AppendLine("Result from Papago: " + s);
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

        private string Encode(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

    }
}
