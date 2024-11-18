using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CefSharp;
using CefSharp.OffScreen;
using WebViewTranslate.Logic;

namespace WebViewTranslate.Translator
{
    /// <summary>
    /// Bing translate screen scraper
    /// </summary>
    public class BingTranslateScreenScraper
    {
        private static ChromiumWebBrowser _chromeBrowser;
        private string _translateResult;
        private string _lastTranslateResult;
        private bool _loaded;
        private StringBuilder _log;
        private Formatting[] _formattings;
        private const string LanguagesUrl = "https://api.cognitive.microsofttranslator.com/languages?api-version=3.0&scope=translation";

        public BingTranslateScreenScraper(string source, string target)
        {
            if (_chromeBrowser != null)
            {
                return;
            }

            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);
            _chromeBrowser = new ChromiumWebBrowser("about:blank");
            _chromeBrowser.LoadingStateChanged += ChromeBrowserOnLoadingStateChanged;
        }

        private void ChromeBrowserOnLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            _loaded = e.IsLoading;
        }

        private static List<TranslationPair> _translationPairs;
        public static List<TranslationPair> GetTranslationPairs()
        {
            if (_translationPairs != null)
            {
                return _translationPairs;
            }

            using (var wc = new WebClient { Encoding = Encoding.UTF8 })
            {
                var json = wc.DownloadString(LanguagesUrl);
                _translationPairs = FillTranslationPairsFromJson(json);
                return _translationPairs;
            }
        }

        private static List<TranslationPair> FillTranslationPairsFromJson(string json)
        {
            var list = new List<TranslationPair>();
            var parser = new JsonParser();
            var x = (Dictionary<string, object>)parser.Parse(json);
            foreach (var k in x.Keys)
            {
                if (x[k] is Dictionary<string, object> v)
                {
                    foreach (var innerKey in v.Keys)
                    {
                        if (v[innerKey] is Dictionary<string, object> l)
                        {
                            list.Add(new TranslationPair(l["name"].ToString(), innerKey));
                        }
                    }
                }
            }
            return list;
        }

        public string GetName()
        {
            return "Bing Screen-Scraper translate";
        }

        public string GetUrl()
        {
            return "https://www.bing.com/translator";
        }

        public void Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            for (int i = 0; i < 10; i++)
            {
                if (!_loaded)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(10);
                }
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
                    input.Append(Environment.NewLine + Environment.NewLine);
                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                while (text.Contains(Environment.NewLine + Environment.NewLine))
                    text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                text = text.Replace(Environment.NewLine, "\n");
                input.Append(text);
            }
            _loaded = false;
            var url = "https://www.bing.com/translator?&from=" + sourceLanguage + "&to=" + targetLanguage + "&text=" + Uri.EscapeDataString(input.ToString());
            _chromeBrowser.Load(url);
            for (int i = 0; i < 100; i++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
                if (_loaded)
                    break;
            }
        }

        public List<string> GetTranslationResult(string targetLanguage, List<Paragraph> paragraphs)
        {
            for (int i = 0; i < 100; i++)
            {
                if (!_loaded)
                {
                    System.Threading.Thread.Sleep(10);
                    Application.DoEvents();
                }
            }

            try
            {

                //var tmpElement = _webView.Document.GetElementById("tta_output");
                string script = "document.getElementById('tta_output_ta').value;";
                var tmpElement = _chromeBrowser.EvaluateScriptAsync(script).Result;
                if (tmpElement == null)
                    throw new ArgumentException("Html element 'tta_output' not found!", "tta_output");

                if (!tmpElement.Success)
                    return null;

                var tmp = (string)tmpElement.Result;
                if (tmp == "Oversættelse" || tmp == "Translation" || tmp.EndsWith(" ...", StringComparison.Ordinal) || paragraphs.Count > 1 && tmp.SplitToLines().Length == 1)
                    return null;

                _translateResult = tmp;
                if (_translateResult == _lastTranslateResult)
                {
                    _lastTranslateResult = null;
                    return null;
                }

                if (string.IsNullOrEmpty(_translateResult))
                    return null;

                _lastTranslateResult = _translateResult;
                var list = MakeList(_translateResult, targetLanguage, _formattings);

                if (list.Count > paragraphs.Count)
                {
                    return list.Where(p => !string.IsNullOrEmpty(p)).ToList();
                }

                if (list.Count < paragraphs.Count)
                {
                    var splitList = SplitMergedLines(list, paragraphs);
                    if (splitList.Count == paragraphs.Count)
                        return splitList;
                }

                return list;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private List<string> MakeList(string res, string targetLanguage, Formatting[] formattings)
        {
            res = Regex.Unescape(res);
            var lines = new List<string>();

            var sb = new StringBuilder();
            foreach (var l in res.SplitToLines())
            {
                if (string.IsNullOrEmpty(l))
                {
                    if (sb.Length > 0)
                        lines.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(l);
                }
            }
            if (sb.Length > 0)
                lines.Add(sb.ToString().Trim());

            var resultList = new List<string>();
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index].Trim();
                var s = WebUtility.HtmlDecode(line);
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
                    s = formattings[index].ReAddFormatting(s);
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
                    badPoints++;
                if (text.StartsWith("-") && !line.StartsWith("-"))
                    badPoints++;
                if (text.Length > 0 && char.IsUpper(text[0]) && line.Length > 0 && !char.IsUpper(line[0]))
                    badPoints++;
                if (text.EndsWith(".") && !line.EndsWith("."))
                    badPoints++;
                if (text.EndsWith("!") && !line.EndsWith("!"))
                    badPoints++;
                if (text.EndsWith("?") && !line.EndsWith("?"))
                    badPoints++;
                if (text.EndsWith(",") && !line.EndsWith(","))
                    badPoints++;
                if (text.EndsWith(":") && !line.EndsWith(":"))
                    badPoints++;
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
                return results;

            return input;
        }
    }
}
