using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SubtitleEdit.Translator
{
    /// <summary>
    /// DeepL Pro V2 translator - see https://www.deepl.com/api.html
    /// </summary>
    public class DeepLTranslator2 : ITranslator
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public DeepLTranslator2(string apiKey, string apiUrl)
        {
            _apiKey = apiKey;
            _apiUrl = apiUrl;
        }

        public List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("English", "en"),
                new TranslationPair("German", "de"),
                new TranslationPair("French", "fr"),
                new TranslationPair("Spanish", "es"),
                new TranslationPair("Italian", "it"),
                new TranslationPair("Dutch", "nl"),
                new TranslationPair("Polish", "pl"),
                new TranslationPair("Portuguese", "pt"),
                new TranslationPair("Russian", "ru"),
                new TranslationPair("Japanese", "ja"),
                new TranslationPair("Chinese", "zh"),
                new TranslationPair("Danish", "da"),
                new TranslationPair("Bulgarian", "bg"),
                new TranslationPair("Czech", "cs"),
                new TranslationPair("Estonian", "et"),
                new TranslationPair("Latvian", "lv"),
                new TranslationPair("Lithuanian", "lt"),
                new TranslationPair("Finnish", "fi"),
                new TranslationPair("Slovak", "sk"),
                new TranslationPair("Slovenian", "sl"),
                new TranslationPair("Greek", "el"),
                new TranslationPair("Romanian", "ro"),
            };
        }

        public string GetName()
        {
            return "DeepL Pro translate";
        }

        public string GetUrl()
        {
            return "https://www.deepl.com";
        }

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            var baseUrl = _apiUrl.Trim().TrimEnd('/') + "/v2/translate";
            var input = new StringBuilder();
            var formattingList = new Formatting[paragraphs.Count];
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var p = paragraphs[index];
                var f = new Formatting();
                formattingList[index] = f;
                if (input.Length > 0)
                {
                    input.Append("&");
                }

                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                input.Append("text=" + Uri.EscapeDataString(text));
            }
            // /v2/translate?text=Hallo%20Welt!&source_lang=DE&target_lang=EN&auth_key=123
            var uri = $"{baseUrl}?{input}&target_lang={targetLanguage}&source_lang={sourceLanguage}&auth_key={_apiKey}";
            log.AppendLine("GET Request: " + uri + Environment.NewLine);
            var request = WebRequest.Create(uri);
            request.ContentType = "application/json";
            request.ContentLength = 0;
            request.Method = "GET";
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            var content = reader.ReadToEnd();
            log.AppendLine("GET Response: " + uri + Environment.NewLine + "---------------------" + Environment.NewLine);
            // Example response: { "translations": [ { "detected_source_language": "DE", "text": "Hello World!" } ] }

            var resultList = new List<string>();
            var parser = new JsonParser();
            var x = (Dictionary<string, object>)parser.Parse(content);
            foreach (var k in x.Keys)
            {
                if (x[k] is List<object> mainList)
                {
                    foreach (var mainListItem in mainList)
                    {
                        if (mainListItem is Dictionary<string, object> innerDic)
                        {
                            foreach (var transItem in innerDic.Keys)
                            {
                                if (transItem == "text")
                                {
                                    var s = innerDic[transItem].ToString();
                                    var index = resultList.Count;
                                    if (formattingList.Length > index)
                                    {
                                        s = formattingList[index].ReAddFormatting(s);
                                    }

                                    resultList.Add(s);
                                }
                            }
                        }
                    }
                }
            }

            return resultList;
        }
    }
}
