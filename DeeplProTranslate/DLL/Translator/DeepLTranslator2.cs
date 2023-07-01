using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using SubtitleEdit.Logic;

namespace SubtitleEdit.Translator
{
    /// <summary>
    /// DeepL Pro V2 translator - see https://www.deepl.com/api.html
    /// </summary>
    public class DeepLTranslator2 : ITranslator
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly string _formality;
        private readonly HttpClient _client;

        public DeepLTranslator2(string apiKey, string apiUrl, string formality)
        {
            _apiKey = apiKey;
            _apiUrl = apiUrl;

            if (string.IsNullOrEmpty(apiKey))
            {
                return;
            }

            _client = new HttpClient();
            _client.BaseAddress = new Uri(_apiUrl.Trim().TrimEnd('/'));
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "DeepL-Auth-Key " + _apiKey);
            _formality = formality ?? "default";
        }

        public List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("Bulgarian", "bg"),
                new TranslationPair("Chinese", "zh"),
                new TranslationPair("Czech", "cs"),
                new TranslationPair("Danish", "da"),
                new TranslationPair("Dutch", "nl", true),
                new TranslationPair("English", "en", true),
                new TranslationPair("Estonian", "et"),
                new TranslationPair("Finnish", "fi"),
                new TranslationPair("French", "fr", true),
                new TranslationPair("German", "de", true),
                new TranslationPair("Greek", "el"),
                new TranslationPair("Hungarian", "hu"),
                new TranslationPair("Indonesian", "fi"),
                new TranslationPair("Italian", "it", true),
                new TranslationPair("Japanese", "ja"),
                new TranslationPair("Korean", "ko"),
                new TranslationPair("Latvian", "lv"),
                new TranslationPair("Lithuanian", "lt"),
                new TranslationPair("Norwegian (Bokmål)", "nb"),
                new TranslationPair("Polish", "pl", true),
                new TranslationPair("Portuguese", "pt", true),
                new TranslationPair("Romanian", "ro"),
                new TranslationPair("Russian", "ru", true),
                new TranslationPair("Slovak", "sk"),
                new TranslationPair("Slovenian", "sl"),
                new TranslationPair("Spanish", "es", true),
                new TranslationPair("Swedish", "sv"),
                new TranslationPair("Turkish", "tr"),
                new TranslationPair("Ukranian", "uk"),
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

        public List<string> Translate(string sourceLanguage, string targetLanguage, Paragraph paragraph, StringBuilder log)
        {
            var formattingList = new Formatting[1];
            var f = new Formatting();
            formattingList[0] = f;
            var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(paragraph.Text, sourceLanguage), sourceLanguage);
            var postContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>("target_lang", targetLanguage),
                new KeyValuePair<string, string>("source_lang", sourceLanguage),
                new KeyValuePair<string, string>("formality", _formality),
            });
            var result = _client.PostAsync("/v2/translate", postContent).Result;
            var resultContent = result.Content.ReadAsStringAsync().Result;
            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new BadApiKeyException();
            }

            var resultList = new List<string>();
            var parser = new JsonParser();
            var x = (Dictionary<string, object>)parser.Parse(resultContent);
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
