using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.Forms.UI.Controls;
using WebViewTranslate.Logic;
using WebViewTranslate.Translator;
using Formatting = WebViewTranslate.Translator.Formatting;

namespace DeepLScreenScraper.Translator
{
    /// <summary>
    /// Google translate screen scraper
    /// </summary>
    public class DeepLScreenScraper
    {
        private readonly WebView _webView;
        private string _translateResult;
        private bool _loaded;
        private StringBuilder _log;
        private readonly string _translateUrl;
        private Formatting[] _formattings;

        public DeepLScreenScraper(WebView webView, string source, string target)
        {
            if (webView == null)
                return;

            _translateUrl = "https://www.deepl.com/translator";
            _webView = webView;
            _webView.NavigationCompleted += _webView_NavigationCompleted;
            _webView.DOMContentLoaded += _webView_DOMContentLoaded;
            _webView.Navigate(new Uri(_translateUrl + "#" + source + "/" + target));
        }

        private void _webView_DOMContentLoaded(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlDOMContentLoadedEventArgs e)
        {
            _loaded = true;
            _log?.AppendLine("_webView_DOMContentLoaded called");
        }

        private void _webView_NavigationCompleted(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationCompletedEventArgs e)
        {
            // _log?.AppendLine("_webView_NavigationCompleted called" + Environment.NewLine);            
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
            return "DeepL Screen-Scraper translate";
        }

        public string GetUrl()
        {
            return _translateUrl;
        }

        public void Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            for (var i = 0; i < 10; i++)
            {
                if (!_loaded)
                {
                    System.Windows.Forms.Application.DoEvents();
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
                {
                    input.Append(Environment.NewLine + Environment.NewLine);
                }

                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                while (text.Contains(Environment.NewLine + Environment.NewLine))
                {
                    text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                }

                text = text.Replace(Environment.NewLine, "\n");
                input.Append(text);
            }

            _loaded = false;
            var url = _translateUrl + "#" + sourceLanguage.ToLower() + "/" + targetLanguage.ToLower() + "/" + WebUtility.UrlEncode(input.ToString());
            _webView.Navigate(new Uri(url));

            for (var i = 0; i < 100; i++)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(10);
                if (_loaded)
                {
                    break;
                }
            }
        }

        public List<string> GetTranslationResult(string targetLanguage, List<Paragraph> paragraphs)
        {
            for (var i = 0; i < 100; i++)
            {
                if (!_loaded)
                {
                    System.Threading.Thread.Sleep(10);
                    System.Windows.Forms.Application.DoEvents();
                }
            }

            try
            {
                //lmt__textarea lmt__target_textarea lmt__textarea_base_style
                _translateResult = _webView.InvokeScript("eval", "document.getElementsByClassName('lmt__target_textarea')[0].innerText");

                var html = _webView.InvokeScript("eval", "document.documentElement.outerHTML;");

                if (string.IsNullOrEmpty(html) || !html.Contains("lmt__target_textarea"))
                {
                    return null;
                }

                var startIndex = html.IndexOf("lmt__target_textarea", StringComparison.Ordinal);
                if (startIndex > 0)
                {
                    startIndex = html.IndexOf(">", startIndex, StringComparison.Ordinal);
                    if (startIndex > 0)
                    {
                        var endIndex = html.IndexOf("</textarea>", startIndex, StringComparison.Ordinal);
                        if (endIndex > 0)
                        {
                            _translateResult = html.Substring(startIndex + 1, endIndex - startIndex - 1);
                        }
                    }
                }


                if (string.IsNullOrEmpty(_translateResult))
                {
                    return null;
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
            catch 
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

            return results.Count == paragraphs.Count ? results : input;
        }
    }
}
