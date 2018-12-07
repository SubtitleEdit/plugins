using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.Forms.UI.Controls;
using WebViewTranslate.Logic;

namespace WebViewTranslate.Translator
{
    /// <summary>
    /// Google translate screen scraper
    /// </summary>
    public class GoogleScreenScraper2 
    {
        private readonly WebView _webView;
        private string _translateResult;
        private bool _loaded;
        private StringBuilder _log;
        private string _googleUrl;
        Formatting[] _formattings;

        public GoogleScreenScraper2(WebView webView, string googleUrl = "https://translate.google.com")
        {
            if (webView == null)
                return;

            _googleUrl = googleUrl.TrimEnd('/');
            _webView = webView;
            _webView.NavigationCompleted += _webView_NavigationCompleted;
            _webView.DOMContentLoaded += _webView_DOMContentLoaded;
        }

        private void _webView_DOMContentLoaded(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlDOMContentLoadedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            _loaded = true;
            _log?.AppendLine("_webView_DOMContentLoaded called");
            string html = _webView.InvokeScript("eval", "document.documentElement.outerHTML;");
            SetTranslatedText(html);
        }

        private void _webView_NavigationCompleted(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationCompletedEventArgs e)
        {
           // _log?.AppendLine("_webView_NavigationCompleted called" + Environment.NewLine);            
        }

