using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using PromptTranslate.Logic;

namespace PromptTranslate.Translator
{
    /// <summary>
    /// Google translate screen scraper
    /// </summary>
    public class PromptTranslator
    {
        private string _translateResult;
        private StringBuilder _log;
        private readonly string _translateUrl;
        private Formatting[] _formattings;

        public PromptTranslator(string source, string target)
        {
            _translateUrl = "https://www.online-translator.com/";
        }

        public static List<TranslationPair> GetTranslationPairs()
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
            };
        }

        public string GetName()
        {
            return "Prompt Screen-Scraper translate";
        }

        public string GetUrl()
        {
            return _translateUrl;
        }

        private string _inputJson = null;
        public void Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            for (int i = 0; i < 10; i++)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }

            _translateResult = null;
            _log = log;
            var input = new StringBuilder();
            _formattings = new Formatting[paragraphs.Count];
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var p = paragraphs[index];
                var f = new Formatting();
                _formattings[index] = f;
                if (input.Length > 0)
                {
                    input.Append(Environment.NewLine + Environment.NewLine);
                }

                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                while (text.Contains(Environment.NewLine + Environment.NewLine))
                {
                    text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                }

                text = text.Replace(Environment.NewLine, "\n");
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = "_";
                }

                input.Append(text);
            }

            var inputText = Json.EncodeJsonText(input.ToString());
            _inputJson = "{ dirCode:'" + sourceLanguage + "-" + targetLanguage + "', template:'auto', text:'" + inputText + "', lang:'" + sourceLanguage + "', limit:'3000',useAutoDetect:true, key:'123', ts:'MainSite',tid:'', IsMobile:false}";
        }


        public List<string> GetTranslationResult(string targetLanguage, List<Paragraph> paragraphs)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var buffer = Encoding.UTF8.GetBytes(_inputJson);
                    var byteContent = new ByteArrayContent(buffer);
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = client.PostAsync("https://www.online-translator.com/services/soap.asmx/GetTranslation", byteContent).Result;
                    _translateResult = response.Content.ReadAsStringAsync().Result;
                    var parser = new JsonParser();
                    var x = (Dictionary<string, object>)parser.Parse(_translateResult);
                    foreach (Dictionary<string, object> element in x.Values)
                    {
                        foreach (var element2 in element)
                        {
                            if (element2.Key == "result")
                            {
                                _translateResult = (string)element2.Value;
                                break;
                            }
                        }
                    }
                }

                var list = MakeList(_translateResult, targetLanguage, _formattings);

                if (list.Count > paragraphs.Count)
                {
                    return list.Where(p => !string.IsNullOrEmpty(p)).ToList();
                }

                if (list.Count < paragraphs.Count)
                {
                    var splitList = SplitMergedLines(list, paragraphs);
                    if (splitList.Count == paragraphs.Count)
                    {
                        return splitList;
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private List<string> MakeList(string input, string targetLanguage, Formatting[] formattings)
        {
            var res = input.Replace("<br/>", Environment.NewLine);
            res = res.Replace("<br />", Environment.NewLine);
            res = res.Replace("<br>", Environment.NewLine);
            var lines = new List<string>();

            var sb = new StringBuilder();
            foreach (var l in res.SplitToLines())
            {
                if (string.IsNullOrWhiteSpace(l))
                {
                    if (sb.Length > 0)
                    {
                        lines.Add(sb.ToString().Trim());
                    }

                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(l);
                }
            }
            if (sb.Length > 0)
            {
                lines.Add(sb.ToString().Trim());
            }

            var resultList = new List<string>();
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index].Trim();
                var s = WebUtility.HtmlDecode(line).Replace("+", " ").Replace("  ", " ").Replace(" , ", ", ");
                s = s.Replace("<I>", "<i>");
                s = s.Replace("<I >", "<i>");
                s = s.Replace("< I >", "<i>");
                s = s.Replace("< i >", "<i>");
                s = s.Replace("</I>", "</i>");
                s = s.Replace("</I >", "</i>");
                s = s.Replace("</ I>", "</i>");
                s = s.Replace("</ i>", "</i>");
                s = s.Replace("< / i>", "</i>");
                s = string.Join(Environment.NewLine, s.SplitToLines());
                s = TranslationHelper.PostTranslate(s, targetLanguage);
                s = s.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
                s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
                s = s.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
                if (formattings.Length > index)
                {
                    s = formattings[index].ReAddFormatting(s);
                }

                resultList.Add(s.Replace("  ", " "));
            }
            return resultList;
        }

        private static List<string> SplitMergedLines(List<string> input, List<Paragraph> paragraphs)
        {
            var hits = 0;
            var results = new List<string>();
            for (var index = 0; index < input.Count; index++)
            {
                var line = input[index];
                var text = paragraphs[index].Text;
                var badPoints = 0;
                if (text.StartsWith("[") && !line.StartsWith("["))
                {
                    badPoints++;
                }

                if (text.StartsWith("-") && !line.StartsWith("-"))
                {
                    badPoints++;
                }

                if (text.Length > 0 && char.IsUpper(text[0]) && line.Length > 0 && !char.IsUpper(line[0]))
                {
                    badPoints++;
                }

                if (text.EndsWith(".") && !line.EndsWith("."))
                {
                    badPoints++;
                }

                if (text.EndsWith("!") && !line.EndsWith("!"))
                {
                    badPoints++;
                }

                if (text.EndsWith("?") && !line.EndsWith("?"))
                {
                    badPoints++;
                }

                if (text.EndsWith(",") && !line.EndsWith(","))
                {
                    badPoints++;
                }

                if (text.EndsWith(":") && !line.EndsWith(":"))
                {
                    badPoints++;
                }

                var added = false;
                if (badPoints > 0 && hits + input.Count < paragraphs.Count)
                {
                    var percent = line.Length * 100.0 / text.Length;
                    if (percent > 150)
                    {
                        var temp = Utilities.AutoBreakLine(line).SplitToLines();
                        if (temp.Length == 2)
                        {
                            hits++;
                            results.Add(temp[0]);
                            results.Add(temp[1]);
                            added = true;
                        }
                    }
                }
                if (!added)
                {
                    results.Add(line);
                }
            }

            if (results.Count == paragraphs.Count)
            {
                return results;
            }

            return input;
        }
    }
}