        public List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("AFRIKAANS", "af"),
                new TranslationPair("ALBANIAN", "sq"),
                new TranslationPair("AMHARIC", "am"),
                new TranslationPair("ARABIC", "ar"),
                new TranslationPair("ARMENIAN", "hy"),
                new TranslationPair("AZERBAIJANI", "az"),
                new TranslationPair("BASQUE", "eu"),
                new TranslationPair("BELARUSIAN", "be"),
                new TranslationPair("BENGALI", "bn"),
                new TranslationPair("BOSNIAN", "bs"),
                new TranslationPair("BULGARIAN", "bg"),
                new TranslationPair("BURMESE", "my"),
                new TranslationPair("CATALAN", "ca"),
                new TranslationPair("CEBUANO", "ceb"),
                new TranslationPair("CHICHEWA", "ny"),
                new TranslationPair("CHINESE", "zh"),
                new TranslationPair("CHINESE_SIMPLIFIED", "zh-CN"),
                new TranslationPair("CHINESE_TRADITIONAL", "zh-TW"),
                new TranslationPair("CORSICAN", "co"),
                new TranslationPair("CROATIAN", "hr"),
                new TranslationPair("CZECH", "cs"),
                new TranslationPair("DANISH", "da"),
                new TranslationPair("DUTCH", "nl"),
                new TranslationPair("ENGLISH", "en"),
                new TranslationPair("ESPERANTO", "eo"),
                new TranslationPair("ESTONIAN", "et"),
                new TranslationPair("FILIPINO", "tl"),
                new TranslationPair("FINNISH", "fi"),
                new TranslationPair("FRENCH", "fr"),
                new TranslationPair("FRISIAN", "fy"),
                new TranslationPair("GALICIAN", "gl"),
                new TranslationPair("GEORGIAN", "ka"),
                new TranslationPair("GERMAN", "de"),
                new TranslationPair("GREEK", "el"),
                new TranslationPair("GUJARATI", "gu"),
                new TranslationPair("HAITIAN CREOLE", "ht"),
                new TranslationPair("HAUSA", "ha"),
                new TranslationPair("HAWAIIAN", "haw"),
                new TranslationPair("HEBREW", "iw"),
                new TranslationPair("HINDI", "hi"),
                new TranslationPair("HMOUNG", "hmn"),
                new TranslationPair("HUNGARIAN", "hu"),
                new TranslationPair("ICELANDIC", "is"),
                new TranslationPair("IGBO", "ig"),
                new TranslationPair("INDONESIAN", "id"),
                new TranslationPair("IRISH", "ga"),
                new TranslationPair("ITALIAN", "it"),
                new TranslationPair("JAPANESE", "ja"),
                new TranslationPair("JAVANESE", "jw"),
                new TranslationPair("KANNADA", "kn"),
                new TranslationPair("KAZAKH", "kk"),
                new TranslationPair("KHMER", "km"),
                new TranslationPair("KOREAN", "ko"),
                new TranslationPair("KURDISH", "ku"),
                new TranslationPair("KYRGYZ", "ky"),
                new TranslationPair("LAO", "lo"),
                new TranslationPair("LATIN", "la"),
                new TranslationPair("LATVIAN", "lv"),
                new TranslationPair("LITHUANIAN", "lt"),
                new TranslationPair("LUXEMBOURGISH", "lb"),
                new TranslationPair("MACEDONIAN", "mk"),
                new TranslationPair("MALAY", "ms"),
                new TranslationPair("MALAGASY", "mg"),
                new TranslationPair("MALAYALAM", "ml"),
                new TranslationPair("MALTESE", "mt"),
                new TranslationPair("MAORI", "mi"),
                new TranslationPair("MARATHI", "mr"),
                new TranslationPair("MONGOLIAN", "mn"),
                new TranslationPair("MYANMAR", "my"),
                new TranslationPair("NEPALI", "ne"),
                new TranslationPair("NORWEGIAN", "no"),
                new TranslationPair("PASHTO", "ps"),
                new TranslationPair("PERSIAN", "fa"),
                new TranslationPair("POLISH", "pl"),
                new TranslationPair("PORTUGUESE", "pt"),
                new TranslationPair("PUNJABI", "pa"),
                new TranslationPair("ROMANIAN", "ro"),
                new TranslationPair("ROMANJI", "romanji"),
                new TranslationPair("RUSSIAN", "ru"),
                new TranslationPair("SAMOAN", "sm"),
                new TranslationPair("SCOTS GAELIC", "gd"),
                new TranslationPair("SERBIAN", "sr"),
                new TranslationPair("SESOTHO", "st"),
                new TranslationPair("SHONA", "sn"),
                new TranslationPair("SINDHI", "sd"),
                new TranslationPair("SINHALA", "si"),
                new TranslationPair("SLOVAK", "sk"),
                new TranslationPair("SLOVENIAN", "sl"),
                new TranslationPair("SOMALI", "so"),
                new TranslationPair("SPANISH", "es"),
                new TranslationPair("SUNDANESE", "su"),
                new TranslationPair("SWAHILI", "sw"),
                new TranslationPair("SWEDISH", "sv"),
                new TranslationPair("TAJIK", "tg"),
                new TranslationPair("TAMIL", "ta"),
                new TranslationPair("TELUGU", "te"),
                new TranslationPair("THAI", "th"),
                new TranslationPair("TURKISH", "tr"),
                new TranslationPair("UKRAINIAN", "uk"),
                new TranslationPair("URDU", "ur"),
                new TranslationPair("UZBEK", "uz"),
                new TranslationPair("VIETNAMESE", "vi"),
                new TranslationPair("WELSH", "cy"),
                new TranslationPair("XHOSA", "xh"),
                new TranslationPair("YIDDISH", "yi"),
                new TranslationPair("YORUBA", "yo"),
                new TranslationPair("ZULU", "zu"),
            };
        }

        public string GetName()
        {
            return "Google Screen-Scraper translate";
        }

        public string GetUrl()
        {
            return _googleUrl;
        }

        public void Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            _translateResult = null;
            _log = log;
            var baseUrl = GetUrl();
            var input = new StringBuilder();
            _formattings = new Formatting[paragraphs.Count];
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var p = paragraphs[index];
                var f = new Formatting();
                _formattings[index] = f;
                if (input.Length > 0)
                    input.Append(" | ");
                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                input.Append(Uri.EscapeDataString(text));
            }
            // https://translate.google.com/?hl=en&eotf=1&sl=en&tl=da&q=How%20are%20you
            // or perhaps https://translate.google.com/#view=home&op=translate&sl=da&tl=en&text=Surkål
            string uri = $"{baseUrl}/?hl=en&eotf=1&sl={sourceLanguage}&tl={targetLanguage}&q={input}";
            log.AppendLine("GET Request: " + uri + Environment.NewLine);
            _loaded = false;
            _webView.Source = null;
            _translateResult = null;
            _webView.Navigate(new Uri(uri));
        }

        public List<string> GetTranslationResult(string targetLanguage, int count)
        {
            if (!_loaded)
                return null;

            string html = _webView.InvokeScript("eval", "document.documentElement.outerHTML;");
            _log?.AppendLine("Translate 5+ seconds: " + html + Environment.NewLine);
            SetTranslatedText(html);
            if (string.IsNullOrEmpty(_translateResult))
                return null;
            return MakeList(_translateResult, targetLanguage, _formattings, count);
        }

        private List<string> MakeList(string res, string targetLanguage, Formatting[] formattings, int count)
        {
            res = Regex.Unescape(res);
            var lines = res.Split('|').ToList();
            var resultList = new List<string>();
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index].Trim();
                var s = WebUtility.HtmlDecode(line);
                s = s.Replace("<I>", "<i>");
                s = s.Replace("< i >", "<i>");
                s = s.Replace("</I>", "</i>");
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
                resultList.Add( s.Replace("  ", " "));
            }

            if (resultList.Count > count)
            {
                resultList = resultList.Where(p => !string.IsNullOrEmpty(p)).ToList();
            }

            return resultList;
        }

        private void SetTranslatedText(string html)
        {
            if (string.IsNullOrEmpty(html) || _translateResult != null)
                return;

            //      log.AppendLine("GET Response: " + uri + Environment.NewLine + "---------------------" + Environment.NewLine);

            string startTag = "class=\"tlid-translation translation";
            int start = html.IndexOf(startTag, StringComparison.Ordinal);
            if (start < 0)
                return;

            start = html.IndexOf(">", start, StringComparison.Ordinal);
            if (start < 0)
                return;

            var sb = new StringBuilder();
            int level = 0;
            bool textOn = true;
            for (int i = start + 1; i < html.Length - 5; i++)
            {
                var ch = html[i];
                if (!textOn && ch == '>')
                {
                    textOn = true;
                    sb.Append(" ");
                }
                else if (textOn && ch == '<')
                {
                    textOn = false;
                    if (html[i + 1] == '/')
                    {
                        level--;
                    }
                    else
                    {
                        level++;
                    }
                }
                else if (!textOn && html.Substring(i, 4).Replace(" ", string.Empty).StartsWith("/>", StringComparison.Ordinal))
                {
                    level--;
                }
                else if (textOn)
                {
                    sb.Append(ch);
                }

                if (level < 0)
                    break;
            }
            _translateResult = sb.ToString().Replace("  ", " ").Trim();
        }

    }
}
